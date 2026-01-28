using Eventoria.Application.Auth;
using Eventoria.Application.Auth.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest req)
        => Ok(await _auth.RegisterAsync(req));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
        => Ok(await _auth.LoginAsync(req));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest req)
        => Ok(await _auth.RefreshAsync(req));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest req)
    {
        await _auth.LogoutAsync(req);
        return Ok(new { message = "Logged out." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        var token = await _auth.GeneratePasswordResetTokenAsync(req);
        // DEV MODE: token dön (prod’da mail)
        return Ok(new { message = "If the account exists, a reset link will be sent.", token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        await _auth.ResetPasswordAsync(req);
        return Ok(new { message = "Password reset successful." });
    }
}
