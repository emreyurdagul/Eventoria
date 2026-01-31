using System.Security.Cryptography;
using System.Text;
using Eventoria.Application.Auth.Abstractions;
using Eventoria.Domain.Entities;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Auth.RefreshTokens;

public sealed class RefreshTokenStore : IRefreshTokenStore
{
    private readonly AppDbContext _db;

    public RefreshTokenStore(AppDbContext db)
    {
        _db = db;
    }

    public async Task<string> IssueAsync(Guid userId, CancellationToken ct)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = Sha256HexLower(raw);

        var entity = new RefreshToken(
            userId: userId,
            tokenHash: hash,
            expiresAt: DateTime.UtcNow.AddDays(14));

        _db.RefreshTokens.Add(entity);
        await _db.SaveChangesAsync(ct);

        return raw;
    }

    public async Task<Guid?> ValidateAndRotateAsync(string rawRefreshToken, CancellationToken ct)
    {
        var hash = Sha256HexLower(rawRefreshToken);

        var existing = await _db.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (existing == null || !existing.IsActive)
            return null;

        existing.Revoke();
        await _db.SaveChangesAsync(ct);

        return existing.UserId;
    }

    public async Task RevokeAsync(string rawRefreshToken, CancellationToken ct)
    {
        var hash = Sha256HexLower(rawRefreshToken);

        var token = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == hash, ct);
        if (token == null) return;

        if (token.IsActive)
        {
            token.Revoke();
            await _db.SaveChangesAsync(ct);
        }
    }

    private static string Sha256HexLower(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
