using MediatR;

namespace Eventoria.Application.Events.Invites.Generate;

public sealed record GenerateInviteKeyCommand(
    Guid UserId,
    Guid EventId
) : IRequest<GenerateInviteKeyResult>;
