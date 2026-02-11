using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using Eventoria.Domain.Enums;
using MediatR;

namespace Eventoria.Application.Auth.GuestJoin;

public sealed class GuestJoinHandler : IRequestHandler<GuestJoinCommand, GuestAuthResponse>
{
    private readonly IEventRepository _events;
    private readonly IEventTokenService _tokens;
    private readonly IGuestIdentityService _guestIdentity;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;
    private readonly IUnitOfWork _uow;

    public GuestJoinHandler(
        IEventRepository events,
        IEventTokenService tokens,
        IGuestIdentityService guestIdentity,
        IJwtTokenService jwt,
        IRefreshTokenStore refresh,
        IUnitOfWork uow)
    {
        _events = events;
        _tokens = tokens;
        _guestIdentity = guestIdentity;
        _jwt = jwt;
        _refresh = refresh;
        _uow = uow;
    }

    public async Task<GuestAuthResponse> Handle(GuestJoinCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.EventCode)) throw new InvalidOperationException("EventCode is required.");
        if (string.IsNullOrWhiteSpace(cmd.InviteKey)) throw new InvalidOperationException("InviteKey is required.");
        if (string.IsNullOrWhiteSpace(cmd.DisplayName)) throw new InvalidOperationException("DisplayName is required.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            var ev = await _events.GetByCodeAsync(cmd.EventCode.Trim(), innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            // EventId doðrulamasý istersen:
            if (cmd.EventId != Guid.Empty && cmd.EventId != ev.Id)
                throw new InvalidOperationException("Event mismatch.");

            // Gönderilen invite key'in hash'ini hesapla
            var inviteHash = _tokens.Sha256Hex(cmd.InviteKey.Trim());

            // Tüm aktif invite'lar içinde bu hash'e sahip olan var mý kontrol et
            var matchingInvite = ev.Invites
                .Where(x => x.IsActive)
                .FirstOrDefault(x => CryptographicEquals(x.InviteKeyHash, inviteHash));

            if (matchingInvite == null)
                throw new UnauthorizedAccessException("Invalid or inactive invite key.");

            // Guest user yarat
            var guestUserId = await _guestIdentity.CreateGuestUserAsync(cmd.DisplayName.Trim(), ev.Id, innerCt);

            // Membership ekle (participant)
            ev.AddMembership(guestUserId, EventRole.Participant);

            // Token üret
            var access = await _jwt.CreateAccessTokenAsync(guestUserId, innerCt);
            var refresh = await _refresh.IssueAsync(guestUserId, innerCt);

            await _uow.SaveChangesAsync(innerCt);

            return new GuestAuthResponse(
                UserId: guestUserId,
                AccessToken: access,
                RefreshToken: refresh,
                IsGuest: true,
                EventId: ev.Id,
                DisplayName: cmd.DisplayName.Trim());
        }, ct);
    }

    private static bool CryptographicEquals(string a, string b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}
