namespace Eventoria.Domain.Common;

public abstract record DomainEvent(DateTime OccurredAtUtc) : IDomainEvent;
