namespace Eventoria.Application.Events.Invites.Generate;

public sealed record GenerateInviteKeyResult(
    string InviteKey,
    DateTime GeneratedAtUtc
);
