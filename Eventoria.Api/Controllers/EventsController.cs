using Eventoria.Api.Contracts.Events;
using Eventoria.Application.Abstractions;
using Eventoria.Application.Abstractions.Auth;
using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using Eventoria.Application.Events.Create;
using Eventoria.Application.Events.Join;
using Eventoria.Application.Events.Queries.GetEventDetails;
using Eventoria.Application.Events.Queries.GetEventMediaFeed;
using Eventoria.Application.Events.Queries.GetMyEventMembers;
using Eventoria.Application.Events.Queries.GetMyEvents;
using Eventoria.Application.Events.Queries.Models;
using Eventoria.Application.Events.Update;
using Eventoria.Application.Posts.Queries.GetEventPosts;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _current;

    public EventsController(IMediator mediator, ICurrentUserService current)
    {
        _mediator = mediator;
        _current = current;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<CreateEventResult>> Create([FromBody] CreateEventRequestBody body, CancellationToken ct)
    {
        var cmd = new CreateEventCommand(
            _current.UserId,
            body.Title,
            body.Description,
            body.Date,
            body.ParticipantLimit,
            body.PhotosPerUserLimit,
            body.VideosPerUserLimit
        );

        return Ok(await _mediator.Send(cmd, ct));
    }

    [HttpPut("{eventId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<UpdateEventResult>> Update(Guid eventId, [FromBody] UpdateEventRequestBody body, CancellationToken ct)
    {
        var cmd = new UpdateEventCommand(
            _current.UserId,
            eventId,
            body.Title,
            body.Description,
            body.Date,
            body.ParticipantLimit,
            body.PhotosPerUserLimit,
            body.VideosPerUserLimit
        );

        return Ok(await _mediator.Send(cmd, ct));
    }

    [HttpPost("join")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<JoinEventResult>> Join([FromBody] JoinEventBody body, CancellationToken ct)
    {
        var cmd = new JoinEventCommand(_current.UserId, body.Code, body.InviteKey);
        return Ok(await _mediator.Send(cmd, ct));
    }

    [HttpGet("{eventId:guid}/posts")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<GetEventPostsResult>> GetPosts(
        Guid eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var res = await _mediator.Send(new GetEventPostsQuery(_current.UserId, eventId, page, pageSize), ct);
        return Ok(res);
    }

    [HttpGet("{eventId:guid}/media-feed")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<GetEventMediaFeedResult>> GetMediaFeed(
        Guid eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var res = await _mediator.Send(
            new GetEventMediaFeedQuery(eventId, _current.UserId, page, pageSize),
            ct);
        return Ok(res);
    }

    [HttpGet("my")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<PagedResult<MyEventItem>>> GetMyEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var res = await _mediator.Send(new GetMyEventsQuery(_current.UserId, page, pageSize), ct);
        return Ok(res);
    }

    [HttpGet("{eventId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<GetEventDetailsResult>> GetEventDetails(Guid eventId, CancellationToken ct)
    {
        var res = await _mediator.Send(new GetEventDetailsQuery(_current.UserId, eventId), ct);
        return Ok(res);
    }

    [HttpGet("{eventId:guid}/members")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<PagedResult<EventMemberDto>>> GetMyEventMembers(
        Guid eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var res = await _mediator.Send(
            new GetMyEventMembersQuery(_current.UserId, eventId, page, pageSize),
            ct);

        return Ok(res);
    }
}
