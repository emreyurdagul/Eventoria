using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IEventRepository : IRepository<Event>
{
    Task<Event?> GetByCodeAsync(string code, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
}
