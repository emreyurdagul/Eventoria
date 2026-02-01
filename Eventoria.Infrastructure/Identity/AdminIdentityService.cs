using Eventoria.Application.Abstractions.Identity;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Identity;

public sealed class AdminIdentityService : IAdminIdentityService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole<Guid>> _roles;

    public AdminIdentityService(
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<Guid>> roles)
    {
        _users = users;
        _roles = roles;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        return await _users.IsInRoleAsync(user, role);
    }

    public async Task<UserSnapshot?> FindByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        return user == null ? null : new UserSnapshot(user.Id, user.Email ?? email);
    }

    public async Task<UserSnapshot> CreateUserAsync(string email, string password, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var res = await _users.CreateAsync(user, password);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));

        return new UserSnapshot(user.Id, user.Email ?? email);
    }

    public async Task EnsureRoleExistsAsync(string role, CancellationToken ct)
    {
        if (!await _roles.RoleExistsAsync(role))
        {
            var res = await _roles.CreateAsync(new IdentityRole<Guid>(role));
            if (!res.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));
        }
    }

    public async Task AddToRoleAsync(Guid userId, string role, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found.");

        if (await _users.IsInRoleAsync(user, role))
            return;

        var res = await _users.AddToRoleAsync(user, role);
        if (!res.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", res.Errors.Select(e => e.Description)));
    }
}
