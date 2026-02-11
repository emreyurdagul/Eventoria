using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Events.Invites.GetActive;

public sealed class GetActiveInviteHandler : IRequestHandler<GetActiveInviteQuery, GetActiveInviteResult?>
{
    private readonly IEventRepository _events;

    public GetActiveInviteHandler(IEventRepository events)
    {
        _events = events;
    }

    public async Task<GetActiveInviteResult?> Handle(GetActiveInviteQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (query.EventId == Guid.Empty)
            throw new InvalidOperationException("EventId is required.");

        // Only Event Admin can view invite status
        var isAdmin = await _events.IsEventAdminAsync(query.EventId, query.UserId, ct);
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only Event Admin can view invite keys.");

        var ev = await _events.GetByIdWithIncludesAsync(query.EventId, ct)
            ?? throw new InvalidOperationException("Event not found.");

        var activeInvite = ev.Invites.FirstOrDefault(i => i.IsActive);
        
        if (activeInvite == null)
            return null;

        return new GetActiveInviteResult(
            activeInvite.Id,
            activeInvite.IsActive,
            activeInvite.CreatedAtUtc
        );
    }
}
