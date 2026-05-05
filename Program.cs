using PgSafeExport.Config;
using PgSafeExport.Database;
using PgSafeExport.Util;

try
{
    var options = ArgsParser.Parse(args);
    var maskConfig = MaskConfigLoader.Load(options.MaskConfigPath);
    var schemaReader = new PostgresSchemaReader(options.ConnectionString);
    var exporter = new PostgresExporter(options, maskConfig, schemaReader);

    await exporter.ExportAsync();
    return 0;
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 2;
}
catch (Exception ex)
{
    Console.Error.WriteLine("Export failed:");
    Console.Error.WriteLine(ex);
    return 1;
}
