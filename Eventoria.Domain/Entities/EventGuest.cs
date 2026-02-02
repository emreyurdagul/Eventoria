using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public sealed class EventGuest : BaseEntity
{
    private EventGuest() { } // EF

    public EventGuest(Guid eventId, string displayName)
    {
        if (eventId == Guid.Empty) throw new ArgumentException("EventId required.");
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("DisplayName required.");

        Id = Guid.NewGuid();
        EventId = eventId;
        DisplayName = displayName.Trim();
    }

    public Guid EventId { get; private set; }
    public string DisplayName { get; private set; } = default!;
}
