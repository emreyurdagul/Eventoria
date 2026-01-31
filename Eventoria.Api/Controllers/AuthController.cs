using Eventoria.Application.Auth.Contracts;
using Eventoria.Application.Auth.ForgotPassword;
using Eventoria.Application.Auth.Login;
using Eventoria.Application.Auth.Logout;
using Eventoria.Application.Auth.Refresh;
using Eventoria.Application.Auth.Register;
using Eventoria.Application.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
        => Ok(await _mediator.Send(new RegisterCommand(req), ct));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
        => Ok(await _mediator.Send(new LoginCommand(req), ct));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
        => Ok(await _mediator.Send(new RefreshCommand(req), ct));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest req, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(req), ct);
        return Ok(new { message = "Logged out." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        var token = await _mediator.Send(new ForgotPasswordCommand(req), ct);

        // DEV MODE: token dön (prod’da mail)
        return Ok(new { message = "If the account exists, a reset link will be sent.", token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ResetPasswordCommand(req), ct);
        return Ok(new { message = "Password reset successful." });
    }
}
