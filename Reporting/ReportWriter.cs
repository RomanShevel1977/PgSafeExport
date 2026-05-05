using System.Text.Json;

namespace PgSafeExport.Reporting;

public static class ReportWriter
{
    public static async Task WriteAsync(string outputDirectory, ExportReport report, CancellationToken ct = default)
    {
        report.FinishedAt = DateTimeOffset.UtcNow;
        var path = Path.Combine(outputDirectory, "pgsafe-report.json");
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }
}
