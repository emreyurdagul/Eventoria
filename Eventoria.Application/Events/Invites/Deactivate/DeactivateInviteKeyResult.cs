namespace Eventoria.Application.Events.Invites.Deactivate;

public sealed record DeactivateInviteKeyResult(
    bool Success,
    DateTime DeactivatedAtUtc
);
