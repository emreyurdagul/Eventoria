namespace Eventoria.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString("N");

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    public void SetUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}
