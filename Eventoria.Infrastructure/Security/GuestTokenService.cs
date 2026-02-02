using Eventoria.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Eventoria.Infrastructure.Security;

public sealed class GuestTokenService : IGuestTokenService
{
    private readonly IConfiguration _config;

    public GuestTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateGuestToken(Guid guestId, Guid eventId, string displayName, TimeSpan validFor)
    {
        var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer = _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        var audience = _config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");

        var claims = new List<Claim>
        {
            // sub = guestId
            new(JwtRegisteredClaimNames.Sub, guestId.ToString()),
            new(ClaimTypes.NameIdentifier, guestId.ToString()),

            // event scope
            new("eventId", eventId.ToString()),

            // actor type
            new("actorType", "guest"),

            // display name
            new("displayName", displayName),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(validFor),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
