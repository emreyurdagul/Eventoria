using System.Security.Cryptography;
using System.Text;
using Eventoria.Application.Abstractions.Security;

namespace Eventoria.Infrastructure.Security;

public sealed class EventTokenService : IEventTokenService
{
    public string GenerateEventCode(int length = 8)
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // okunabilir base32 benzeri
        Span<byte> bytes = stackalloc byte[length];
        RandomNumberGenerator.Fill(bytes);

        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];

        return new string(chars);
    }

    public string GenerateInviteKey(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
