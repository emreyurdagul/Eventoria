using Eventoria.Application.Abstractions;
using Eventoria.Application.Abstractions.Auth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Eventoria.Infrastructure.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public bool IsAuthenticated =>
        _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid? UserIdOrNull
    {
        get
        {
            var principal = _http.HttpContext?.User;
            if (principal == null) return null;

            // JwtRegisteredClaimNames.Sub => "sub"
            var idStr =
                principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");

            return Guid.TryParse(idStr, out var id) ? id : null;
        }
    }

    public Guid UserId =>
        UserIdOrNull ?? throw new UnauthorizedAccessException("User is not authenticated.");
}
