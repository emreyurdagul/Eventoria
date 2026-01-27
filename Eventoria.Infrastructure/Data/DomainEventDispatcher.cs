using Eventoria.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Eventoria.Infrastructure.Data;

public class DomainEventDispatcher(ILogger<DomainEventDispatcher> logger) : IDomainEventDispatcher
{
    private readonly ILogger<DomainEventDispatcher> _logger = logger;

    public Task DispatchAsync(IReadOnlyCollection<IDomainEvent> events, CancellationToken ct)
    {
        // Şimdilik sadece logluyoruz.
        // Sonra: MediatR handlers / outbox pattern / integration events.
        foreach (var ev in events)
            _logger.LogInformation("DomainEvent dispatched: {EventType} at {At}", ev.GetType().Name, ev.OccurredAtUtc);

        return Task.CompletedTask;
    }
}
