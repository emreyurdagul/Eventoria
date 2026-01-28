namespace Eventoria.Application.Events.Contracts;

public record CreateEventRequest(
    string Title,
    string? Description,
    DateOnly? Date
);
