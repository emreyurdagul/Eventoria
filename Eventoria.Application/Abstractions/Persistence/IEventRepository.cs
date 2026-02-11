using Eventoria.Application.Admin.Events.Models;
using Eventoria.Application.Common.Models;
using Eventoria.Application.Events.Queries.Models;
using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByIdWithIncludesAsync(Guid eventId, CancellationToken ct);

    Task<Event?> GetByCodeAsync(string code, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task<bool> IsEventAdminAsync(Guid eventId, Guid userId, CancellationToken ct);

    Task<IReadOnlyList<MyEventItem>> GetMyEventsPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct);

    Task<int> CountCreatedByAsync(Guid? adminUserId, CancellationToken ct);
    Task<int> SumParticipantLimitsCreatedByAsync(Guid? adminUserId, CancellationToken ct);
    Task<int> SumParticipantLimitsCreatedByExcludingEventAsync(Guid creatorUserId, Guid excludeEventId, CancellationToken ct);
    Task<bool> IsMemberAsync(Guid eventId, Guid userId, CancellationToken ct);
    Task<IReadOnlyList<MyEventItem>> GetMyEventsAsync(Guid userId, CancellationToken ct);
    Task<EventDetailsDto?> GetEventDetailsAsync(Guid eventId, Guid userId, CancellationToken ct);
    
    Task<PagedResult<EventMemberDto>> GetEventMembersAsync(Guid eventId, int page, int pageSize, CancellationToken ct);
    
    Task DeactivateActiveInvitesAsync(Guid eventId, CancellationToken ct);
}
