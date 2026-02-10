namespace Eventoria.Application.Admin.Users.Models;

public sealed record EventAdminDto(
    Guid Id, 
    string Email, 
    string? DisplayName,
    DateTime CreatedAtUtc
);
