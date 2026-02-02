using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.GoogleLogin;

public sealed class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, AuthResponse>
{
    private readonly IExternalIdentityService _externalIdentity;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;

    public GoogleLoginHandler(
        IExternalIdentityService externalIdentity,
        IJwtTokenService jwt,
        IRefreshTokenStore refresh)
    {
        _externalIdentity = externalIdentity;
        _jwt = jwt;
        _refresh = refresh;
    }

    public async Task<AuthResponse> Handle(GoogleLoginCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.ProviderKey))
            throw new InvalidOperationException("ProviderKey is required.");

        if (string.IsNullOrWhiteSpace(cmd.Email))
            throw new InvalidOperationException("Email is required.");

        var userId = await _externalIdentity.GetOrCreateUserFromGoogleAsync(
            providerKey: cmd.ProviderKey,
            email: cmd.Email.Trim().ToLowerInvariant(),
            firstName: cmd.FirstName,
            lastName: cmd.LastName,
            ct: ct);

        var access = await _jwt.CreateAccessTokenAsync(userId, ct);
        var refresh = await _refresh.IssueAsync(userId, ct);

        return new AuthResponse(access, refresh);
    }
}
