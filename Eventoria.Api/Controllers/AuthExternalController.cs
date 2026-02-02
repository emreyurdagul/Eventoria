using Eventoria.Application.Auth.GoogleLogin;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Eventoria.Api.Controllers;

[ApiController]
[Route("api/auth/external")]
public sealed class AuthExternalController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public AuthExternalController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config = config;
    }

    // 1) Google login başlat
    // Frontend: window.location = /api/auth/external/google?returnUrl=/somepage
    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult Google([FromQuery] string? returnUrl = null)
    {
        // Callback'e geri dönünce buraya düşecek
        var callbackUrl = Url.ActionLink(
            action: nameof(GoogleCallback),
            controller: "AuthExternal",
            values: new { returnUrl });

        var props = new AuthenticationProperties
        {
            RedirectUri = callbackUrl
        };

        return Challenge(props, "Google");
    }

    // 2) Google callback
    // Default davranış: frontend'e redirect (tokenları hash ile geçiyoruz)
    // İstersen ?mode=json ile JSON döndürür
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback(
        [FromQuery] string? returnUrl = null,
        [FromQuery] string? mode = null,
        CancellationToken ct = default)
    {
        // Google principal genellikle External cookie scheme’de olur:
        var authResult = await HttpContext.AuthenticateAsync("External");
        if (!authResult.Succeeded || authResult.Principal == null)
            return Unauthorized();

        var principal = authResult.Principal;

        var email =
            principal.FindFirstValue(ClaimTypes.Email)
            ?? principal.FindFirstValue("email");

        var providerKey =
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        var firstName = principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = principal.FindFirstValue(ClaimTypes.Surname);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerKey))
            return Unauthorized();

        var tokens = await _mediator.Send(
            new GoogleLoginCommand(providerKey, email, firstName, lastName),
            ct);

        // external cookie temizle
        await HttpContext.SignOutAsync("External");

        // JSON isteyenler için (debug / mobile / postman)
        if (string.Equals(mode, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(tokens);

        // Default: frontend’e redirect
        var baseUrl = _config["Frontend:BaseUrl"] ?? "http://localhost:5173";
        var safeReturn = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;

        // Tokenları query yerine hash ile göndermek daha iyi (server loglarında görünmez)
        // http://frontend/#/auth/callback?access=...&refresh=...&returnUrl=...
        var redirectUrl =
            $"{baseUrl}#/auth/callback" +
            $"?accessToken={Uri.EscapeDataString(tokens.AccessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(tokens.RefreshToken)}" +
            $"&returnUrl={Uri.EscapeDataString(safeReturn)}";

        return Redirect(redirectUrl);
    }
}
