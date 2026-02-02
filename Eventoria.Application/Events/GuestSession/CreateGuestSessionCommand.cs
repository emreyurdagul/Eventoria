using MediatR;

namespace Eventoria.Application.Events.GuestSession;

public sealed record CreateGuestSessionCommand(
    string Code,
    string InviteKey,
    string DisplayName
) : IRequest<CreateGuestSessionResult>;

public sealed record CreateGuestSessionResult(
    Guid EventId,
    string EventTitle,
    Guid GuestId,
    string GuestToken
);
