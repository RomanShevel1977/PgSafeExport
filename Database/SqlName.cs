namespace PgSafeExport.Database;

public static class SqlName
{
    public static string Quote(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";
    public static string Qualified(string schema, string table) => $"{Quote(schema)}.{Quote(table)}";
}
