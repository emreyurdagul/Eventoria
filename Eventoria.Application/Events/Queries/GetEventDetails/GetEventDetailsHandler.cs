using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Events.Queries.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetEventDetails;

public sealed class GetEventDetailsHandler : IRequestHandler<GetEventDetailsQuery, EventDetailsDto>
{
    private readonly IEventRepository _events;

    public GetEventDetailsHandler(IEventRepository events)
    {
        _events = events;
    }

    public async Task<EventDetailsDto> Handle(GetEventDetailsQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (query.EventId == Guid.Empty) throw new InvalidOperationException("EventId is required.");

        var dto = await _events.GetEventDetailsAsync(query.EventId, query.UserId, ct);
        if (dto == null)
            throw new UnauthorizedAccessException("You are not a member of this event.");

        return dto;
    }
}
