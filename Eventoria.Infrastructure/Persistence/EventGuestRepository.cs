using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using Eventoria.Infrastructure.Data;

namespace Eventoria.Infrastructure.Persistence;

public sealed class EventGuestRepository : GenericRepository<EventGuest>, IEventGuestRepository
{
    public EventGuestRepository(AppDbContext db) : base(db) { }

    public new Task<EventGuest?> GetByIdAsync(Guid id, CancellationToken ct)
        => base.GetByIdAsync(id, ct);
}
