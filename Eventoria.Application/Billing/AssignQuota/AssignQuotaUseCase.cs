using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Billing;

namespace Eventoria.Application.Billing.AssignQuota;

public sealed class AssignQuotaUseCase
{
    private readonly IEventAdminQuotaRepository _quotas;

    public AssignQuotaUseCase(IEventAdminQuotaRepository quotas)
        => _quotas = quotas;

    public async Task<AssignQuotaResult> HandleAsync(AssignQuotaCommand cmd, CancellationToken ct)
    {
        if (cmd.AdminUserId == Guid.Empty) throw new ArgumentException("AdminUserId required");
        if (cmd.MaxEvents <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.MaxEvents));
        if (cmd.MaxTotalParticipants <= 0) throw new ArgumentOutOfRangeException(nameof(cmd.MaxTotalParticipants));

        await _quotas.DeactivateAllForAdminAsync(cmd.AdminUserId, ct);

        var quota = new EventAdminQuota(cmd.AdminUserId, cmd.MaxEvents, cmd.MaxTotalParticipants);
        await _quotas.AddAsync(quota, ct);

        await _quotas.SaveChangesAsync(ct);

        return new AssignQuotaResult(quota.Id);
    }
}
