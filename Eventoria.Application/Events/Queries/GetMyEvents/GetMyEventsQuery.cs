using Eventoria.Application.Common.Models;
using Eventoria.Application.Events.Queries.Models;
using MediatR;

namespace Eventoria.Application.Events.Queries.GetMyEvents;

public sealed record GetMyEventsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<MyEventItem>>;
