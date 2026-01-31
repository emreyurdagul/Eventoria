using MediatR;

namespace Eventoria.Application.Media.Download;

public sealed record GetMediaDownloadUrlQuery(
    Guid UserId,
    Guid MediaFileId
) : IRequest<GetMediaDownloadUrlResult>;
