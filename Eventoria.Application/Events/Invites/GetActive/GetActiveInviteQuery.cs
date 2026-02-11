using MediatR;

namespace Eventoria.Application.Events.Invites.GetActive;

public sealed record GetActiveInviteQuery(
    Guid UserId,
    Guid EventId
) : IRequest<GetActiveInviteResult?>;
