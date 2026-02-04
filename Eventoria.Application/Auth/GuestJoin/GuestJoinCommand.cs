using MediatR;
using Eventoria.Application.Auth.Contracts;

namespace Eventoria.Application.Auth.GuestJoin;

public sealed record GuestJoinCommand(
    Guid EventId,
    string EventCode,
    string InviteKey,
    string DisplayName
) : IRequest<GuestAuthResponse>;
