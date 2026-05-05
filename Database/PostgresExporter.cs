using CsvHelper;
using Npgsql;
using PgSafeExport.Config;
using PgSafeExport.Core;
using PgSafeExport.Masking;
using PgSafeExport.Reporting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace PgSafeExport.Database;

public sealed class PostgresExporter
{
    private readonly ExportOptions _options;
    private readonly MaskConfig _maskConfig;
    private readonly PostgresSchemaReader _schemaReader;

    public PostgresExporter(ExportOptions options, MaskConfig maskConfig, PostgresSchemaReader schemaReader)
    {
        _options = options;
        _maskConfig = maskConfig;
        _schemaReader = schemaReader;
    }

    public async Task ExportAsync(CancellationToken ct = default)
    {
        var outputDirectory = PrepareOutputDirectory();
        var tables = await _schemaReader.GetTablesAsync(_options.Schema, ct);
        tables = ApplyTableFilters(tables);

        if (_options.DryRun)
        {
            PrintDryRun(tables);
            return;
        }

        Console.WriteLine($"Found {tables.Count} table(s). Parallelism: {_options.MaxParallelism}");

        var report = new ExportReport();
        var reports = new ConcurrentBag<TableExportReport>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxParallelism,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(tables, parallelOptions, async (table, token) =>
        {
            var tableReport = await ExportTableSafeAsync(table, outputDirectory, token);
            reports.Add(tableReport);
        });

        report.Tables = reports.OrderBy(x => x.Table).ToList();
        await ReportWriter.WriteAsync(outputDirectory, report, ct);

        if (_options.Zip)
        {
            var zipPath = _options.OutputPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
                ? _options.OutputPath
                : _options.OutputPath + ".zip";

            if (File.Exists(zipPath)) File.Delete(zipPath);
            ZipFile.CreateFromDirectory(outputDirectory, zipPath, CompressionLevel.Fastest, includeBaseDirectory: false);
            Directory.Delete(outputDirectory, recursive: true);
            Console.WriteLine($"ZIP created: {zipPath}");
        }

        Console.WriteLine($"Exported {report.TotalRows} row(s) from {report.TotalTables} table(s).");
    }

    private string PrepareOutputDirectory()
    {
        if (_options.Zip)
        {
            var temp = Path.Combine(Path.GetTempPath(), "pgsafe-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            return temp;
        }

        Directory.CreateDirectory(_options.OutputPath);
        return _options.OutputPath;
    }

    private List<TableInfo> ApplyTableFilters(List<TableInfo> tables)
    {
        bool Matches(HashSet<string> set, TableInfo t) => set.Contains(t.Name) || set.Contains(t.FullName);

        if (_options.IncludeTables.Count > 0)
            tables = tables.Where(t => Matches(_options.IncludeTables, t)).ToList();

        if (_options.ExcludeTables.Count > 0)
            tables = tables.Where(t => !Matches(_options.ExcludeTables, t)).ToList();

        return tables;
    }

    private void PrintDryRun(List<TableInfo> tables)
    {
        Console.WriteLine("Dry-run export plan");
        Console.WriteLine($"Schema: {_options.Schema}");
        Console.WriteLine($"Tables: {tables.Count}");
        Console.WriteLine();

        foreach (var table in tables.OrderBy(t => t.FullName))
        {
            var tableMask = GetTableMask(table);
            var maskedColumns = GetMaskedColumns(table, tableMask).ToList();
            Console.WriteLine($"- {table.FullName}");
            Console.WriteLine($"  columns: {table.Columns.Count}");
            Console.WriteLine($"  where: {(string.IsNullOrWhiteSpace(tableMask?.Where) ? "<none>" : tableMask!.Where)}");
            Console.WriteLine($"  masked: {(maskedColumns.Count == 0 ? "<none>" : string.Join(", ", maskedColumns))}");
        }
    }

    private async Task<TableExportReport> ExportTableSafeAsync(TableInfo table, string outputDirectory, CancellationToken ct)
    {
        try
        {
            return await ExportTableAsync(table, outputDirectory, ct);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{table.FullName}] failed: {ex.Message}");
            return new TableExportReport
            {
                Table = table.FullName,
                File = "",
                Rows = 0,
                FileSizeBytes = 0,
                DurationSeconds = 0,
                Status = "error",
                Error = ex.Message
            };
        }
    }

