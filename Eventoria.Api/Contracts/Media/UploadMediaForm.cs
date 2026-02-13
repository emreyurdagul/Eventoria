using Eventoria.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Eventoria.Api.Contracts.Media;

public sealed class UploadMediaForm
{
    public Guid? EventId { get; set; }
    public Guid? ThumbnailMediaFileId { get; set; }
    public MediaVisibility Visibility { get; set; } = MediaVisibility.Private;

    public IFormFile File { get; set; } = default!;
}
