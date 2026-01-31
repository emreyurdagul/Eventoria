using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Billing;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Persistence;

public sealed class EventAdminQuotaRepository
    : GenericRepository<EventAdminQuota>, IEventAdminQuotaRepository
{
    private readonly AppDbContext _db;

    public EventAdminQuotaRepository(AppDbContext db) : base(db)
        => _db = db;

    public Task<EventAdminQuota?> GetActiveByAdminIdAsync(Guid adminUserId, CancellationToken ct)
        => _db.EventAdminQuotas
            .FirstOrDefaultAsync(x => x.AdminUserId == adminUserId && x.IsActive, ct);

    public async Task DeactivateAllForAdminAsync(Guid adminUserId, CancellationToken ct)
    {
        var actives = await _db.EventAdminQuotas
            .Where(x => x.AdminUserId == adminUserId && x.IsActive)
            .ToListAsync(ct);

        foreach (var q in actives)
            q.Deactivate();
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);
}
