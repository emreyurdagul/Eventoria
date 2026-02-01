using Eventoria.Api.Contracts.Posts;
using Eventoria.Application.Posts.Create;
using Eventoria.Application.Posts.Delete;
using Eventoria.Application.Posts.Media.Remove;
using Eventoria.Application.Posts.Queries.GetEventPosts;
using Eventoria.Application.Posts.Queries.GetPostDetails;
using Eventoria.Application.Posts.Reorder;
using Eventoria.Application.Posts.Update;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/posts")]
[Authorize]
public sealed class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }


    // ✅ 1) Event feed (cover-only)
    // GET /api/events/{eventId}/posts?page=1&pageSize=20
    [HttpGet("{eventId:guid}/posts")]
    public async Task<ActionResult<GetEventPostsResult>> GetPosts(
        Guid eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var res = await _mediator.Send(new GetEventPostsQuery(userId, eventId, page, pageSize), ct);
        return Ok(res);
    }


    [HttpPost]
    public async Task<ActionResult<CreatePostResult>> Create([FromBody] CreatePostBody body, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (body.MediaFileIds == null || body.MediaFileIds.Count == 0)
            return BadRequest("MediaFileIds is required.");

        var cmd = new CreatePostCommand(userId, body.EventId, body.Caption, body.MediaFileIds);
        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }

    [HttpPut("{postId:guid}")]
    public async Task<ActionResult<UpdatePostResult>> Update(Guid postId, [FromBody] UpdatePostBody body, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var cmd = new UpdatePostCommand(userId, postId, body.Caption);
        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }

    [HttpPatch("{postId:guid}/media-order")]
    public async Task<ActionResult<ReorderPostMediaResult>> ReorderMedia(Guid postId, [FromBody] ReorderPostMediaBody body, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        if (body.OrderedMediaFileIds == null || body.OrderedMediaFileIds.Count == 0)
            return BadRequest("OrderedMediaFileIds is required.");

        var cmd = new ReorderPostMediaCommand(userId, postId, body.OrderedMediaFileIds);
        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }

    [HttpGet("{postId:guid}")]
    public async Task<ActionResult<GetPostDetailsResult>> GetById(Guid postId, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var res = await _mediator.Send(new GetPostDetailsQuery(userId, postId), ct);
        return Ok(res);
    }

    [HttpDelete("{postId:guid}")]
    public async Task<ActionResult<DeletePostResult>> Delete(Guid postId, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var res = await _mediator.Send(new DeletePostCommand(userId, postId), ct);
        return Ok(res);
    }


    [HttpDelete("{postId:guid}/media/{mediaFileId:guid}")]
    public async Task<ActionResult<RemovePostMediaResult>> RemoveMedia(Guid postId, Guid mediaFileId, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var res = await _mediator.Send(new RemovePostMediaCommand(userId, postId, mediaFileId), ct);
        return Ok(res);
    }

    //// ✅ My posts (user created posts)
    //// GET /api/posts/my?page=1&pageSize=20
    //[HttpGet("my")]
    //public async Task<ActionResult<GetMyPostsResult>> GetMyPosts(
    //    [FromQuery] int page = 1,
    //    [FromQuery] int pageSize = 20,
    //    CancellationToken ct = default)
    //{
    //    var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    //    if (!Guid.TryParse(userIdStr, out var userId))
    //        return Unauthorized();

    //    var res = await _mediator.Send(new GetMyPostsQuery(userId, page, pageSize), ct);
    //    return Ok(res);
    //}

}
