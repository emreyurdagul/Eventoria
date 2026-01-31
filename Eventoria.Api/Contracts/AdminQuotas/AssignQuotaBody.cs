namespace Eventoria.Api.Contracts.AdminQuotas
{
    public sealed record AssignQuotaBody(
        Guid AdminUserId,
        int MaxEvents,
        int MaxTotalParticipants
    );

}
