using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Domain.Entities;
using MediatR;

namespace Eventoria.Application.Events.Update;

public sealed class UpdateEventHandler : IRequestHandler<UpdateEventCommand, UpdateEventResult>
{
    private readonly IEventRepository _events;
    private readonly IEventAdminQuotaRepository _quotas;
    private readonly IUnitOfWork _uow;

    public UpdateEventHandler(IEventRepository events, IEventAdminQuotaRepository quotas, IUnitOfWork uow)
    {
        _events = events;
        _quotas = quotas;
        _uow = uow;
    }

    public async Task<UpdateEventResult> Handle(UpdateEventCommand cmd, CancellationToken ct)
    {
        if (cmd.ActorUserId == Guid.Empty) throw new InvalidOperationException("ActorUserId is required.");
        if (cmd.EventId == Guid.Empty) throw new InvalidOperationException("EventId is required.");

        if (string.IsNullOrWhiteSpace(cmd.Title))
            throw new InvalidOperationException("Title is required.");

        if (cmd.ParticipantLimit <= 0)
            throw new InvalidOperationException("ParticipantLimit must be > 0.");

        if (cmd.PhotosPerUserLimit < 0 || cmd.VideosPerUserLimit < 0)
            throw new InvalidOperationException("Media limits must be >= 0.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            // 1) Yetki
            var isAdmin = await _events.IsEventAdminAsync(cmd.EventId, cmd.ActorUserId, innerCt);
            if (!isAdmin)
                throw new UnauthorizedAccessException("Only event admins can update event.");

            // 2) Event’i çek (memberships lazım)
            var ev = await _events.GetByIdWithIncludesAsync(cmd.EventId, innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            // 3) Event doluluk: limit mevcut üye sayısının altına düşemez
            var currentMemberCount = ev.Memberships.Count;
            if (cmd.ParticipantLimit < currentMemberCount)
                throw new InvalidOperationException("ParticipantLimit cannot be less than current member count.");

            // 4) Quota re-check (sadece participant limit değiştiyse)
            var oldLimit = ev.Specs.ParticipantLimit;
            var newLimit = cmd.ParticipantLimit;

            if (newLimit != oldLimit)
            {
                var quota = await _quotas.GetActiveForAdminAsync(ev.CreatedByUserId, innerCt);
                if (quota == null)
                    throw new UnauthorizedAccessException("No active quota assigned.");

                // Bu event hariç diğer event'lerin toplamı
                var othersSum = await _events.SumParticipantLimitsCreatedByExcludingEventAsync(
                    ev.CreatedByUserId, ev.Id, innerCt);

                if (othersSum + newLimit > quota.MaxTotalParticipants)
                    throw new InvalidOperationException("Participant quota exceeded (MaxTotalParticipants).");
            }

            // 5) Apply changes
            ev.UpdateDetails(cmd.Title, cmd.Description, cmd.Date);
            ev.UpdateSpecs(new EventSpecs(cmd.ParticipantLimit, cmd.PhotosPerUserLimit, cmd.VideosPerUserLimit));

            await _uow.SaveChangesAsync(innerCt);

            return new UpdateEventResult(ev.Id);
        }, ct);
    }
}
