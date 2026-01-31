using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IIdentityService _identity;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;

    public LoginHandler(IIdentityService identity, IJwtTokenService jwt, IRefreshTokenStore refresh)
    {
        _identity = identity;
        _jwt = jwt;
        _refresh = refresh;
    }

    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Request.Email.Trim().ToLowerInvariant();

        var ok = await _identity.ValidatePasswordAsync(email, cmd.Request.Password, ct);
        if (!ok)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var userId = await _identity.FindUserIdByEmailAsync(email, ct);
        if (userId == null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var access = await _jwt.CreateAccessTokenAsync(userId.Value, ct);
        var refresh = await _refresh.IssueAsync(userId.Value, ct);

        return new AuthResponse(access, refresh);
    }
}
