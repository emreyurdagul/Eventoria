using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByCodeAsync(string code, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    // Sık ihtiyaç: admin mi? membership içinde arar
    Task<bool> IsEventAdminAsync(Guid eventId, Guid userId, CancellationToken ct);
}
