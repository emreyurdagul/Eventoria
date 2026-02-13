namespace Eventoria.Application.Events.Queries.GetEventMediaFeed;

public sealed record EventMediaFeedItemDto(
    Guid MediaId,
    Guid PostId,
    string? PostTitle,
    Guid PostAuthorId,
    string PostAuthorName,
    string MediaType,           // "Photo" or "Video"
    Guid? ThumbnailMediaFileId, // For videos: the thumbnail's MediaFileId
    string? ThumbnailUrl,
    string? DownloadUrl,
    DateTime PostedAtUtc,
    int LikeCount,
    int CommentCount
);

public sealed record GetEventMediaFeedResult(
    IReadOnlyList<EventMediaFeedItemDto> MediaItems,
    int Page,
    int PageSize,
    int Total,
    bool HasMore
);
