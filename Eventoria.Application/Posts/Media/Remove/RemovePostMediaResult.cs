using System;
using System.Collections.Generic;
using System.Text;

namespace Eventoria.Application.Posts.Media.Remove;
public sealed record RemovePostMediaResult(
    Guid PostId,
    Guid MediaFileId
);