namespace Eventoria.Application.Auth.External.Contracts;

public sealed record ExternalAuthResult(
    string AccessToken,
    string RefreshToken
);
