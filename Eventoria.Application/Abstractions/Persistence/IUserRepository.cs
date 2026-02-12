namespace Eventoria.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<Dictionary<Guid, string?>> GetDisplayNamesByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct);
}
