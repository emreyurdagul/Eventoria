using Eventoria.Application.Admin.Events.GetEventMembers;
using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/admin/events")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser")]
public sealed class AdminEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{eventId:guid}/members")]
    public async Task<ActionResult<PagedResult<EventMemberDto>>> GetEventMembers(
        Guid eventId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var actorId))
            return Unauthorized();

        var res = await _mediator.Send(
            new GetEventMembersQuery(actorId, eventId, page, pageSize),
            ct);

        return Ok(res);
    }
}
