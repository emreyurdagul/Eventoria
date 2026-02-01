using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eventoria.Infrastructure.Persistence;

public sealed class MediaFileRepository : GenericRepository<MediaFile>, IMediaFileRepository
{
    private readonly AppDbContext _db;

    public MediaFileRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    // IRepository zaten var; interface’te ayrıca tanımladığın için override gibi dursun
    public override async Task<MediaFile?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _db.Set<MediaFile>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> ExistsAllAsync(IReadOnlyCollection<Guid> mediaFileIds, CancellationToken ct)
    {
        if (mediaFileIds == null || mediaFileIds.Count == 0) return false;

        var count = await _db.Set<MediaFile>()
            .CountAsync(x => mediaFileIds.Contains(x.Id), ct);

        return count == mediaFileIds.Count;
    }

    public async Task<bool> AreOwnedByUserAsync(Guid userId, IReadOnlyCollection<Guid> mediaFileIds, CancellationToken ct)
    {
        if (userId == Guid.Empty) return false;
        if (mediaFileIds == null || mediaFileIds.Count == 0) return false;

        // ⚠️ Buradaki OwnerUserId alan adını senin MediaFile’a göre düzelt:
        var count = await _db.Set<MediaFile>()
            .CountAsync(x =>
                mediaFileIds.Contains(x.Id) &&
                x.OwnerUserId == userId, ct);

        return count == mediaFileIds.Count;
    }

    public async Task<bool> AreInEventAsync(Guid eventId, IReadOnlyCollection<Guid> mediaFileIds, CancellationToken ct)
    {
        if (eventId == Guid.Empty) return false;
        if (mediaFileIds == null || mediaFileIds.Count == 0) return false;

        // ⚠️ Buradaki EventId alan adını senin MediaFile’a göre düzelt:
        var count = await _db.Set<MediaFile>()
            .CountAsync(x =>
                mediaFileIds.Contains(x.Id) &&
                x.EventId == eventId, ct);

        return count == mediaFileIds.Count;
    }

    public Task<List<MediaFile>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    => _db.Set<MediaFile>()
        .AsNoTracking()
        .Where(x => ids.Contains(x.Id))
        .ToListAsync(ct);
}
