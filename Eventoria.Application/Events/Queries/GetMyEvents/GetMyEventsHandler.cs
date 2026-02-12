using Eventoria.Application.Abstractions.Auth;
using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Common.Models;
using Eventoria.Application.Events.Queries.GetMyEvents;
using Eventoria.Application.Events.Queries.Models;
using MediatR;

public sealed class GetMyEventsHandler
    : IRequestHandler<GetMyEventsQuery, PagedResult<MyEventItem>>
{
    private readonly IEventRepository _events;
    private readonly ICurrentUserService _currentUser;

    public GetMyEventsHandler(IEventRepository events, ICurrentUserService currentUser)
    {
        _events = events;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<MyEventItem>> Handle(GetMyEventsQuery q, CancellationToken ct)
    {
        if (q.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (q.Page <= 0)
            throw new InvalidOperationException("Page must be >= 1.");

        if (q.PageSize <= 0 || q.PageSize > 100)
            throw new InvalidOperationException("PageSize must be between 1 and 100.");

        // SuperAdmin kontrolü
        if (_currentUser.IsSuperAdmin)
        {
            // SuperAdmin tüm etkinlikleri görür
            var allTotal = await _events.CountAllEventsAsync(ct);

            if (allTotal == 0)
            {
                return new PagedResult<MyEventItem>(
                    Items: Array.Empty<MyEventItem>(),
                    Page: q.Page,
                    PageSize: q.PageSize,
                    TotalCount: 0
                );
            }

            var allItems = await _events.GetAllEventsPagedAsync(
                page: q.Page,
                pageSize: q.PageSize,
                ct);

            return new PagedResult<MyEventItem>(
                Items: allItems,
                Page: q.Page,
                PageSize: q.PageSize,
                TotalCount: allTotal
            );
        }

        // Normal kullanýcý - sadece üyesi olduðu etkinlikleri görür
        var totalCount = await _events.CountMyEventsAsync(q.UserId, ct);

        if (totalCount == 0)
        {
            return new PagedResult<MyEventItem>(
                Items: Array.Empty<MyEventItem>(),
                Page: q.Page,
                PageSize: q.PageSize,
                TotalCount: 0
            );
        }

        var items = await _events.GetMyEventsPagedAsync(
            userId: q.UserId,
            page: q.Page,
            pageSize: q.PageSize,
            ct);

        return new PagedResult<MyEventItem>(
            Items: items,
            Page: q.Page,
            PageSize: q.PageSize,
            TotalCount: totalCount
        );
    }
}
