using Npgsql;
using PgSafeExport.Core;

namespace PgSafeExport.Database;

public sealed class PostgresSchemaReader
{
    private readonly string _connectionString;

    public PostgresSchemaReader(string connectionString) => _connectionString = connectionString;

    public async Task<List<TableInfo>> GetTablesAsync(string schema, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        const string sql = """
        SELECT
            t.table_schema,
            t.table_name,
            c.column_name,
            c.data_type,
            c.udt_name
        FROM information_schema.tables t
        JOIN information_schema.columns c
          ON c.table_schema = t.table_schema
         AND c.table_name = t.table_name
        WHERE t.table_schema = @schema
          AND t.table_type = 'BASE TABLE'
        ORDER BY t.table_name, c.ordinal_position;
        """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("schema", schema);

        var map = new Dictionary<string, TableBuilder>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var tableSchema = reader.GetString(0);
            var tableName = reader.GetString(1);
            var column = new ColumnInfo
            {
                Name = reader.GetString(2),
                DataType = reader.GetString(3),
                UdtName = reader.GetString(4)
            };

            var key = $"{tableSchema}.{tableName}";
            if (!map.TryGetValue(key, out var builder))
            {
                builder = new TableBuilder(tableSchema, tableName);
                map[key] = builder;
            }
            builder.Columns.Add(column);
        }

        return map.Values.Select(x => new TableInfo
        {
            Schema = x.Schema,
            Name = x.Name,
            Columns = x.Columns
        }).ToList();
    }

    private sealed class TableBuilder(string schema, string name)
    {
        public string Schema { get; } = schema;
        public string Name { get; } = name;
        public List<ColumnInfo> Columns { get; } = new();
    }
}
