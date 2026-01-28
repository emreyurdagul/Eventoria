using Eventoria.Application.Events.Contracts;

namespace Eventoria.Application.Events;

public interface IEventService
{
    Task<CreateEventResponse> CreateAsync(Guid creatorUserId, CreateEventRequest req, CancellationToken ct);
    Task RotateInviteAsync(Guid eventId, Guid actorUserId, CancellationToken ct);
    Task JoinAsync(Guid userId, JoinEventRequest req, CancellationToken ct);
}
