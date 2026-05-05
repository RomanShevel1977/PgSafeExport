namespace PgSafeExport.Masking;

public sealed class PassthroughMasker : IMasker
{
    public string Mask(string? value) => value ?? "";
}
