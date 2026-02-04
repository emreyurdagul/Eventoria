namespace Eventoria.Application.Auth.Abstractions;

public interface IIdentityService
{
    Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken ct);
    Task<(bool Success, string? Error)> CreateUserAsync(string email, string password, CancellationToken ct);
    Task<bool> ValidatePasswordAsync(string email, string password, CancellationToken ct);

    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken ct);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct);

    // ✅ NEW
    Task<(Guid UserId, string? Error)> CreateGuestAsync(Guid eventId, string displayName, CancellationToken ct);
    Task<(Guid UserId, string? Error)> UpgradeGuestAsync(Guid currentUserId, string email, string password, string? displayName, CancellationToken ct);
}
