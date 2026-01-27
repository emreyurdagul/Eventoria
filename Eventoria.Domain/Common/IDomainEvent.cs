namespace Eventoria.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAtUtc { get; }
}
