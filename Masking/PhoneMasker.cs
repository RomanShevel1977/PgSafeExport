namespace PgSafeExport.Masking;

public sealed class PhoneMasker(string salt) : IMasker
{
    public string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var hash = StableHash.Hex(salt, value);
        var digits = new string(hash.Where(char.IsDigit).Take(9).ToArray()).PadRight(9, '0');
        return $"+100{digits}";
    }
}
