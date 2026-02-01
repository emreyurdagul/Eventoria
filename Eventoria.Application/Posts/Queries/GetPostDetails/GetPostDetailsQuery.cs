using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Queries.GetPostDetails;

public sealed record GetPostDetailsQuery(
    Guid UserId,
    Guid PostId
) : IRequest<GetPostDetailsResult>;
