using Eventoria.Application.Auth.GoogleLogin;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    // Frontend örnek:
    // window.location = "https://api.etkinlikgalerim.online/api/auth/external/google?returnUrl=/somepage"
    [HttpGet("google")]
    [AllowAnonymous]
    public IActionResult Google([FromQuery] string? returnUrl = null)
    {
        // Google login tamamlanınca uygulama içinde buraya döneceğiz (senin action)
        var appCallback = Url.Action(nameof(GoogleCallback), "AuthExternal");
        if (string.IsNullOrWhiteSpace(appCallback))
            throw new InvalidOperationException("Could not build GoogleCallback url.");

        var props = new AuthenticationProperties
        {
            RedirectUri = appCallback
        };

        if (!string.IsNullOrWhiteSpace(returnUrl))
            props.Items["returnUrl"] = returnUrl;

        return Challenge(props, "Google");
    }


    // 2) Google callback
    // Google Console Redirect URI:
    // https://api.etkinlikgalerim.online/api/auth/external/google/callback
    [HttpGet("google/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? mode = null, CancellationToken ct = default)
    {
        // ✅ Identity’nin external cookie scheme’i
        var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!authResult.Succeeded || authResult.Principal is null)
            return Redirect(GetErrorRedirect());

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
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return Redirect(GetErrorRedirect());
        }

        // returnUrl’i state içinden geri al
        string? returnUrl = null;

        if (authResult.Properties?.Items != null &&
            authResult.Properties.Items.TryGetValue("returnUrl", out var tmp))
        {
            returnUrl = tmp;
        }

        var safeReturn = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;


        var tokens = await _mediator.Send(
            new GoogleLoginCommand(providerKey, email.Trim().ToLowerInvariant(), firstName, lastName),
            ct);

        // external cookie temizle
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Debug / mobile / postman
        if (string.Equals(mode, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(tokens);

        // ✅ Frontend success sayfasına yönlendir
        // Tokenları fragment (#) içinde taşımak, query’ye göre daha az loglanır.
        // Örn: https://etkinlikgalerim.online/auth/oauth-success#accessToken=...&refreshToken=...&returnUrl=...
        var successBase = GetSuccessRedirect();
        var fragment =
            $"accessToken={Uri.EscapeDataString(tokens.AccessToken)}" +
            $"&refreshToken={Uri.EscapeDataString(tokens.RefreshToken)}" +
            $"&returnUrl={Uri.EscapeDataString(safeReturn)}";

        // successBase zaten içinde # varsa (nadiren), ona göre birleştir
        var redirectUrl = successBase.Contains('#')
            ? $"{successBase}&{fragment}"
            : $"{successBase}#{fragment}";

        return Redirect(redirectUrl);
    }

    [HttpGet("debug/config")]
    [AllowAnonymous]
    public IActionResult DebugConfig([FromServices] IWebHostEnvironment env)
    {
        return Ok(new
        {
            env = env.EnvironmentName,
            success = _config["Frontend:OAuthSuccessRedirect"],
            error = _config["Frontend:OAuthErrorRedirect"]
        });
    }


    private string GetSuccessRedirect()
        => _config["Frontend:OAuthSuccessRedirect"]
           ?? "http://localhost:5173/auth/oauth-success";

    private string GetErrorRedirect()
        => _config["Frontend:OAuthErrorRedirect"]
           ?? "http://localhost:5173/auth/oauth-error";

}
