using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public class EventInvite : BaseEntity
{
    private EventInvite() { }

    internal EventInvite(Guid eventId, string inviteKeyHash)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        InviteKeyHash = inviteKeyHash;
        IsActive = true;
    }

    public Guid EventId { get; private set; }
    public string InviteKeyHash { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public DateTime? RotatedAtUtc { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
        RotatedAtUtc = DateTime.UtcNow;
        SetUpdated();
    }
}
