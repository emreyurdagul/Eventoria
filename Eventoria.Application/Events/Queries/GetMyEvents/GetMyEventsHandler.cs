using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Events.Queries.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetMyEvents;

public sealed class GetMyEventsHandler : IRequestHandler<GetMyEventsQuery, IReadOnlyList<MyEventItem>>
{
    private readonly IEventRepository _events;

    public GetMyEventsHandler(IEventRepository events)
    {
        _events = events;
    }

    public async Task<IReadOnlyList<MyEventItem>> Handle(GetMyEventsQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        return await _events.GetMyEventsAsync(query.UserId, ct);
    }
}