    private async Task<TableExportReport> ExportTableAsync(TableInfo table, string outputDirectory, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var tableMask = GetTableMask(table);
        var fileName = $"{table.Schema}.{table.Name}.csv";
        var outputPath = Path.Combine(outputDirectory, fileName);

        var columnsSql = string.Join(", ", table.Columns.Select(c => SqlName.Quote(c.Name)));
        var tableSql = SqlName.Qualified(table.Schema, table.Name);
        var whereSql = string.IsNullOrWhiteSpace(tableMask?.Where) ? "" : $" WHERE {tableMask!.Where}";

        var copySql = $"""
        COPY (
            SELECT {columnsSql}
            FROM {tableSql}
            {whereSql}
        ) TO STDOUT WITH CSV HEADER
        """;

        var maskers = BuildMaskers(table, tableMask);
        var maskedColumns = GetMaskedColumns(table, tableMask).ToList();

        await using var conn = new NpgsqlConnection(_options.ConnectionString);
        await conn.OpenAsync(ct);

        using var textReader = await conn.BeginTextExportAsync(copySql, ct);
        await using var stream = File.Create(outputPath);
        await using var streamWriter = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        using var csvReader = new CsvReader(textReader, CsvHelpers.Config);
        using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

        if (!await csvReader.ReadAsync())
            return EmptyReport(table, outputPath, sw, maskedColumns, tableMask?.Where);

        csvReader.ReadHeader();
        foreach (var col in table.Columns)
            csvWriter.WriteField(col.Name);
        await csvWriter.NextRecordAsync();

        long rowCount = 0;
        while (await csvReader.ReadAsync())
        {
            for (var i = 0; i < table.Columns.Count; i++)
            {
                var raw = csvReader.GetField(i);
                var masked = maskers[i].Mask(raw);
                csvWriter.WriteField(masked);
            }

            await csvWriter.NextRecordAsync();
            rowCount++;
        }

        await streamWriter.FlushAsync(ct);
        sw.Stop();

        var info = new FileInfo(outputPath);
        Console.WriteLine($"[{table.FullName}] {rowCount} row(s), {info.Length} bytes, {sw.Elapsed.TotalSeconds:0.00}s");

        return new TableExportReport
        {
            Table = table.FullName,
            File = fileName,
            Rows = rowCount,
            FileSizeBytes = info.Length,
            DurationSeconds = sw.Elapsed.TotalSeconds,
            MaskedColumns = maskedColumns,
            Where = tableMask?.Where
        };
    }

    private static TableExportReport EmptyReport(TableInfo table, string outputPath, Stopwatch sw, List<string> maskedColumns, string? where)
    {
        sw.Stop();
        return new TableExportReport
        {
            Table = table.FullName,
            File = Path.GetFileName(outputPath),
            Rows = 0,
            FileSizeBytes = File.Exists(outputPath) ? new FileInfo(outputPath).Length : 0,
            DurationSeconds = sw.Elapsed.TotalSeconds,
            MaskedColumns = maskedColumns,
            Where = where
        };
    }

    private TableMaskConfig? GetTableMask(TableInfo table)
    {
        if (_maskConfig.Tables.TryGetValue(table.FullName, out var byFullName)) return byFullName;
        if (_maskConfig.Tables.TryGetValue(table.Name, out var byName)) return byName;
        return null;
    }

    private List<IMasker> BuildMaskers(TableInfo table, TableMaskConfig? tableMask)
    {
        return table.Columns.Select(column =>
        {
            var rule = ResolveRule(column, tableMask);
            return MaskerFactory.Create(rule, _maskConfig.Salt);
        }).ToList();
    }

    private IEnumerable<string> GetMaskedColumns(TableInfo table, TableMaskConfig? tableMask)
    {
        foreach (var column in table.Columns)
        {
            var rule = ResolveRule(column, tableMask);
            if (!string.IsNullOrWhiteSpace(rule))
                yield return $"{column.Name}:{rule}";
        }
    }

    private string? ResolveRule(ColumnInfo column, TableMaskConfig? tableMask)
    {
        if (tableMask?.Columns.TryGetValue(column.Name, out var configuredRule) == true)
            return configuredRule;

        if (!_maskConfig.AutoDetect)
            return null;

        var name = column.Name.ToLowerInvariant();
        if (name.Contains("email")) return "fake_email";
        if (name.Contains("phone") || name.Contains("mobile")) return "fake_phone";
        if (name is "name" or "full_name" or "firstname" or "first_name" or "lastname" or "last_name") return "fake_name";
        if (name.Contains("password") || name.Contains("token") || name.Contains("secret")) return "redact";
        if (column.IsJson && (name.Contains("metadata") || name.Contains("profile") || name.Contains("data"))) return "json_redact";

        return null;
    }
}
