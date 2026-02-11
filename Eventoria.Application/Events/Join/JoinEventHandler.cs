using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using Eventoria.Domain.Enums;
using MediatR;

namespace Eventoria.Application.Events.Join;

public sealed class JoinEventHandler : IRequestHandler<JoinEventCommand, JoinEventResult>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly IEventTokenService _tokens;

    public JoinEventHandler(IEventRepository events, IUnitOfWork uow, IEventTokenService tokens)
    {
        _events = events;
        _uow = uow;
        _tokens = tokens;
    }

    public async Task<JoinEventResult> Handle(JoinEventCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (string.IsNullOrWhiteSpace(cmd.Code) || string.IsNullOrWhiteSpace(cmd.InviteKey))
            throw new InvalidOperationException("Code and InviteKey are required.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            var ev = await _events.GetByCodeAsync(cmd.Code.Trim(), innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            // Zaten üye mi?
            var alreadyMember = await _events.IsMemberAsync(ev.Id, cmd.UserId, innerCt);
            if (alreadyMember)
                return new JoinEventResult(ev.Id); // idempotent

            // Kapasite
            if (ev.Memberships.Count >= ev.Specs.ParticipantLimit)
                throw new InvalidOperationException("Event is full.");

            // Gönderilen invite key'in hash'ini hesapla
            var inviteHash = _tokens.Sha256Hex(cmd.InviteKey.Trim());

            // Tüm aktif invite'lar içinde bu hash'e sahip olan var mý kontrol et
            var matchingInvite = ev.Invites
                .Where(x => x.IsActive)
                .FirstOrDefault(x => _tokens.FixedTimeEquals(x.InviteKeyHash, inviteHash));

            if (matchingInvite == null)
                throw new UnauthorizedAccessException("Invalid or inactive invite key.");

            // Membership ekle
            ev.AddMembership(cmd.UserId, EventRole.Participant);

            await _uow.SaveChangesAsync(innerCt);

            return new JoinEventResult(ev.Id);
        }, ct);
    }
}
