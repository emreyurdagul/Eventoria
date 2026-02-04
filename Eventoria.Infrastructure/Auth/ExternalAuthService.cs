using Eventoria.Application.Auth;
using Eventoria.Application.Auth.Abstractions;
using Eventoria.Application.Auth.Contracts;
using Eventoria.Application.Auth.External;
using Eventoria.Infrastructure.Identity;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Eventoria.Infrastructure.Auth;

public sealed class ExternalAuthService : IExternalAuthService
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenStore _refresh;

    public ExternalAuthService(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser> users,
        IJwtTokenService jwt,
        IRefreshTokenStore refresh)
    {
        _signIn = signIn;
        _users = users;
        _jwt = jwt;
        _refresh = refresh;
    }

    public async Task<AuthResponse> SignInWithGoogleAsync(CancellationToken ct)
    {
        // Bu info, Google external cookie set edildiyse gelir (Callback endpoint’inde çalışır)
        var info = await _signIn.GetExternalLoginInfoAsync();
        if (info == null)
            throw new InvalidOperationException("External login info not found.");

        var email =
            info.Principal.FindFirstValue(ClaimTypes.Email)
            ?? info.Principal.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Google account email is missing.");

        email = email.Trim().ToLowerInvariant();

        // 1) providerKey ile bağlı user var mı?
        var user = await _users.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

        if (user == null)
        {
            // 2) email ile user var mı?
            user = await _users.FindByEmailAsync(email);

            // 3) yoksa create
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // MVP: google doğruladı varsayımı
                };

                var create = await _users.CreateAsync(user);
                if (!create.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(" | ", create.Errors.Select(e => e.Description)));
            }

            // 4) External login’i bağla
            // NOT: UserLoginInfo ctor named param kabul etmez, bu şekilde kullanılmalı:
            var loginInfo = new UserLoginInfo(
                info.LoginProvider,
                info.ProviderKey,
                info.LoginProvider);

            var addLogin = await _users.AddLoginAsync(user, loginInfo);
            if (!addLogin.Succeeded)
                throw new InvalidOperationException(
                    string.Join(" | ", addLogin.Errors.Select(e => e.Description)));
        }

        // External cookie temizliği (Identity external flow’u kullandığın için önemli)
        await _signIn.SignOutAsync();

        // 5) Token üretimi: artık AuthService yok -> handler mantığıyla aynı pipeline
        var access = await _jwt.CreateAccessTokenAsync(user.Id, ct);
        var refresh = await _refresh.IssueAsync(user.Id, ct);

        return new AuthResponse(access, refresh);
    }
}
