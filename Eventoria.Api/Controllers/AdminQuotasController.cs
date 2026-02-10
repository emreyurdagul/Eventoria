using Eventoria.Api.Contracts.AdminQuotas;
using Eventoria.Application.Billing.AssignQuota;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/admin/quotas")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperUser")]
public sealed class AdminQuotasController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminQuotasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("assign")]
    public async Task<ActionResult<AssignQuotaResult>> Assign(
        [FromBody] AssignQuotaBody body,
        CancellationToken ct)
    {
        var cmd = new AssignQuotaCommand(
            body.UserId,
            body.MaxEvents,
            body.MaxTotalParticipants);

        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }
}

