using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetEventPosts;

public sealed record EventPostCoverDto(
    Guid MediaFileId,
    int Order,
    string Url,
    string? ContentType,
    long SizeBytes
);