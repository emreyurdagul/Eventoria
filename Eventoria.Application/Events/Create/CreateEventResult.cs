namespace Eventoria.Application.Events.Create;

public sealed record CreateEventResult(
    Guid EventId,
    string Code,
    string InviteKey
);
