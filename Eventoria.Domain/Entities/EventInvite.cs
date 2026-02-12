using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public class EventInvite : BaseEntity
{
    private EventInvite() { }

    internal EventInvite(Guid eventId, string inviteKeyHash, string? encryptedInviteKey)
    {
        EventId = eventId;
        InviteKeyHash = inviteKeyHash;
        EncryptedInviteKey = encryptedInviteKey;  // Þifrelenmiþ plaintext key
        IsActive = true;
    }

    public Guid EventId { get; private set; }
    public string InviteKeyHash { get; private set; } = default!;
    public string? EncryptedInviteKey { get; private set; }  // Yeni alan
    public bool IsActive { get; private set; }

    public DateTime? RotatedAtUtc { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
        RotatedAtUtc = DateTime.UtcNow;
        EncryptedInviteKey = null;  // Deaktif olunca encrypted key'i de sil (güvenlik)
        SetUpdated();
    }
}
