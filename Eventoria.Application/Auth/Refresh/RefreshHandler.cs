using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using MediatR;

namespace Eventoria.Application.Auth.Refresh;

public sealed class RefreshHandler : IRequestHandler<RefreshCommand, AuthResponse>
{
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;

    public RefreshHandler(IJwtTokenService jwt, IRefreshTokenStore refresh)
    {
        _jwt = jwt;
        _refresh = refresh;
    }

    public async Task<AuthResponse> Handle(RefreshCommand cmd, CancellationToken ct)
    {
        var userId = await _refresh.ValidateAndRotateAsync(cmd.Request.RefreshToken, ct);
        if (userId == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var access = await _jwt.CreateAccessTokenAsync(userId.Value, ct);
        var newRefresh = await _refresh.IssueAsync(userId.Value, ct);

        return new AuthResponse(access, newRefresh);
    }
}
