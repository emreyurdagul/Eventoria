using System.Security.Cryptography;
using System.Text;
using Eventoria.Application.Auth;
using Eventoria.Application.Auth.Contracts;
using Eventoria.Domain.Entities;
using Eventoria.Infrastructure.Data;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwt;

    public AuthService(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        AppDbContext db,
        IJwtTokenService jwt)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _users.FindByEmailAsync(email);
        if (exists != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));

        var access = await _jwt.CreateAsync(user);
        var refresh = await IssueRefreshTokenAsync(user.Id);

        return new AuthResponse(access, refresh);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _users.FindByEmailAsync(email);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var ok = await _signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!ok.Succeeded)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var access = await _jwt.CreateAsync(user);
        var refresh = await IssueRefreshTokenAsync(user.Id);

        return new AuthResponse(access, refresh);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request)
    {
        // 1) Validate refresh token by hash lookup
        var hash = Sha256Hex(request.RefreshToken);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (existing == null || !existing.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // 2) Rotate: revoke old token
        existing.Revoke();
        await _db.SaveChangesAsync();

        // 3) Issue new pair
        var user = await _users.FindByIdAsync(existing.UserId.ToString());
        if (user == null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var access = await _jwt.CreateAsync(user);
        var refresh = await IssueRefreshTokenAsync(user.Id);

        return new AuthResponse(access, refresh);
    }

    public async Task LogoutAsync(LogoutRequest request)
    {
        var hash = Sha256Hex(request.RefreshToken);

        var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash);
        if (token == null) return;

        if (token.IsActive)
        {
            token.Revoke();
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ForgotPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email);

        // security: kullanıcı yoksa da aynı cevap
        if (user == null) return "OK";

        return await _users.GeneratePasswordResetTokenAsync(user);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("Invalid request.");

        var result = await _users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));
    }

    private async Task<string> IssueRefreshTokenAsync(Guid userId)
    {
        // raw token (client’a gidecek)
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = Sha256Hex(raw);

        var entity = new RefreshToken(
            userId: userId,
            tokenHash: hash,
            expiresAt: DateTime.UtcNow.AddDays(14));

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync();

        return raw;
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
