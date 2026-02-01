using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Reorder;
public sealed record ReorderPostMediaCommand(
    Guid UserId,
    Guid PostId,
    IReadOnlyList<Guid> OrderedMediaFileIds
) : IRequest<ReorderPostMediaResult>;
