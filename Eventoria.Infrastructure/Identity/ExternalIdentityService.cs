using Eventoria.Application.Auth.Abstractions;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;

namespace Eventoria.Infrastructure.Identity;

public sealed class ExternalIdentityService : IExternalIdentityService
{
    private readonly UserManager<ApplicationUser> _users;

    public ExternalIdentityService(UserManager<ApplicationUser> users)
    {
        _users = users;
    }

    public async Task<Guid> GetOrCreateUserFromGoogleAsync(
        string providerKey,
        string email,
        string? firstName,
        string? lastName,
        CancellationToken ct)
    {
        // 1) Daha önce bu google hesabı bağlanmış mı?
        var user = await _users.FindByLoginAsync("Google", providerKey);
        if (user != null)
            return user.Id;

        // 2) Email ile kullanıcı var mı?
        user = await _users.FindByEmailAsync(email);

        // 3) Yoksa oluştur
        if (user == null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                EmailConfirmed = true, // MVP: Google doğruladı varsayımı

            };

            var create = await _users.CreateAsync(user);
            if (!create.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", create.Errors.Select(e => e.Description)));
        }

        // 4) Google login’i bağla
        var loginInfo = new UserLoginInfo("Google", providerKey, "Google");
        var add = await _users.AddLoginAsync(user, loginInfo);

        // Bazı durumlarda aynı login zaten ekli olabilir → bunu tolere etmek istersen:
        if (!add.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", add.Errors.Select(e => e.Description)));

        return user.Id;
    }
}
