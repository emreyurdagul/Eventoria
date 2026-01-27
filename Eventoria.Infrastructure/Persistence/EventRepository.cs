using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
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

    public async Task<Event?> GetByCodeAsync(string code, CancellationToken ct)
        => await _db.Events
            .Include(e => e.Invites)
            .Include(e => e.Memberships)
            .FirstOrDefaultAsync(e => e.Code == code, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => _db.Events.AnyAsync(e => e.Code == code, ct);
}
