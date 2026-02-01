using Eventoria.Api.Contracts.Media;
using Eventoria.Application.Media.Download;
using Eventoria.Application.Media.Upload;
using Eventoria.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(210_000_000)]
    public async Task<ActionResult<UploadMediaResult>> Upload(
            [FromForm] UploadMediaForm form,
            CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (form.File == null || form.File.Length <= 0)
            return BadRequest("File is required.");

        await using var stream = form.File.OpenReadStream();

        var cmd = new UploadMediaCommand(
            UserId: userId,
            EventId: form.EventId,
            Visibility: form.Visibility,
            FileName: form.File.FileName,
            ContentType: form.File.ContentType,
            SizeBytes: form.File.Length,
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
