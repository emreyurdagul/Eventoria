using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Register;

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IIdentityService _identity;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;

    public RegisterHandler(IIdentityService identity, IJwtTokenService jwt, IRefreshTokenStore refresh)
    {
        _identity = identity;
        _jwt = jwt;
        _refresh = refresh;
    }

    public async Task<AuthResponse> Handle(RegisterCommand cmd, CancellationToken ct)
    {
        var email = cmd.Request.Email.Trim().ToLowerInvariant();

        var existingId = await _identity.FindUserIdByEmailAsync(email, ct);
        if (existingId != null)
            throw new InvalidOperationException("Email already registered.");

        var (ok, err) = await _identity.CreateUserAsync(email, cmd.Request.Password, ct);
        if (!ok)
            throw new InvalidOperationException(err ?? "Register failed.");

        var userId = await _identity.FindUserIdByEmailAsync(email, ct);
        if (userId == null)
            throw new InvalidOperationException("Register failed.");

        var access = await _jwt.CreateAccessTokenAsync(userId.Value, ct);
        var refresh = await _refresh.IssueAsync(userId.Value, ct);

        return new AuthResponse(access, refresh);
    }
}
