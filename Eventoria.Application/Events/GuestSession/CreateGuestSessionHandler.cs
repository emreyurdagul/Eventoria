using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Domain.Entities;
using MediatR;

namespace Eventoria.Application.Events.GuestSession;

public sealed class CreateGuestSessionHandler
    : IRequestHandler<CreateGuestSessionCommand, CreateGuestSessionResult>
{
    private readonly IEventRepository _events;
    private readonly IEventGuestRepository _guests;
    private readonly IUnitOfWork _uow;
    private readonly IEventTokenService _tokens;       // sende var (Sha256Hex vs.)
    private readonly IGuestTokenService _guestTokens;  // yeni

    public CreateGuestSessionHandler(
        IEventRepository events,
        IEventGuestRepository guests,
        IUnitOfWork uow,
        IEventTokenService tokens,
        IGuestTokenService guestTokens)
    {
        _events = events;
        _guests = guests;
        _uow = uow;
        _tokens = tokens;
        _guestTokens = guestTokens;
    }

    public async Task<CreateGuestSessionResult> Handle(CreateGuestSessionCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Code)) throw new InvalidOperationException("Code is required.");
        if (string.IsNullOrWhiteSpace(cmd.InviteKey)) throw new InvalidOperationException("InviteKey is required.");
        if (string.IsNullOrWhiteSpace(cmd.DisplayName)) throw new InvalidOperationException("DisplayName is required.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            var code = cmd.Code.Trim();
            var inviteHash = _tokens.Sha256Hex(cmd.InviteKey.Trim());

            // Event’i code ile bul + aktif invite hash doğrula
            var ev = await _events.GetByCodeAsync(code, innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            var activeInvite = ev.Invites.FirstOrDefault(x => x.IsActive);
            if (activeInvite is null)
                throw new InvalidOperationException("Invite is not active.");

            // timing safe compare (senin tokens servisinde yoksa burada string equals de olur MVP için)
            if (!string.Equals(activeInvite.InviteKeyHash, inviteHash, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid invite key.");

            // Guest create (aynı isim tekrar girebilir; kısıt koymak istemiyorsan serbest)
            var guest = new EventGuest(ev.Id, cmd.DisplayName);
            await _guests.AddAsync(guest, innerCt);
            await _uow.SaveChangesAsync(innerCt);

            // Guest JWT (event-scope)
            var token = _guestTokens.CreateGuestToken(
                guestId: guest.Id,
                eventId: ev.Id,
                displayName: guest.DisplayName,
                validFor: TimeSpan.FromDays(7));

            return new CreateGuestSessionResult(
                EventId: ev.Id,
                EventTitle: ev.Title,
                GuestId: guest.Id,
                GuestToken: token
            );
        }, ct);
    }
}
