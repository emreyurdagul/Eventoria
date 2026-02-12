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
    private readonly IEncryptionService _encryption;

    public GenerateInviteKeyHandler(
        IEventRepository events,
        IUnitOfWork uow,
        IEventTokenService tokens,
        IEncryptionService encryption)
    {
        _events = events;
        _uow = uow;
        _tokens = tokens;
        _encryption = encryption;
    }

    public async Task<GenerateInviteKeyResult> Handle(GenerateInviteKeyCommand cmd, CancellationToken ct)
    {
        if (cmd.UserId == Guid.Empty)
            throw new InvalidOperationException("UserId is required.");

        if (cmd.EventId == Guid.Empty)
            throw new InvalidOperationException("EventId is required.");

        // Check admin permission before starting transaction
        var isAdmin = await _events.IsEventAdminAsync(cmd.EventId, cmd.UserId, ct);
        if (!isAdmin)
            throw new UnauthorizedAccessException("Only Event Admin can generate invite keys.");

        return await _uow.ExecuteInTransactionAsync(async innerCt =>
        {
            // Load the event
            var ev = await _events.GetByIdWithIncludesAsync(cmd.EventId, innerCt)
                ?? throw new InvalidOperationException("Event not found.");

            // Generate new invite key (plain text - will be shown to user only once)
            var inviteKey = GenerateRandomKey();
            var inviteHash = _tokens.Sha256Hex(inviteKey);
            var encryptedKey = _encryption.Encrypt(inviteKey);  // Encrypt plaintext key

            // Add new invite (old ones remain active, can be deactivated manually via DeactivateInviteKey)
            ev.AddInviteWithoutDeactivation(inviteHash, encryptedKey);

            await _uow.SaveChangesAsync(innerCt);

            return new GenerateInviteKeyResult(inviteKey, DateTime.UtcNow);
        }, ct);
    }

    private static string GenerateRandomKey()
    {
        // Generate cryptographically secure random key (32 characters alphanumeric)
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var result = new char[32];
        
        for (int i = 0; i < result.Length; i++)
        {
            var randomByte = new byte[1];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(randomByte);
            
            result[i] = chars[randomByte[0] % chars.Length];
        }
        
        return new string(result);
    }
}
