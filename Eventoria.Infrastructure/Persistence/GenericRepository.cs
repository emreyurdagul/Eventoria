using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Eventoria.Infrastructure.Persistence;

public class GenericRepository<TEntity>
    : IRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly DbSet<TEntity> Set;
    protected readonly DbContext Db;

    public GenericRepository(DbContext db)
    {
        Db = db;
        Set = db.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct)
        => await Set.FindAsync(new object[] { id }, ct);

    public virtual Task AddAsync(TEntity entity, CancellationToken ct)
        => Set.AddAsync(entity, ct).AsTask();

    public virtual void Update(TEntity entity) => Set.Update(entity);

    public virtual void Remove(TEntity entity) => Set.Remove(entity);

    public virtual IQueryable<TEntity> Query() => Set.AsQueryable();

    public virtual Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
        => Set.AnyAsync(predicate, ct);
}
