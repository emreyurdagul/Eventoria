namespace Eventoria.Api.Contracts.GuestSession
{
    public sealed record CreateGuestSessionBody(
        string Code,
        string InviteKey,
        string DisplayName
    );

}
