namespace Eventoria.Api.Contracts.AdminQuotas
{
    public sealed record AssignQuotaBody(
        Guid? UserId,
        int MaxEvents,
        int MaxTotalParticipants
    );

}
