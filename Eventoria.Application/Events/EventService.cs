using System.Security.Cryptography;
using System.Text;
using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Events.Contracts;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;

namespace Eventoria.Application.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;

    public EventService(IEventRepository events, IUnitOfWork uow)
    {
        _events = events;
        _uow = uow;
    }

    public async Task<CreateEventResponse> CreateAsync(Guid creatorUserId, CreateEventRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new InvalidOperationException("Title is required.");

        // Code üretimi (kısa, URL-friendly). Çakışırsa retry.
        string code;
        do
        {
            code = GenerateCode(8);
        } while (await _events.CodeExistsAsync(code, ct));

        // InviteKey (plain) üret -> hash sakla
        var inviteKey = GenerateInviteKey(32);
        var inviteHash = Sha256(inviteKey);

        // Domain
        var ev = new Event(req.Title.Trim(), req.Description?.Trim(), req.Date, creatorUserId, code);
        ev.AddMembership(creatorUserId, EventRole.Admin);
        ev.AddInvite(inviteHash);

        await _events.AddAsync(ev, ct);
        await _uow.SaveChangesAsync(ct);

        // Not: inviteKey kullanıcıya dönülecek (QR içine koyacağız).
        // Şimdilik response'a inviteKey eklemedim; security için ayrı endpoint ile döndürmek daha iyi.
        return new CreateEventResponse(ev.Id, ev.Code);
    }

    public async Task RotateInviteAsync(Guid eventId, Guid actorUserId, CancellationToken ct)
    {
        await _uow.ExecuteInTransactionAsync(async ct =>
        {
            var ev = await _events.GetByIdAsync(eventId, ct)
                ?? throw new InvalidOperationException("Event not found.");

            var isAdmin = await _events.IsEventAdminAsync(eventId, actorUserId, ct);
            if (!isAdmin) throw new UnauthorizedAccessException("Only event admins can rotate invite.");

            var newInviteKey = GenerateInviteKey(32);
            var newHash = Sha256(newInviteKey);
            ev.AddInvite(newHash);

            // burada newInviteKey'i döndürmek isteyeceksin -> metot imzasını RotateInviteResponse yapabiliriz
            // şimdilik sadece rotate ediyoruz.
        }, ct);
    }

    public async Task JoinAsync(Guid userId, JoinEventRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code) || string.IsNullOrWhiteSpace(req.InviteKey))
            throw new InvalidOperationException("Code and InviteKey are required.");

        await _uow.ExecuteInTransactionAsync(async ct =>
        {
            var ev = await _events.GetByCodeAsync(req.Code.Trim(), ct)
                ?? throw new InvalidOperationException("Event not found.");

            var inviteHash = Sha256(req.InviteKey.Trim());

            // Aktif invite doğrulaması: ev.Invites içinde aktif olan var mı?
            var active = ev.Invites.FirstOrDefault(x => x.IsActive);
            if (active == null) throw new InvalidOperationException("Invite is not active.");

            if (!FixedTimeEquals(active.InviteKeyHash, inviteHash))
                throw new UnauthorizedAccessException("Invalid invite key.");

            ev.AddMembership(userId, EventRole.Participant);
        }, ct);
    }

    private static string GenerateCode(int length)
    {
        // base32 benzeri: okunabilir
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        return new string(chars);
    }

    private static string GenerateInviteKey(int length)
    {
        // URL-safe token (base64url)
        var bytes = RandomNumberGenerator.GetBytes(length);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private static string Sha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        // timing attack azaltır
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
