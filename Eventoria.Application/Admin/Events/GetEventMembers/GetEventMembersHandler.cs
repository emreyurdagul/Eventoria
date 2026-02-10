using Eventoria.Application.Abstractions.Identity;
using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using MediatR;

namespace Eventoria.Application.Admin.Events.GetEventMembers;

public sealed class GetEventMembersHandler : IRequestHandler<GetEventMembersQuery, PagedResult<EventMemberDto>>
{
    private readonly IAdminIdentityService _identity;
    private readonly IEventRepository _events;

    public GetEventMembersHandler(IAdminIdentityService identity, IEventRepository events)
    {
        _identity = identity;
        _events = events;
    }

    public async Task<PagedResult<EventMemberDto>> Handle(GetEventMembersQuery query, CancellationToken ct)
    {
        if (query.ActorUserId == Guid.Empty)
            throw new InvalidOperationException("ActorUserId is required.");

        var isSuperUser = await _identity.IsInRoleAsync(query.ActorUserId, "SuperUser", ct);
        if (!isSuperUser)
            throw new UnauthorizedAccessException("Only SuperUser can view event members.");

        return await _events.GetEventMembersAsync(query.EventId, query.Page, query.PageSize, ct);
    }
}
