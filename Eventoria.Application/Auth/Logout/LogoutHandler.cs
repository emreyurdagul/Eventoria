using Eventoria.Application.Auth.Abstractions;
using MediatR;

namespace Eventoria.Application.Auth.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenStore _refresh;

    public LogoutHandler(IRefreshTokenStore refresh)
    {
        _refresh = refresh;
    }

    public async Task Handle(LogoutCommand cmd, CancellationToken ct)
        => await _refresh.RevokeAsync(cmd.Request.RefreshToken, ct);
}
