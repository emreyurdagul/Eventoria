using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Media.Remove;

public sealed record RemovePostMediaCommand(
    Guid UserId,
    Guid PostId,
    Guid MediaFileId
) : IRequest<RemovePostMediaResult>;
