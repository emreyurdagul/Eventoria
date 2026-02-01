using MediatR;

namespace Eventoria.Application.Posts.Create;

public sealed record CreatePostCommand(
    Guid UserId,
    Guid EventId,
    string? Caption,
    IReadOnlyList<Guid> MediaFileIds
) : IRequest<CreatePostResult>;

