using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Delete;

public sealed record DeletePostCommand(
    Guid UserId,
    Guid PostId
) : IRequest<DeletePostResult>;