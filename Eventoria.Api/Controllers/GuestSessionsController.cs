using Eventoria.Api.Contracts.GuestSession;
using Eventoria.Application.Events.GuestSession;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/events/guest-session")]
public sealed class GuestSessionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GuestSessionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<CreateGuestSessionResult>> Create([FromBody] CreateGuestSessionBody body, CancellationToken ct)
    {
        var cmd = new CreateGuestSessionCommand(
            Code: body.Code,
            InviteKey: body.InviteKey,
            DisplayName: body.DisplayName);

        var res = await _mediator.Send(cmd, ct);
        return Ok(res);
    }
}

