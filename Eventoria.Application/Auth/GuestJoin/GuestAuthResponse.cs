namespace Eventoria.Application.Auth.Contracts;

public sealed record GuestAuthResponse(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    bool IsGuest,
    Guid EventId,
    string DisplayName
);
