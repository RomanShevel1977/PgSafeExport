namespace PgSafeExport.Core;

public sealed class TableInfo
{
    public required string Schema { get; init; }
    public required string Name { get; init; }
    public required List<ColumnInfo> Columns { get; init; }
    public string FullName => $"{Schema}.{Name}";
}

public sealed class ColumnInfo
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public required string UdtName { get; init; }
    public bool IsJson => UdtName.Equals("json", StringComparison.OrdinalIgnoreCase) || UdtName.Equals("jsonb", StringComparison.OrdinalIgnoreCase);
}
