using Eventoria.Domain.Billing;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IEventAdminQuotaRepository : IRepository<EventAdminQuota>
{
    Task<EventAdminQuota?> GetActiveByAdminIdAsync(Guid adminUserId, CancellationToken ct);
    Task DeactivateAllForAdminAsync(Guid adminUserId, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
