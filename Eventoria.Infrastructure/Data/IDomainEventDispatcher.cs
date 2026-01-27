using Eventoria.Domain.Common;

namespace Eventoria.Infrastructure.Data;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct);
}
