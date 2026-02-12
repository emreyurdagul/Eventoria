namespace Eventoria.Application.Events.Invites.GetActive;

public sealed record GetActiveInviteResult(
    Guid InviteId,
    bool IsActive,
    string EventCode,
    string? InviteKey,      // Decrypted plaintext key
    DateTime CreatedAtUtc,
    string Message
);

public sealed record GetActiveInvitesListResult(
    IReadOnlyList<GetActiveInviteResult> Invites
);
