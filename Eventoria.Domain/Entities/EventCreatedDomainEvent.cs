using Eventoria.Domain.Common;

namespace Eventoria.Domain.Entities;

public record EventCreatedDomainEvent(Guid EventId, Guid CreatorUserId)
    : DomainEvent(DateTime.UtcNow);
