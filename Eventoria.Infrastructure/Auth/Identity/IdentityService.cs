using Eventoria.Application.Auth.Abstractions;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Auth.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;

    public IdentityService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    public async Task<Guid?> FindUserIdByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(string email, string password, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _users.CreateAsync(user, password);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" | ", result.Errors.Select(e => e.Description)));
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user == null) return false;

        var ok = await _signIn.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        return ok.Succeeded;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user == null) return null;

        return await _users.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(email);
        if (user == null) return (false, "Invalid request.");

        var result = await _users.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded
            ? (true, null)
            : (false, string.Join(" | ", result.Errors.Select(e => e.Description)));
    }
}
