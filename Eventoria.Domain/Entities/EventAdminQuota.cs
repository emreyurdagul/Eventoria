using Eventoria.Domain.Common;

namespace Eventoria.Domain.Billing;

public sealed class EventAdminQuota : BaseEntity
{
    private EventAdminQuota() { } // EF

    public EventAdminQuota(Guid adminUserId, int maxEvents, int maxTotalParticipants)
    {
        if (adminUserId == Guid.Empty) throw new ArgumentException("AdminUserId required", nameof(adminUserId));
        if (maxEvents <= 0) throw new ArgumentOutOfRangeException(nameof(maxEvents));
        if (maxTotalParticipants <= 0) throw new ArgumentOutOfRangeException(nameof(maxTotalParticipants));

        Id = Guid.NewGuid();
        AdminUserId = adminUserId;

        MaxEvents = maxEvents;
        MaxTotalParticipants = maxTotalParticipants;

        IsActive = true;
    }

    public Guid AdminUserId { get; private set; }

    public int MaxEvents { get; private set; }
    public int MaxTotalParticipants { get; private set; }

    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        SetUpdated();
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        SetUpdated();
    }

    public void UpdateLimits(int maxEvents, int maxTotalParticipants)
    {
        if (maxEvents <= 0) throw new ArgumentOutOfRangeException(nameof(maxEvents));
        if (maxTotalParticipants <= 0) throw new ArgumentOutOfRangeException(nameof(maxTotalParticipants));

        MaxEvents = maxEvents;
        MaxTotalParticipants = maxTotalParticipants;

        SetUpdated();
    }
}
