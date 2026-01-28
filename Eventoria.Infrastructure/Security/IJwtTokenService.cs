using Eventoria.Infrastructure.Security;

namespace Eventoria.Application.Auth;

public interface IJwtTokenService
{
    Task<string> CreateAsync(ApplicationUser user);
}
