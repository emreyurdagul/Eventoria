namespace Eventoria.Application.Billing.AssignQuota;

public sealed record AssignQuotaCommand(Guid AdminUserId, int MaxEvents, int MaxTotalParticipants);
