using CsvHelper.Configuration;
using System.Globalization;

namespace PgSafeExport.Database;

public static class CsvHelpers
{
    public static CsvConfiguration Config => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        BadDataFound = null,
        MissingFieldFound = null,
        DetectColumnCountChanges = false
    };
}
