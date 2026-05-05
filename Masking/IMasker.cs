namespace PgSafeExport.Masking;

public interface IMasker
{
    string Mask(string? value);
}
