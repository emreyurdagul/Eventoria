using Eventoria.Domain.Common;
using Eventoria.Domain.Enums;

namespace Eventoria.Domain.Entities;

public class EventMembership : BaseEntity
{
    private EventMembership() { }

    internal EventMembership(Guid eventId, Guid userId, EventRole role)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        UserId = userId;
        Role = role;
        JoinedAtUtc = DateTime.UtcNow;
    }

    public Guid EventId { get; private set; }

    public Event Event { get; private set; }
    public Guid UserId { get; private set; }

    public EventRole Role { get; private set; }

    public DateTime JoinedAtUtc { get; private set; }
}
