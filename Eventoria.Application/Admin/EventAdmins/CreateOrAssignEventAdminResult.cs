namespace Eventoria.Application.Admin.EventAdmins.CreateOrAssign;

public sealed record CreateOrAssignEventAdminResult(
    Guid UserId,
    string Email,
    bool UserCreated,
    string AssignedRole
);
