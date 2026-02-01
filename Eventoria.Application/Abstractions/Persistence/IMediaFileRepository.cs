using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IMediaFileRepository : IRepository<MediaFile>
{


    // ✅ Post / Event senaryosu için gerekli
    Task<bool> ExistsAllAsync(IReadOnlyCollection<Guid> mediaFileIds, CancellationToken ct);

    /// <summary>
    /// Tüm mediaFile'lar bu kullanıcıya mı ait?
    /// </summary>
    Task<bool> AreOwnedByUserAsync(Guid userId, IReadOnlyCollection<Guid> mediaFileIds, CancellationToken ct);

    /// <summary>
    /// Tüm mediaFile'lar bu event'e mi ait? (Event feed'e post basacaksan önerilir)
    /// </summary>
    Task<bool> AreInEventAsync(Guid eventId, IReadOnlyCollection<Guid> mediaFileIds, CancellationToken ct);

    Task<List<MediaFile>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);

}
