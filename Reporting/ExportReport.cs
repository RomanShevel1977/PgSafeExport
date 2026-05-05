namespace PgSafeExport.Reporting;

public sealed class ExportReport
{
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset FinishedAt { get; set; }
    public List<TableExportReport> Tables { get; set; } = new();
    public int TotalTables => Tables.Count;
    public long TotalRows => Tables.Sum(x => x.Rows);
}

public sealed class TableExportReport
{
    public required string Table { get; init; }
    public required string File { get; init; }
    public long Rows { get; init; }
    public long FileSizeBytes { get; init; }
    public double DurationSeconds { get; init; }
    public List<string> MaskedColumns { get; init; } = new();
    public string? Where { get; init; }
    public string Status { get; init; } = "ok";
    public string? Error { get; init; }
}
