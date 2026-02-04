using Eventoria.Application.Auth.Abstractions;
using Eventoria.Infrastructure.Identity;
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

    public async Task<(Guid UserId, string? Error)> CreateGuestAsync(Guid eventId, string displayName, CancellationToken ct)
    {
        if (eventId == Guid.Empty) return (Guid.Empty, "EventId required.");
        if (string.IsNullOrWhiteSpace(displayName)) return (Guid.Empty, "DisplayName required.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            IsGuest = true,
            DisplayName = displayName.Trim(),
            GuestEventId = eventId,

            // Identity için unique username şart
            UserName = $"guest:{Guid.NewGuid():N}",
            Email = null,
            EmailConfirmed = false
        };

        var result = await _users.CreateAsync(user);
        if (!result.Succeeded)
            return (Guid.Empty, string.Join(" | ", result.Errors.Select(e => e.Description)));

        return (user.Id, null);
    }

    public async Task<(Guid UserId, string? Error)> UpgradeGuestAsync(
        Guid currentUserId,
        string email,
        string password,
        string? displayName,
        CancellationToken ct)
    {
        if (currentUserId == Guid.Empty) return (Guid.Empty, "Unauthorized.");
        if (string.IsNullOrWhiteSpace(email)) return (Guid.Empty, "Email required.");
        if (string.IsNullOrWhiteSpace(password)) return (Guid.Empty, "Password required.");

        var user = await _users.FindByIdAsync(currentUserId.ToString());
        if (user is null) return (Guid.Empty, "Unauthorized.");

        if (!user.IsGuest)
            return (Guid.Empty, "User is already upgraded.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Email başka kullanıcıda var mı?
        var existing = await _users.FindByEmailAsync(normalizedEmail);
        if (existing is not null && existing.Id != user.Id)
            return (Guid.Empty, "Email already registered.");

        // Email + username set
        var setEmail = await _users.SetEmailAsync(user, normalizedEmail);
        if (!setEmail.Succeeded)
            return (Guid.Empty, string.Join(" | ", setEmail.Errors.Select(e => e.Description)));

        var setUserName = await _users.SetUserNameAsync(user, normalizedEmail);
        if (!setUserName.Succeeded)
            return (Guid.Empty, string.Join(" | ", setUserName.Errors.Select(e => e.Description)));

        // Password ekle
        var hasPassword = await _users.HasPasswordAsync(user);
        if (hasPassword)
            return (Guid.Empty, "User already has password.");

        var addPass = await _users.AddPasswordAsync(user, password);
        if (!addPass.Succeeded)
            return (Guid.Empty, string.Join(" | ", addPass.Errors.Select(e => e.Description)));

        // DisplayName opsiyonel güncelle
        if (!string.IsNullOrWhiteSpace(displayName))
            user.DisplayName = displayName.Trim();

        user.IsGuest = false;
        user.UpgradedAtUtc = DateTime.UtcNow;

        // Email doğrulama şimdilik yok diyorsun => true yapabilirsin
        user.EmailConfirmed = true;

        var update = await _users.UpdateAsync(user);
        if (!update.Succeeded)
            return (Guid.Empty, string.Join(" | ", update.Errors.Select(e => e.Description)));

        return (user.Id, null);
    }

}
