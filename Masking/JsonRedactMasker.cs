using System.Text.Json;

namespace PgSafeExport.Masking;

public sealed class JsonRedactMasker : IMasker
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "email", "phone", "name", "firstName", "lastName", "address", "password", "token", "secret"
    };

    public string Mask(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";

        try
        {
            using var doc = JsonDocument.Parse(value);
            var masked = MaskElement(doc.RootElement);
            return JsonSerializer.Serialize(masked);
        }
        catch
        {
            return "[REDACTED_JSON]";
        }
    }

    private static object? MaskElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                p => p.Name,
                p => SensitiveKeys.Contains(p.Name) ? "[REDACTED]" : MaskElement(p.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(MaskElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }
}
