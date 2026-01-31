using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByIdWithIncludesAsync(Guid eventId, CancellationToken ct);

    Task<Event?> GetByCodeAsync(string code, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    Task<bool> IsEventAdminAsync(Guid eventId, Guid userId, CancellationToken ct);

    // ✅ Quota için gerekli
    Task<int> CountCreatedByAsync(Guid adminUserId, CancellationToken ct);
    Task<int> SumParticipantLimitsCreatedByAsync(Guid adminUserId, CancellationToken ct);

}
