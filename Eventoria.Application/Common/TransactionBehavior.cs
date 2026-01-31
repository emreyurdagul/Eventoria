using Eventoria.Application.Abstractions.Persistence;
using MediatR;

namespace Eventoria.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;

    public TransactionBehavior(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Her request transaction olmak zorunda değilse marker interface ile filtreleriz.
        return await _uow.ExecuteInTransactionAsync(async _ => await next(), ct);
    }
}
