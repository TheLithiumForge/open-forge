using System.Text.Json;
using OpenForge.Cli.Composition.Shared.Tools.Npm.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Composition.Shared.Tools.Npm;

internal static class ToolNpmMetadataReader
{
    internal const int MaximumVersions = 4096;

    internal static IReadOnlyList<ToolNpmVersionMetadata> Read(byte[] bytes, ToolNpmChannel channel)
    {
        var package = JsonSerializer.Deserialize(bytes, ToolNpmMetadataJsonContext.Default.ToolNpmPackument);
        if (package?.Name != channel.Package || package.Versions is null || package.Versions.Count > MaximumVersions)
            throw new InvalidDataException("The npm metadata package or version count isn't valid.");
        var result = new List<ToolNpmVersionMetadata>();
        foreach (var (version, published) in package.Versions)
        {
            if (published is null) throw new InvalidDataException("An npm metadata version has no package facts.");
            if (published.Version is not null && published.Version != version) throw new InvalidDataException("Npm metadata version bindings disagree.");
            var entry = Entry(published.Bin, channel.EntryRelativePath);
            var node = published.Engines?.GetValueOrDefault("node");
            result.Add(new(version, entry, node));
        }
        return result;
    }

    private static string Entry(JsonElement bin, string declared)
    {
        string? value = null;
        if (bin.ValueKind == JsonValueKind.String) value = bin.GetString();
        if (bin.ValueKind == JsonValueKind.Object)
        {
            var paths = bin.EnumerateObject().Where(property => property.Value.ValueKind == JsonValueKind.String)
                .Select(property => property.Value.GetString()).OfType<string>().ToArray();
            value = paths.FirstOrDefault(path => path == declared || path == $"./{declared}");
            if (value is null && paths.Length == 1) value = paths[0];
        }
        if (value?.StartsWith("./", StringComparison.Ordinal) == true) value = value[2..];
        return PortableWorkspacePath.TryNormalize(value, out var relative) ? relative : "";
    }
}
