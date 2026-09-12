using System.Text.Json;

namespace OpenForge.Cli.TestSupport;

public static class ExtensionCatalogueSource
{
    public static IReadOnlyList<string> PackageIds { get; } = ReadPackageIds();

    private static IReadOnlyList<string> ReadPackageIds()
    {
        const string prefix = "OpenForge.Tests.ExtensionManifests/";
        var assembly = typeof(ExtensionCatalogueSource).Assembly;
        var ids = new List<string>();
        foreach (var name in assembly.GetManifestResourceNames().Where(name =>
                     name.StartsWith(prefix, StringComparison.Ordinal)
                     && name[prefix.Length..].Count(character => character is '/' or '\\') == 1))
        {
            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidDataException($"The source manifest fixture '{name}' is missing.");
            using var document = JsonDocument.Parse(stream);
            ids.Add(document.RootElement.GetProperty("id").GetString()
                ?? throw new InvalidDataException($"The source manifest fixture '{name}' has no ID."));
        }

        if (ids.Count == 0 || ids.Distinct(StringComparer.Ordinal).Count() != ids.Count)
        {
            throw new InvalidDataException("The source catalogue fixture requires distinct package IDs.");
        }

        return Array.AsReadOnly(ids.Order(StringComparer.Ordinal).ToArray());
    }
}
