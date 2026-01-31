namespace Eventoria.Api.Contracts.Events;

public sealed record JoinEventRequestBody(
    string Code,
    string InviteKey
);
