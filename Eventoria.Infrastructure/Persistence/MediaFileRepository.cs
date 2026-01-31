using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using Eventoria.Infrastructure.Data;

namespace Eventoria.Infrastructure.Persistence;

public sealed class MediaFileRepository : GenericRepository<MediaFile>, IMediaFileRepository
{
    public MediaFileRepository(AppDbContext db) : base(db) { }
}
