namespace Eventoria.Application.Auth.Abstractions;

public interface IRefreshTokenStore
{
    Task<string> IssueAsync(Guid userId, CancellationToken ct);                 // raw token döner
    Task<Guid?> ValidateAndRotateAsync(string rawRefreshToken, CancellationToken ct); // userId döner (rotate + revoke)
    Task RevokeAsync(string rawRefreshToken, CancellationToken ct);
}
