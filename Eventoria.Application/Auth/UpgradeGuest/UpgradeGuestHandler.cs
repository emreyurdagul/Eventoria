using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using Eventoria.Application.Auth.UpgradeGuest;
using MediatR;

namespace Eventoria.Application.Auth.UpgradeGuest;

public sealed class UpgradeGuestHandler : IRequestHandler<UpgradeCommand, AuthResponse>
{
    private readonly IGuestIdentityService _guestIdentity;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;
    private readonly IUnitOfWork _uow;

    public UpgradeGuestHandler(
        IGuestIdentityService guestIdentity,
        IJwtTokenService jwt,
        IRefreshTokenStore refresh,
        IUnitOfWork uow)
    {
        _guestIdentity = guestIdentity;
        _jwt = jwt;
        _refresh = refresh;
        _uow = uow;
    }

    public async Task<AuthResponse> Handle(UpgradeCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email?.Trim().ToLowerInvariant();
        var password = cmd.Password;

        if (cmd.CurrentUserId == Guid.Empty) throw new UnauthorizedAccessException("Unauthorized.");
        if (string.IsNullOrWhiteSpace(email)) throw new InvalidOperationException("Email is required.");
        if (string.IsNullOrWhiteSpace(password)) throw new InvalidOperationException("Password is required.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            var (ok, err) = await _guestIdentity.UpgradeGuestAsync(cmd.CurrentUserId, email!, password, innerCt);
            if (!ok) throw new InvalidOperationException(err ?? "Upgrade failed.");

            var access = await _jwt.CreateAccessTokenAsync(cmd.CurrentUserId, innerCt);
            var refresh = await _refresh.IssueAsync(cmd.CurrentUserId, innerCt);

            await _uow.SaveChangesAsync(innerCt);

            return new AuthResponse(access, refresh);
        }, ct);
    }
}
