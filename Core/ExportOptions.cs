namespace PgSafeExport.Core;

public sealed class ExportOptions
{
    public required string ConnectionString { get; init; }
    public required string OutputPath { get; init; }
    public string? MaskConfigPath { get; init; }
    public int MaxParallelism { get; init; } = 4;
    public string Schema { get; init; } = "public";
    public bool DryRun { get; init; }
    public bool Zip { get; init; }
    public HashSet<string> IncludeTables { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludeTables { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
