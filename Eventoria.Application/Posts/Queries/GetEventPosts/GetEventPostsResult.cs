using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetEventPosts;

public sealed record GetEventPostsResult(
    Guid EventId,
    int Page,
    int PageSize,
    List<EventPostListItemDto> Items
);
