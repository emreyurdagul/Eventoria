using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using MediatR;

namespace Eventoria.Application.Admin.Events.GetEventMembers;

public sealed record GetEventMembersQuery(
    Guid ActorUserId,
    Guid EventId,
    int Page,
    int PageSize
) : IRequest<PagedResult<EventMemberDto>>;
