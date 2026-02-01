namespace Eventoria.Application.Abstractions.Identity;

public interface IAdminIdentityService
{
    Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct);

    Task<UserSnapshot?> FindByEmailAsync(string email, CancellationToken ct);

    Task<UserSnapshot> CreateUserAsync(string email, string password, CancellationToken ct);

    Task EnsureRoleExistsAsync(string role, CancellationToken ct);

    Task AddToRoleAsync(Guid userId, string role, CancellationToken ct);
}

public sealed record UserSnapshot(Guid Id, string Email);
