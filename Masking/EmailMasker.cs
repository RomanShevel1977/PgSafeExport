namespace PgSafeExport.Masking;

public sealed class EmailMasker(string salt) : IMasker
{
    public string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var hash = StableHash.Hex(salt, value);
        return $"user_{hash[..12]}@example.test";
    }
}
