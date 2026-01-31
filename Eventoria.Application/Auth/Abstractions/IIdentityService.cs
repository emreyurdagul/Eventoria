namespace Eventoria.Application.Auth.Abstractions;

public interface IIdentityService
{
    Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken ct);
    Task<(bool Success, string? Error)> CreateUserAsync(string email, string password, CancellationToken ct);
    Task<bool> ValidatePasswordAsync(string email, string password, CancellationToken ct);

    Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken ct);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct);
}
