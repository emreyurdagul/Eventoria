using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetEventPosts;


public sealed record GetEventPostsQuery(
    Guid UserId,
    Guid EventId,
    int Page = 1,
    int PageSize = 20
) : IRequest<GetEventPostsResult>;
