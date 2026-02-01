using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Update;

public sealed record UpdatePostCommand(Guid UserId, Guid PostId, string? Caption)
    : IRequest<UpdatePostResult>;