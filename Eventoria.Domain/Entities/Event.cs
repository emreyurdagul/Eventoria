using Eventoria.Domain.Common;
using Eventoria.Domain.Enums;

namespace Eventoria.Domain.Entities;

public class Event : BaseEntity
{
    private readonly List<EventMembership> _memberships = [];
    private readonly List<EventInvite> _invites = [];

    private Event() { } // EF Core için

    public Event(string title, string? description, DateOnly? date, Guid creatorUserId, string code)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Date = date;
        Code = code;
        CreatedByUserId = creatorUserId;

        AddDomainEvent(new EventCreatedDomainEvent(Id, creatorUserId));
    }

    public string Code { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateOnly? Date { get; private set; }
    public EventStatus Status { get; private set; } = EventStatus.Active;

    public Guid CreatedByUserId { get; private set; }

    public IReadOnlyCollection<EventMembership> Memberships => _memberships;
    public IReadOnlyCollection<EventInvite> Invites => _invites;

    public void AddMembership(Guid userId, EventRole role)
    {
        if (_memberships.Any(m => m.UserId == userId))
            return;

        _memberships.Add(new EventMembership(Id, userId, role));
    }

    public void AddInvite(string inviteHash)
    {
        foreach (var inv in _invites.Where(i => i.IsActive))
            inv.Deactivate();

        _invites.Add(new EventInvite(Id, inviteHash));
    }
}
