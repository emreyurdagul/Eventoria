using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using MediatR;

namespace Eventoria.Application.Events.Update;

public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, UpdateEventResult>
{
    private readonly IEventRepository _events;

    public UpdateEventHandler(IEventRepository events)
    {
        _events = events;
    }

    public async Task<UpdateEventResult> Handle(UpdateEventCommand cmd, CancellationToken ct)
    {
        if (cmd.ActorUserId == Guid.Empty) throw new InvalidOperationException("ActorUserId is required.");
        if (cmd.EventId == Guid.Empty) throw new InvalidOperationException("EventId is required.");

        // Admin kontrolü (sende zaten repository methodu var)
        var isAdmin = await _events.IsEventAdminAsync(cmd.EventId, cmd.ActorUserId, ct);
        if (!isAdmin) throw new UnauthorizedAccessException("Only event admins can update event.");

        var ev = await _events.GetByIdWithIncludesAsync(cmd.EventId, ct)
            ?? throw new InvalidOperationException("Event not found.");

        // Details
        ev.UpdateDetails(cmd.Title, cmd.Description, cmd.Date);

        // Specs
        var specs = new EventSpecs(cmd.ParticipantLimit, cmd.PhotosPerUserLimit, cmd.VideosPerUserLimit);
        ev.UpdateSpecs(specs);

        // SaveChanges => senin UnitOfWorkBehavior/TransactionBehavior setup’ına göre pipeline’da yapılacak.
        // Eğer davranışın SaveChanges çağırmıyorsa, handler’a uow inject edip SaveChangesAsync çağırırsın.

        return new UpdateEventResult(ev.Id);
    }
}
