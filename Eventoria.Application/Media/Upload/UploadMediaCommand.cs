using Eventoria.Domain.Enums;
using MediatR;

namespace Eventoria.Application.Media.Upload;

public sealed record UploadMediaCommand(
    Guid UserId,
    Guid? EventId,
    MediaVisibility Visibility,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content
) : IRequest<UploadMediaResult>;
