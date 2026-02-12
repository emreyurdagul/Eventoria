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
    /// Creates a new invite key without deactivating previous ones
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
    /// Get all active invite keys (Event Admin only)
    /// Returns all active invites with decrypted keys
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<GetActiveInvitesListResult>> GetActiveInvites(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetActiveInviteQuery(_current.UserId, eventId),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Deactivate specific invite key (Event Admin only)
    /// Prevents new users from joining with the deactivated key
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
