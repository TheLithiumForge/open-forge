using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

internal static class ToolNpmPackageReader
{
    internal const string ManifestName = "package.json";
    internal const string ModulesDirectory = "node_modules";
    internal const string NodeRuntime = "node";
    internal const int MaximumManifestBytes = 1024 * 1024;

    internal static (string Version, string Entry, string? NodeRange)? Read(string directory, string command)
    {
        var path = System.IO.Path.Combine(directory, ManifestName);
        if (!File.Exists(path) || new FileInfo(path).Length > MaximumManifestBytes) return null;
        using var json = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = json.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("version", out var versionValue) || versionValue.ValueKind != JsonValueKind.String
            || versionValue.GetString() is not { } version || ToolVersionService.ReadStable(version) is null || !root.TryGetProperty("bin", out var bin)) return null;
        string? relative = null;
        if (bin.ValueKind == JsonValueKind.String && root.TryGetProperty("name", out var name) && name.ValueKind == JsonValueKind.String
            && name.GetString()?.Split('/')[^1] == command) relative = bin.GetString();
        else if (bin.ValueKind == JsonValueKind.Object && bin.TryGetProperty(command, out var entry) && entry.ValueKind == JsonValueKind.String) relative = entry.GetString();
        if (relative?.StartsWith("./", StringComparison.Ordinal) == true) relative = relative[2..];
        if (!PortableWorkspacePath.TryNormalize(relative, out var normalized)) return null;
        string? nodeRange = null;
        if (root.TryGetProperty("engines", out var engines) && engines.ValueKind == JsonValueKind.Object
            && engines.TryGetProperty(NodeRuntime, out var node) && node.ValueKind == JsonValueKind.String) nodeRange = node.GetString();
        return (version, System.IO.Path.Combine(directory, normalized.Replace('/', System.IO.Path.DirectorySeparatorChar)), nodeRange);
    }

    internal static IEnumerable<string> PackageDirectories(string modules)
    {
        if (!Directory.Exists(modules)) yield break;
        var count = 0;
        foreach (var folder in Directory.EnumerateDirectories(modules).Order(StringComparer.Ordinal))
        {
            if (++count > 512) yield break;
            if (new DirectoryInfo(folder).LinkTarget is not null) continue;
            if (!System.IO.Path.GetFileName(folder).StartsWith('@')) { yield return folder; continue; }
            foreach (var scoped in Directory.EnumerateDirectories(folder).Order(StringComparer.Ordinal))
            {
                if (++count > 512) yield break;
                if (new DirectoryInfo(scoped).LinkTarget is null) yield return scoped;
            }
        }
    }
}
