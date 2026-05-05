using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PgSafeExport.Config;

public static class MaskConfigLoader
{
    public static MaskConfig Load(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return new MaskConfig();

        if (!File.Exists(path))
            throw new FileNotFoundException($"Mask config not found: {path}", path);

        var yaml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return deserializer.Deserialize<MaskConfig>(yaml) ?? new MaskConfig();
    }
}
