using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using MediatR;

namespace Eventoria.Application.Events.Invites.GetActive;

public sealed class GetActiveInviteHandler : IRequestHandler<GetActiveInviteQuery, GetActiveInvitesListResult>
{
    private readonly IEventRepository _events;
    private readonly IEncryptionService _encryption;

    public GetActiveInviteHandler(IEventRepository events, IEncryptionService encryption)
    {
        _events = events;
        _encryption = encryption;
    }

    public async Task<GetActiveInvitesListResult> Handle(GetActiveInviteQuery query, CancellationToken ct)
    {
        if (query.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (query.EventId == Guid.Empty)
            throw new InvalidOperationException("EventId is required.");

        // Only Event Admin can view invite status
        var isAdmin = await _events.IsEventAdminAsync(query.EventId, query.UserId, ct);
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only Event Admin can view invite keys.");

        var ev = await _events.GetByIdWithIncludesAsync(query.EventId, ct)
            ?? throw new InvalidOperationException("Event not found.");

        var activeInvites = ev.Invites.Where(i => i.IsActive).ToList();

        var results = new List<GetActiveInviteResult>();

        foreach (var invite in activeInvites)
        {
            // Decrypt invite key
            string? plainTextKey = null;
            if (!string.IsNullOrEmpty(invite.EncryptedInviteKey))
            {
                try
                {
                    plainTextKey = _encryption.Decrypt(invite.EncryptedInviteKey);
                }
                catch
                {
                    // Decryption failed - old data or corrupted
                    plainTextKey = null;
                }
            }

            results.Add(new GetActiveInviteResult(
                InviteId: invite.Id,
                IsActive: invite.IsActive,
                EventCode: ev.Code,
                InviteKey: plainTextKey,
                CreatedAtUtc: invite.CreatedAtUtc,
                Message: plainTextKey != null 
                    ? "Active invite key available" 
                    : "Active invite key (decryption failed)"
            ));
        }

        return new GetActiveInvitesListResult(results.AsReadOnly());
    }
}
