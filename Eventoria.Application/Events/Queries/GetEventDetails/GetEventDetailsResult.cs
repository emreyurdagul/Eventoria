using Eventoria.Application.Events.Queries.Models;
using Eventoria.Domain.Enums;

namespace Eventoria.Application.Events.Queries.GetEventDetails;

public sealed record GetEventDetailsResult(
    Guid EventId,
    string Title,
    string? Description,
    DateTime Date,
    string Code,
    EventRole MyRole,
    int ParticipantLimit,
    int ParticipantCount,
    int PhotosPerUserLimit,
    int VideosPerUserLimit,
    DateTime CreatedAtUtc
);
