using Eventoria.Application.Auth.Contracts;
using Eventoria.Application.Auth.External.Contracts;

namespace Eventoria.Application.Auth.External;

public interface IExternalAuthService
{
    /// <summary>
    /// Google callback sonrası external login info ile user bul/oluştur ve JWT+Refresh üret.
    /// </summary>
    Task<AuthResponse> SignInWithGoogleAsync(CancellationToken ct);
}
