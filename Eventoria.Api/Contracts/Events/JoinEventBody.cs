namespace Eventoria.Api.Contracts.Events;

public sealed record JoinEventBody(
    string Code,
    string InviteKey
);
