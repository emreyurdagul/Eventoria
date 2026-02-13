namespace Eventoria.Application.Posts.Queries.GetPostDetails;

public sealed record PostMediaDto(
    Guid MediaFileId,
    int Order,
    string Url,
    string? ContentType,
    long SizeBytes,
    string? ThumbnailUrl = null,              // For videos: thumbnail image URL
    Guid? ThumbnailMediaFileId = null         // For videos: thumbnail MediaFileId
);
