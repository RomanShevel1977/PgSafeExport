using System.Security.Cryptography;
using System.Text;

namespace PgSafeExport.Masking;

public static class StableHash
{
    public static string Hex(string salt, string? input)
    {
        var normalized = (input ?? string.Empty).Trim().ToLowerInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salt + ":" + normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static int Int(string salt, string? input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salt + ":" + (input ?? string.Empty)));
        return Math.Abs(BitConverter.ToInt32(bytes, 0));
    }
}
