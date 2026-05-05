namespace PgSafeExport.Masking;

public sealed class NameMasker(string salt) : IMasker
{
    private static readonly string[] Names =
    [
        "John Smith", "Mary Johnson", "Alex Brown", "Robert Miller", "Anna Wilson",
        "David Taylor", "Emma Davis", "Michael Clark", "Olivia White", "Daniel Hall"
    ];

    public string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        var index = StableHash.Int(salt, value) % Names.Length;
        return Names[index];
    }
}
