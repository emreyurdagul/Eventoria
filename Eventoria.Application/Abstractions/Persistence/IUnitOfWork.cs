namespace Eventoria.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);

    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct);
}
