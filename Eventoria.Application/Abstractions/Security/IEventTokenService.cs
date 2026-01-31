namespace Eventoria.Application.Abstractions.Security;

public interface IEventTokenService
{
    string GenerateEventCode(int length = 8);
    string GenerateInviteKey(int byteLength = 32); // byte length
    string Sha256Hex(string input);
    bool FixedTimeEquals(string a, string b);
}
