using Eventoria.Application.Abstractions.Auth;
using Eventoria.Application.Events.Invites.Deactivate;
using Eventoria.Application.Events.Invites.Generate;
using Eventoria.Application.Events.Invites.GetActive;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/invites")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class EventInvitesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _current;

    public EventInvitesController(IMediator mediator, ICurrentUserService current)
    {
        _mediator = mediator;
        _current = current;
    }

    /// <summary>
    /// Generate new invite key for event (Event Admin only)
    /// Automatically deactivates previous invite keys
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<GenerateInviteKeyResult>> GenerateInviteKey(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GenerateInviteKeyCommand(_current.UserId, eventId),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Get active invite key status (Event Admin only)
    /// Does NOT return the actual key - only metadata
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<GetActiveInviteResult>> GetActiveInvite(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetActiveInviteQuery(_current.UserId, eventId),
            ct);

        if (result == null)
            return NotFound(new { message = "No active invite key found." });

        return Ok(result);
    }

    /// <summary>
    /// Deactivate current invite key (Event Admin only)
    /// Prevents new users from joining with the old key
    /// </summary>
    [HttpPost("deactivate")]
    public async Task<ActionResult<DeactivateInviteKeyResult>> DeactivateInviteKey(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new DeactivateInviteKeyCommand(_current.UserId, eventId),
            ct);

        return Ok(result);
    }
}
