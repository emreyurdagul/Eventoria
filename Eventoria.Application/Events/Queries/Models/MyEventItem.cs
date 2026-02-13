using Eventoria.Domain.Enums;

namespace Eventoria.Application.Events.Queries.Models;

public sealed record MyEventItem(
    Guid EventId,
    string Code,
    string Title,
    DateOnly? Date,
    EventStatus Status,
    EventRole MyRole,
    int ParticipantLimit,
    int MemberCount,
    EventCoverDto? CoverPhoto
);
