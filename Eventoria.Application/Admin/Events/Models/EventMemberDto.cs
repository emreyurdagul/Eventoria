using Eventoria.Domain.Enums;

namespace Eventoria.Application.Admin.Events.Models;

public sealed record EventMemberDto(
    Guid UserId,
    string Email,
    string? DisplayName,
    EventRole Role,
    DateTime JoinedAtUtc
);
