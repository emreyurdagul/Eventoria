using Eventoria.Domain.Enums;

namespace Eventoria.Application.Events.Queries.Models;

public sealed record EventDetailsDto(
    Guid EventId,
    string Code,
    string Title,
    string? Description,
    DateOnly? Date,
    EventStatus Status,
    Guid CreatedByUserId,
    EventRole MyRole,
    int ParticipantLimit,
    int PhotosPerUserLimit,
    int VideosPerUserLimit,
    int MemberCount,
    bool HasActiveInvite // sadece admin için true/false meaningful
);
