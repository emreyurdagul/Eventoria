using Eventoria.Api.Contracts.Events;
using Eventoria.Application.Events.Create;
using Eventoria.Application.Events.Join;
using Eventoria.Application.Events.Update;
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
}
