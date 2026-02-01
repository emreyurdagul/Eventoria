namespace Eventoria.Api.Contracts.Posts;

public sealed record ReorderPostMediaBody(List<Guid> OrderedMediaFileIds);
