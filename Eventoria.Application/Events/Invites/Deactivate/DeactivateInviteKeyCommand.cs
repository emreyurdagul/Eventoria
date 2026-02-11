using MediatR;

namespace Eventoria.Application.Events.Invites.Deactivate;

public sealed record DeactivateInviteKeyCommand(
    Guid UserId,
    Guid EventId
) : IRequest<DeactivateInviteKeyResult>;
