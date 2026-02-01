namespace Eventoria.Api.Contracts.Posts;

public sealed record CreatePostBody(
    Guid EventId,
    string? Caption,
    List<Guid> MediaFileIds
);
