namespace PgSafeExport.Masking;

public sealed class Last4Masker : IMasker
{
    public string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var digits = new string(value.Where(char.IsDigit).ToArray());
        var last4 = digits.Length <= 4 ? digits : digits[^4..];
        return $"***-***-{last4}";
    }
}
