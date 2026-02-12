namespace Eventoria.Api.Contracts.Auth;

public sealed record GuestJoinBody(
    Guid EventId,
    string EventCode,
    string InviteKey,
    string? DisplayName  // Opsiyonel
);
