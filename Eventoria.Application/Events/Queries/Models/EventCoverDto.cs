namespace Eventoria.Application.Events.Queries.Models;

public sealed record EventCoverDto(
    Guid MediaFileId,
    string Url,
    string ContentType
);
