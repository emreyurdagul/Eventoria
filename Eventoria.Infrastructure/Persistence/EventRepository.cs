using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using Eventoria.Domain.Enums;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Persistence;

public class EventRepository : GenericRepository<Event>, IEventRepository
{
    private readonly AppDbContext _db;

    public EventRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<Event?> GetByIdWithIncludesAsync(Guid eventId, CancellationToken ct)
    => await _db.Events
        .Include(e => e.Invites)
        .Include(e => e.Memberships)
        .FirstOrDefaultAsync(e => e.Id == eventId, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => _db.Events.AnyAsync(e => e.Code == code, ct);

    public async Task<Event?> GetByCodeAsync(string code, CancellationToken ct)
        => await _db.Events
            .Include(e => e.Invites)
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Code == code, ct);

    public Task<bool> IsEventAdminAsync(Guid eventId, Guid userId, CancellationToken ct)
        => _db.EventMemberships.AnyAsync(m =>
            m.EventId == eventId &&
            m.UserId == userId &&
            m.Role == EventRole.Admin, ct);

    public Task<int> CountCreatedByAsync(Guid adminUserId, CancellationToken ct)
    => _db.Events.CountAsync(e => e.CreatedByUserId == adminUserId, ct);

    public async Task<int> SumParticipantLimitsCreatedByAsync(Guid adminUserId, CancellationToken ct)
    {
        // Eğer Specs null olabilir diyorsan:
        // return await _db.Events
        //   .Where(e => e.CreatedByUserId == adminUserId)
        //   .SumAsync(e => (int?)e.Specs.ParticipantLimit ?? 0, ct);

        return await _db.Events
            .Where(e => e.CreatedByUserId == adminUserId)
            .SumAsync(e => e.Specs.ParticipantLimit, ct);
    }

}
