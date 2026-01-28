namespace Eventoria.Application.Events.Contracts;

public record JoinEventRequest(
    string Code,
    string InviteKey
);
