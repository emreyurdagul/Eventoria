using Eventoria.Infrastructure.Security;

namespace Eventoria.Api.Security;

public interface IJwtTokenService
{
    Task<string> CreateAsync(ApplicationUser user);
}
