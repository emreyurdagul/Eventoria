using Eventoria.Application.Abstractions.Identity;
using Eventoria.Application.Admin.Users.Models;
using Eventoria.Application.Common.Models;
using MediatR;

namespace Eventoria.Application.Admin.Users.GetEventAdmins;

public sealed class GetEventAdminsHandler : IRequestHandler<GetEventAdminsQuery, PagedResult<EventAdminDto>>
{
    private readonly IAdminIdentityService _identity;

    public GetEventAdminsHandler(IAdminIdentityService identity)
    {
        _identity = identity;
    }

    public async Task<PagedResult<EventAdminDto>> Handle(GetEventAdminsQuery query, CancellationToken ct)
    {
        if (query.ActorUserId == Guid.Empty)
            throw new InvalidOperationException("ActorUserId is required.");

        var isSuperUser = await _identity.IsInRoleAsync(query.ActorUserId, "SuperUser", ct);
        if (!isSuperUser)
            throw new UnauthorizedAccessException("Only SuperUser can list EventAdmins.");

        return await _identity.GetUsersByRoleAsync("EventAdmin", query.Page, query.PageSize, ct);
    }
}
