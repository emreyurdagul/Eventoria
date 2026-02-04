using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    // Guest mi?
    public bool IsGuest { get; set; }

    // Guest display name (QR sayfasında girilen)
    public string? DisplayName { get; set; }

    // Guest’in hangi event’ten doğduğunu izlemek istersen (opsiyonel ama faydalı)
    public Guid? GuestEventId { get; set; }

    // Upgrade / izleme
    public DateTime? UpgradedAtUtc { get; set; }
}
