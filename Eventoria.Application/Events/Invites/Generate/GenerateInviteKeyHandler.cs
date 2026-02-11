using Eventoria.Application.Abstractions.Persistence;
using Eventoria.Application.Abstractions.Security;
using MediatR;
using System.Security.Cryptography;

namespace Eventoria.Application.Events.Invites.Generate;

public sealed class GenerateInviteKeyHandler : IRequestHandler<GenerateInviteKeyCommand, GenerateInviteKeyResult>
{
    private readonly IEventRepository _events;
    private readonly IUnitOfWork _uow;
    private readonly IEventTokenService _tokens;

    public GenerateInviteKeyHandler(
        IEventRepository events,
        IUnitOfWork uow,
        IEventTokenService tokens)
    {
        _events = events;
        _uow = uow;
        _tokens = tokens;
    }

    public async Task<GenerateInviteKeyResult> Handle(GenerateInviteKeyCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (cmd.EventId == Guid.Empty)
            throw new InvalidOperationException("EventId is required.");

        var isAdmin = await _events.IsEventAdminAsync(cmd.EventId, cmd.UserId, ct);
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only Event Admin can generate invite keys.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            var ev = await _events.GetByIdAsync(cmd.EventId, innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            var inviteKey = GenerateRandomKey();
            var inviteHash = _tokens.Sha256Hex(inviteKey);

            ev.AddInviteWithoutDeactivation(inviteHash);

            // ✅ SaveChanges burada YOK — transaction wrapper zaten yapacak
            return new GenerateInviteKeyResult(inviteKey, DateTime.UtcNow);
        }, ct);
    }

    private static string GenerateRandomKey()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[32];

        for (int i = 0; i < result.Length; i++)
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        return new string(result);
    }
}
