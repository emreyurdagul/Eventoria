using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Eventoria.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public UnitOfWork(AppDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct)
        => _db.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            await action(ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        });
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var result = await action(ct);
                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // İstersen burada logla (hangi entity patladı)
                foreach (var e in ex.Entries)
                    Console.WriteLine($"Concurrency: {e.Metadata.Name} | State: {e.State}");

                await tx.RollbackAsync(ct);

                throw new DbUpdateConcurrencyException("Optimistic concurrency conflict.", ex);
            }
        });
    }
}
