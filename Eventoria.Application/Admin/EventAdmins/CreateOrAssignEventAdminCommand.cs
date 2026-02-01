using MediatR;

namespace Eventoria.Application.Admin.EventAdmins.CreateOrAssign;

public sealed record CreateOrAssignEventAdminCommand(
    Guid ActorUserId,
    string Email,
    string? TempPassword
) : IRequest<CreateOrAssignEventAdminResult>;
