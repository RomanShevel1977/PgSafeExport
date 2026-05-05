using PgSafeExport.Core;

namespace PgSafeExport.Util;

public static class ArgsParser
{
    public static ExportOptions Parse(string[] args)
    {
        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
            throw new ArgumentException(HelpText());

        var command = args[0];
        if (!command.Equals("export", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Unknown command: {command}\n\n{HelpText()}");

        var map = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--"))
                throw new ArgumentException($"Unexpected argument: {arg}");

            var key = arg[2..];
            if (key is "dry-run" or "zip")
            {
                map[key] = "true";
                continue;
            }

            if (i + 1 >= args.Length)
                throw new ArgumentException($"Missing value for --{key}");

            map[key] = args[++i];
        }

        var conn = Required(map, "conn");
        var output = Required(map, "out");

        return new ExportOptions
        {
            ConnectionString = conn,
            OutputPath = output,
            MaskConfigPath = Value(map, "mask"),
            MaxParallelism = int.TryParse(Value(map, "parallel"), out var p) ? Math.Max(1, p) : 4,
            Schema = Value(map, "schema") ?? "public",
            DryRun = Bool(map, "dry-run"),
            Zip = Bool(map, "zip"),
            IncludeTables = SplitSet(Value(map, "tables")),
            ExcludeTables = SplitSet(Value(map, "exclude-tables"))
        };
    }

    private static string Required(Dictionary<string, string?> map, string key)
        => Value(map, key) ?? throw new ArgumentException($"Missing required option --{key}\n\n{HelpText()}");

    private static string? Value(Dictionary<string, string?> map, string key)
        => map.TryGetValue(key, out var value) ? value : null;

    private static bool Bool(Dictionary<string, string?> map, string key)
        => bool.TryParse(Value(map, key), out var b) && b;

    private static HashSet<string> SplitSet(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static string HelpText() => """
PgSafeExport

Usage:
  pgsafe export --conn <connection-string> --out <folder-or-zip> [options]

Options:
  --mask <file>              YAML masking config
  --schema <schema>          PostgreSQL schema, default: public
  --parallel <n>             Parallel table workers, default: 4
  --tables <list>            Include only listed tables, e.g. users,orders,public.customers
  --exclude-tables <list>    Exclude tables, e.g. audit_logs,events
  --dry-run                  Show export plan without exporting data
  --zip                      Create zip archive instead of folder output
""";
}
