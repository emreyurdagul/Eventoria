using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Events.Invites.Deactivate;

public sealed class DeactivateInviteKeyHandler : IRequestHandler<DeactivateInviteKeyCommand, DeactivateInviteKeyResult>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;

    public DeactivateInviteKeyHandler(IEventRepository events, IUnitOfWork uow)
    {
        _events = events;
        _uow = uow;
    }

    public async Task<DeactivateInviteKeyResult> Handle(DeactivateInviteKeyCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (cmd.EventId == Guid.Empty)
            throw new InvalidOperationException("EventId is required.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            var ev = await _events.GetByIdWithIncludesAsync(cmd.EventId, innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            // Only Event Admin can deactivate invite keys
            var isAdmin = await _events.IsEventAdminAsync(cmd.EventId, cmd.UserId, innerCt);
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only Event Admin can deactivate invite keys.");

            var activeInvite = ev.Invites.FirstOrDefault(i => i.IsActive);
            
            if (activeInvite == null)
                throw new InvalidOperationException("No active invite key found.");

            activeInvite.Deactivate();

            await _uow.SaveChangesAsync(innerCt);

            return new DeactivateInviteKeyResult(true, DateTime.UtcNow);
        }, ct);
    }
}
