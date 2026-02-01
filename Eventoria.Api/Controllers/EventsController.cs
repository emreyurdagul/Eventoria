using Eventoria.Api.Contracts.Events;
using Eventoria.Application.Common.Models;
using Eventoria.Application.Events.Create;
using Eventoria.Application.Events.Join;
using Eventoria.Application.Events.Queries.GetEventDetails;
using Eventoria.Application.Events.Queries.GetMyEvents;
using Eventoria.Application.Events.Queries.Models;
using Eventoria.Application.Events.Update;
using Eventoria.Application.Posts.Queries.GetEventPosts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CreateEventResult>> Create([FromBody] CreateEventRequestBody body, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var cmd = new CreateEventCommand(
            userId,
            body.Title,
            body.Description,
            body.Date,
            body.ParticipantLimit,
            body.PhotosPerUserLimit,
            body.VideosPerUserLimit
        );

        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }


    [HttpPut("{eventId:guid}")]
    [Authorize]
    public async Task<ActionResult<UpdateEventResult>> Update(Guid eventId, [FromBody] UpdateEventRequestBody body, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var cmd = new UpdateEventCommand(
            userId,
            eventId,
            body.Title,
            body.Description,
            body.Date,
            body.ParticipantLimit,
            body.PhotosPerUserLimit,
            body.VideosPerUserLimit
        );

        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }

    [HttpPost("join")]
    [Authorize]
    public async Task<ActionResult<JoinEventResult>> Join([FromBody] JoinEventBody body, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var cmd = new JoinEventCommand(userId, body.Code, body.InviteKey);
        var res = await _mediator.Send(cmd, ct);

        return Ok(res);
    }


    [HttpGet("{eventId:guid}/posts")]
    [Authorize]
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

    // ✅ GET /api/events/my?page=1&pageSize=20
    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<MyEventItem>>> GetMyEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserIdOrThrow();
        var res = await _mediator.Send(
            new GetMyEventsQuery(userId, page, pageSize),
            ct);

        return Ok(res);
    }


    // ✅ GET /api/events/{eventId}
    [HttpGet("{eventId:guid}")]
    public async Task<ActionResult<GetEventDetailsResult>> GetEventDetails(Guid eventId, CancellationToken ct)
    {
        var userId = GetUserIdOrThrow();
        var res = await _mediator.Send(new GetEventDetailsQuery(userId, eventId), ct);
        return Ok(res);
    }

    private Guid GetUserIdOrThrow()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var userId))
            throw new UnauthorizedAccessException("Invalid user id.");
        return userId;
    }

}
