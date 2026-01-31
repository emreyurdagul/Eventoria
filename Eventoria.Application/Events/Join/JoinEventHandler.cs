using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Domain.Enums;
using MediatR;

namespace Eventoria.Application.Events.Join;

public sealed class JoinEventHandler : IRequestHandler<JoinEventCommand, JoinEventResult>
{
    private readonly IEventRepository _events;
    private readonly IEventTokenService _tokens;

    public JoinEventHandler(IEventRepository events, IEventTokenService tokens)
    {
        _events = events;
        _tokens = tokens;
    }

    public async Task<JoinEventResult> Handle(JoinEventCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty) throw new InvalidOperationException("UserId is required.");
        if (string.IsNullOrWhiteSpace(cmd.Code) || string.IsNullOrWhiteSpace(cmd.InviteKey))
            throw new InvalidOperationException("Code and InviteKey are required.");

        var ev = await _events.GetByCodeAsync(cmd.Code.Trim(), ct)
            ?? throw new InvalidOperationException("Event not found.");

        var inviteHash = _tokens.Sha256Hex(cmd.InviteKey.Trim());

        var active = ev.Invites.FirstOrDefault(x => x.IsActive);
        if (active == null) throw new InvalidOperationException("Invite is not active.");

        if (!_tokens.FixedTimeEquals(active.InviteKeyHash, inviteHash))
            throw new UnauthorizedAccessException("Invalid invite key.");

        ev.AddMembership(cmd.UserId, EventRole.Participant);

        return new JoinEventResult(ev.Id);
    }
}
