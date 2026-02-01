namespace Eventoria.Api.Contracts.Admin;

public sealed record CreateOrAssignEventAdminBody(
    string Email,
    string? TempPassword
);
