using MediatR;

namespace Eventoria.Application.Events.Queries.GetEventMediaFeed;

public sealed record GetEventMediaFeedQuery(
    Guid EventId,
    Guid? UserId,               // Optional: for permission checks
    int Page = 1,
    int PageSize = 20
) : IRequest<GetEventMediaFeedResult>;
