using Eventoria.Domain.Entities;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IMediaFileRepository : IRepository<MediaFile>
{
    Task<MediaFile?> GetByIdAsync(Guid id, CancellationToken ct); // (IRepository zaten var ama net olsun diye)
}
