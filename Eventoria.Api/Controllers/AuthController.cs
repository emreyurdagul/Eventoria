using Eventoria.Api.Contracts.Auth;
using Eventoria.Api.Security;
using Eventoria.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IJwtTokenService _jwt;

    public AuthController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        IJwtTokenService jwt)
    {
        _users = users;
        _signIn = signIn;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        var exists = await _users.FindByEmailAsync(email);
        if (exists != null)
            return BadRequest(new { error = "Email already registered." });

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // Register sonrasý login token dönmek iyi DX
        var token = await _jwt.CreateAsync(user);
        return Ok(new AuthResponse(token));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email);
        if (user == null)
            return Unauthorized(new { error = "Invalid credentials." });

        var result = await _signIn.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized(new { error = "Invalid credentials." });

        var token = await _jwt.CreateAsync(user);
        return Ok(new AuthResponse(token));
    }

    // Þimdilik email göndermek yok -> token üretip döndürüyoruz (dev)
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email);

        // Security: email var/yok ayýrt etme
        if (user == null) return Ok(new { message = "If the account exists, a reset link will be sent." });

        var token = await _users.GeneratePasswordResetTokenAsync(user);

        // DEV MODE: token dön
        return Ok(new { message = "Password reset token generated.", token });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(email);
        if (user == null)
            return BadRequest(new { error = "Invalid request." });

        var result = await _users.ResetPasswordAsync(user, req.Token, req.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return Ok(new { message = "Password reset successful." });
    }
}
