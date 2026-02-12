using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Infrastructure.Data;
using Eventoria.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<Guid, string?>> GetDisplayNamesByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        return await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.DisplayName })
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName, ct);
    }
}
