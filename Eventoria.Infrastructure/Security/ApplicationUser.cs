using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Security;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
