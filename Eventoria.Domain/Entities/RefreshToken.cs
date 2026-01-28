using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; protected set; }
    public DateTime? RevokedAt { get; protected set; }

    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAt)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
