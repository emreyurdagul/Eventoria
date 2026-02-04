namespace Eventoria.Application.Auth.Abstractions;

public interface IGuestIdentityService
{
    Task<Guid> CreateGuestUserAsync(string displayName, Guid? eventId, CancellationToken ct);
    Task<(bool Success, string? Error)> UpgradeGuestAsync(Guid guestUserId, string email, string password, CancellationToken ct);

}
