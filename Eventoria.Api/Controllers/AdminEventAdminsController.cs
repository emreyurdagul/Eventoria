using Eventoria.Api.Contracts.Admin;
using Eventoria.Application.Admin.EventAdmins.CreateOrAssign;
using Eventoria.Application.Admin.Users.GetEventAdmins;
using Eventoria.Application.Admin.Users.Models;
using Eventoria.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/admin/event-admins")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser")]
public sealed class AdminEventAdminsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminEventAdminsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CreateOrAssignEventAdminResult>> CreateOrAssign(
        [FromBody] CreateOrAssignEventAdminBody body,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var actorId))
            return Unauthorized();

        var res = await _mediator.Send(
            new CreateOrAssignEventAdminCommand(actorId, body.Email, body.TempPassword),
            ct);

        return Ok(res);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<EventAdminDto>>> GetEventAdmins(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdStr, out var actorId))
            return Unauthorized();

        var res = await _mediator.Send(
            new GetEventAdminsQuery(actorId, page, pageSize),
            ct);

        return Ok(res);
    }
}
