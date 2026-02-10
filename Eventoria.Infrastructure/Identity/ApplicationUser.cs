using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public bool IsGuest { get; set; }

    public string? DisplayName { get; set; }

    public Guid? GuestEventId { get; set; }

    public DateTime? UpgradedAtUtc { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
