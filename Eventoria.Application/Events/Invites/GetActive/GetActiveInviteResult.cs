namespace Eventoria.Application.Events.Invites.GetActive;

public sealed record GetActiveInviteResult(
    Guid InviteId,
    bool IsActive,
    DateTime CreatedAtUtc
);
