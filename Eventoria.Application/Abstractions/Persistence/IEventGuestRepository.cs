using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IEventGuestRepository : IRepository<EventGuest>
{
    Task<EventGuest?> GetByIdAsync(Guid id, CancellationToken ct);
}
