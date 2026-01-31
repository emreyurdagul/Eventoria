using System.Security.Claims;
using Eventoria.Application.Media.Download;
using Eventoria.Application.Media.Upload;
using Eventoria.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(210_000_000)] // 200MB + küçük pay
    public async Task<ActionResult<UploadMediaResult>> Upload(
        [FromForm] Guid? eventId,
        [FromForm] MediaVisibility visibility,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (file == null || file.Length <= 0)
            return BadRequest("File is required.");

        await using var stream = file.OpenReadStream();

        var cmd = new UploadMediaCommand(
            UserId: userId,
            EventId: eventId,
            Visibility: visibility,
            FileName: file.FileName,
            ContentType: file.ContentType,
            SizeBytes: file.Length,
            Content: stream
        );

        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }

    [HttpGet("{mediaFileId:guid}/download-url")]
    public async Task<ActionResult<GetMediaDownloadUrlResult>> GetDownloadUrl(
        [FromRoute] Guid mediaFileId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var res = await _mediator.Send(new GetMediaDownloadUrlQuery(userId, mediaFileId), ct);
        return Ok(res);
    }
}
