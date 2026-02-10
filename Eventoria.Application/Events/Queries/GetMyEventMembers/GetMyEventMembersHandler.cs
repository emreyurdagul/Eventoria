using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetMyEventMembers;

public sealed class GetMyEventMembersHandler : IRequestHandler<GetMyEventMembersQuery, PagedResult<EventMemberDto>>
{
    private readonly IEventRepository _events;

    public GetMyEventMembersHandler(IEventRepository events)
    {
        _events = events;
    }

    public async Task<PagedResult<EventMemberDto>> Handle(GetMyEventMembersQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        var isAdmin = await _events.IsEventAdminAsync(query.EventId, query.UserId, ct);
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only Event Admin can view event members.");

        return await _events.GetEventMembersAsync(query.EventId, query.Page, query.PageSize, ct);
    }
}
