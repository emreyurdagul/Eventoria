using Eventoria.Application.Auth.Abstractions;
using Eventoria.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Auth.Identity;

public sealed class GuestIdentityService : IGuestIdentityService
{
    private readonly UserManager<ApplicationUser> _users;

    public GuestIdentityService(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    public async Task<Guid> CreateGuestUserAsync(string displayName, Guid? eventId, CancellationToken ct)
    {
        // Guest için unique username üret
        // email yok -> Email null
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            IsGuest = true,
            DisplayName = displayName,
            GuestEventId = eventId,
            UserName = $"guest_{Guid.NewGuid():N}", // unique
            Email = null,
            EmailConfirmed = false
        };

        var result = await _users.CreateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

        return user.Id;
    }

    public async Task<(bool Success, string? Error)> UpgradeGuestAsync(Guid guestUserId, string email, string password, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(guestUserId.ToString());
        if (user is null) return (false, "User not found.");

        if (!user.IsGuest) return (false, "User is already upgraded.");

        email = email.Trim().ToLowerInvariant();

        // Email başkasında var mı?
        var existing = await _users.FindByEmailAsync(email);
        if (existing is not null && existing.Id != user.Id)
            return (false, "Email already registered.");

        // Email + username set
        var setEmail = await _users.SetEmailAsync(user, email);
        if (!setEmail.Succeeded)
            return (false, string.Join(" | ", setEmail.Errors.Select(e => e.Description)));

        var setUserName = await _users.SetUserNameAsync(user, email);
        if (!setUserName.Succeeded)
            return (false, string.Join(" | ", setUserName.Errors.Select(e => e.Description)));

        // Password ekle (guest'te yok)
        if (await _users.HasPasswordAsync(user))
            return (false, "User already has password.");

        var addPass = await _users.AddPasswordAsync(user, password);
        if (!addPass.Succeeded)
            return (false, string.Join(" | ", addPass.Errors.Select(e => e.Description)));

        user.IsGuest = false;
        user.UpgradedAtUtc = DateTime.UtcNow;

        // Email doğrulama yoksa:
        user.EmailConfirmed = true;

        var update = await _users.UpdateAsync(user);
        if (!update.Succeeded)
            return (false, string.Join(" | ", update.Errors.Select(e => e.Description)));

        return (true, null);
    }
}
