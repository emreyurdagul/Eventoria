using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetPostDetails;
public sealed record GetPostDetailsResult(
    Guid PostId,
    Guid EventId,
    Guid CreatedByUserId,
    string? Caption,
    DateTime CreatedAtUtc,
    List<PostMediaDto> Media
);