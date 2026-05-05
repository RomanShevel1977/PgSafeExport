namespace PgSafeExport.Masking;

public sealed class ConstantMasker(string value) : IMasker
{
    public string Mask(string? _) => value;
}
