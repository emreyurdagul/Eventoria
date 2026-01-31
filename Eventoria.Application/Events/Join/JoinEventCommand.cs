using MediatR;

namespace Eventoria.Application.Events.Join;

public sealed record JoinEventCommand(
    Guid UserId,
    string Code,
    string InviteKey
) : IRequest<JoinEventResult>;
