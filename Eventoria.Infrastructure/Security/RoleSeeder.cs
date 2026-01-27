using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventoria.Infrastructure.Security;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        IServiceProvider sp,
        CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure roles
        if (!await roleManager.RoleExistsAsync("SuperUser"))
            await roleManager.CreateAsync(new IdentityRole<Guid>("SuperUser"));

        // Ensure superusers by email
        var emails = config.GetSection("SuperUsers").Get<string[]>() ?? Array.Empty<string>();

        foreach (var raw in emails)
        {
            var email = raw.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email)) continue;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null) continue; // user registers first, then seed assigns role

            if (!await userManager.IsInRoleAsync(user, "SuperUser"))
                await userManager.AddToRoleAsync(user, "SuperUser");
        }
    }
}
