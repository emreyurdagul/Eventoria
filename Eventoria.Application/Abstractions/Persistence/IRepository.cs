using Eventoria.Domain.Common;
using System.Linq.Expressions;

namespace Eventoria.Application.Abstractions.Persistence;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(TEntity entity, CancellationToken ct);
    void Update(TEntity entity);
    void Remove(TEntity entity);

    IQueryable<TEntity> Query(); // Read tarafı için
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
}
