using Eventoria.Domain.Common;
using Eventoria.Domain.Enums;

namespace Eventoria.Domain.Entities;

public class Event : AggregateRoot
{
    private readonly List<EventMembership> _memberships = [];
    private readonly List<EventInvite> _invites = [];

    private Event() { } // EF Core için

    public Event(string title, string? description, DateOnly? date, Guid creatorUserId, string code, EventSpecs specs)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Date = date;
        Code = code;
        CreatedByUserId = creatorUserId;

        Specs = specs ?? throw new ArgumentNullException(nameof(specs));

        AddDomainEvent(new EventCreatedDomainEvent(Id, creatorUserId));
    }

    public string Code { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateOnly? Date { get; private set; }
    public EventStatus Status { get; private set; } = EventStatus.Active;

    public Guid CreatedByUserId { get; private set; }

    public Guid? CoverPhotoMediaFileId { get; private set; }

    public EventSpecs Specs { get; private set; } = default!;

    public IReadOnlyCollection<EventMembership> Memberships => _memberships;
    public IReadOnlyCollection<EventInvite> Invites => _invites;

    public void AddMembership(Guid userId, EventRole role)
    {
        if (_memberships.Any(m => m.UserId == userId))
            return;

        _memberships.Add(new EventMembership(Id, userId, role));
    }

    public void AddInvite(string inviteHash, string? encryptedInviteKey)
    {
        foreach (var inv in _invites.Where(i => i.IsActive))
            inv.Deactivate();

        _invites.Add(new EventInvite(Id, inviteHash, encryptedInviteKey));
    }

    public void AddInviteWithoutDeactivation(string inviteHash, string? encryptedInviteKey)
    {
        _invites.Add(new EventInvite(Id, inviteHash, encryptedInviteKey));
    }

    public void UpdateSpecs(EventSpecs specs)
    {
        Specs = specs ?? throw new ArgumentNullException(nameof(specs));
        SetUpdated();
    }

    public void UpdateDetails(string title, string? description, DateOnly? date)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Title is required.");

        Title = title.Trim();
        Description = description?.Trim();
        Date = date;

        SetUpdated();
    }

    public void SetCoverPhoto(Guid? mediaFileId)
    {
        CoverPhotoMediaFileId = mediaFileId;
        SetUpdated();
    }
}
