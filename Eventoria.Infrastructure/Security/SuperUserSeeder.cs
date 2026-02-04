using Eventoria.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Eventoria.Infrastructure.Security;

public static class SuperUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var email = config["SuperUser:Email"];
        var password = config["SuperUser:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        // Role ensure
        if (!await roleManager.RoleExistsAsync("SuperUser"))
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>("SuperUser"));
        }

        // User ensure
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception("SuperUser create failed: " +
                    string.Join(",", result.Errors.Select(x => x.Description)));
        }

        // Role assign
        if (!await userManager.IsInRoleAsync(user, "SuperUser"))
        {
            await userManager.AddToRoleAsync(user, "SuperUser");
        }
    }
}
