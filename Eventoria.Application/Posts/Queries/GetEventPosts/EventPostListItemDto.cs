using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetEventPosts;

public sealed record EventPostListItemDto(
    Guid PostId,
    Guid? CreatedByUserId,
    string? CreatedByDisplayName,  // Yeni eklendi
    string? Caption,
    DateTime CreatedAtUtc,
    EventPostCoverDto? Cover
);
