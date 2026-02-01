using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetPostDetails;

public sealed record PostMediaDto(
    Guid MediaFileId,
    int Order,
    string Url,
    string? ContentType,
    long SizeBytes
);
