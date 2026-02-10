using Eventoria.Application.Admin.Users.Models;
using Eventoria.Application.Common.Models;
using MediatR;

namespace Eventoria.Application.Admin.Users.GetEventAdmins;

public sealed record GetEventAdminsQuery(
    Guid ActorUserId,
    int Page,
    int PageSize
) : IRequest<PagedResult<EventAdminDto>>;
