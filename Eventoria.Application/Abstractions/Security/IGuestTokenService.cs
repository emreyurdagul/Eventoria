namespace Eventoria.Application.Abstractions.Security;

public interface IGuestTokenService
{
    string CreateGuestToken(
        Guid guestId,
        Guid eventId,
        string displayName,
        TimeSpan validFor);
}
