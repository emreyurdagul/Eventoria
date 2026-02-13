namespace Eventoria.Application.Posts.Queries.GetEventPosts;

public sealed record EventPostCoverDto(
    Guid MediaFileId,
    int Order,
    string Url,
    string? ContentType,
    long SizeBytes,
    string? ThumbnailUrl = null,              // For videos: thumbnail image URL
    Guid? ThumbnailMediaFileId = null         // For videos: thumbnail MediaFileId
);
