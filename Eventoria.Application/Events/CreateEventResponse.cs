namespace Eventoria.Application.Events.Contracts;

public record CreateEventResponse(
    Guid EventId,
    string Code
);
