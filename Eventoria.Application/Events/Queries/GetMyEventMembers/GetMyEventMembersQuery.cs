using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetMyEventMembers;

public sealed record GetMyEventMembersQuery(
    Guid UserId,
    Guid EventId,
    int Page,
    int PageSize
) : IRequest<PagedResult<EventMemberDto>>;
