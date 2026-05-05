namespace PgSafeExport.Config;

public sealed class MaskConfig
{
    public bool AutoDetect { get; set; } = false;
    public string Salt { get; set; } = "pgsafe-default-salt";
    public Dictionary<string, TableMaskConfig> Tables { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TableMaskConfig
{
    public string? Where { get; set; }
    public Dictionary<string, string> Columns { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
