namespace Eventoria.Application.Auth.Abstractions;

public interface IJwtTokenService
{
    Task<string> CreateAccessTokenAsync(Guid userId, CancellationToken ct);
}
