using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Billing;
using MediatR;

namespace Eventoria.Application.Billing.AssignQuota;

public sealed class AssignQuotaHandler
    : IRequestHandler<AssignQuotaCommand, AssignQuotaResult>
{
    private readonly IEventAdminQuotaRepository _quotas;
    private readonly IUnitOfWork _uow;

    public AssignQuotaHandler(
        IEventAdminQuotaRepository quotas,
        IUnitOfWork uow)
    {
        _quotas = quotas;
        _uow = uow;
    }

    public async Task<AssignQuotaResult> Handle(AssignQuotaCommand cmd, CancellationToken ct)
    {
        if (cmd.AdminUserId == Guid.Empty)
            throw new InvalidOperationException("AdminUserId is required.");

        if (cmd.MaxEvents <= 0)
            throw new InvalidOperationException("MaxEvents must be > 0.");

        if (cmd.MaxTotalParticipants <= 0)
            throw new InvalidOperationException("MaxTotalParticipants must be > 0.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            // 1) Eski aktif quota’ları kapat
            await _quotas.DeactivateAllForAdminAsync(cmd.AdminUserId, innerCt);

            // 2) Yeni quota oluştur
            var quota = new EventAdminQuota(
                cmd.AdminUserId,
                cmd.MaxEvents,
                cmd.MaxTotalParticipants);

            await _quotas.AddAsync(quota, innerCt);
            await _uow.SaveChangesAsync(innerCt);

            return new AssignQuotaResult(quota.Id);
        }, ct);
    }
}
