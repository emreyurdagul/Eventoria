namespace Eventoria.Application.Auth.Abstractions;

public interface IExternalIdentityService
{
    /// <summary>
    /// Google callback sonrası user'ı bulur veya oluşturur.
    /// </summary>
    Task<Guid> GetOrCreateUserFromGoogleAsync(
        string providerKey,
        string email,
        string? firstName,
        string? lastName,
        CancellationToken ct);
}
