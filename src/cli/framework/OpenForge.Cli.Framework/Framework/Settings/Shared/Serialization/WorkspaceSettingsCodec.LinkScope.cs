using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    internal static byte[]? SeedArchiveLinkScope(ReadOnlyMemory<byte> existing)
    {
        var settings = ReadSettingsObject(existing);
        if (settings.ContainsKey(WorkspaceSettingsDefinitions.IgnoreLinksFromProperty)) return null;
        settings[WorkspaceSettingsDefinitions.IgnoreLinksFromProperty] = new JsonArray(JsonValue.Create(WorkspaceSettingsDefinitions.MemoryArchivePath));
        return WriteSettingsObject(settings);
    }

    internal static byte[]? MoveLinkScope(ReadOnlyMemory<byte> existing, string sourcePath, string destinationPath)
        => MoveLinkScope(existing, [], (sourcePath, destinationPath));

    internal static byte[]? MoveLinkScope(
        ReadOnlyMemory<byte> existing,
        IReadOnlyList<(string Source, string Destination)> layers,
        (string Source, string Destination)? folderMove)
        => ChangeLinkScope(existing, path => Remap(path, layers, folderMove));

    internal static byte[]? RemoveLinkScope(ReadOnlyMemory<byte> existing, IReadOnlyList<string> removedPaths)
        => ChangeLinkScope(existing, path => removedPaths.Any(removed => IsWithin(path, removed)) ? null : path);

    private static byte[]? ChangeLinkScope(ReadOnlyMemory<byte> existing, Func<string, string?> change)
    {
        if (existing.IsEmpty) return null;
        var settings = ReadSettingsObject(existing);
        var changed = false;
        foreach (var key in new[] { WorkspaceSettingsDefinitions.IgnoreLinksFromProperty, WorkspaceSettingsDefinitions.IgnoreLinksToProperty })
        {
            if (settings[key] is not JsonArray values) continue;
            for (var index = values.Count - 1; index >= 0; index--)
            {
                var before = values[index]?.GetValue<string>()
                    ?? throw new InvalidOperationException("Validated link scope entries require strings.");
                var after = change(before);
                if (string.Equals(before, after, StringComparison.Ordinal)) continue;
                if (after is null) values.RemoveAt(index);
                else values[index] = JsonValue.Create(after);
                changed = true;
            }
        }
        return changed ? WriteSettingsObject(settings) : null;
    }

    private static string Remap(string path, string sourcePath, string destinationPath)
        => IsWithin(path, sourcePath) ? destinationPath + path[sourcePath.Length..] : path;

    private static string Remap(
        string path,
        IReadOnlyList<(string Source, string Destination)> layers,
        (string Source, string Destination)? folderMove)
    {
        var key = PortableWorkspacePath.CreatePortableKey(path);
        foreach (var layer in layers)
            if (key == PortableWorkspacePath.CreatePortableKey(layer.Source)) return layer.Destination;
        if (folderMove is { } folder) return Remap(path, folder.Source, folder.Destination);
        return path;
    }

    private static bool IsWithin(string path, string root)
        => WorkspaceAllowList.Admits([root], path);

    private static JsonObject ReadSettingsObject(ReadOnlyMemory<byte> existing)
    {
        if (existing.IsEmpty) return new JsonObject
        {
            [WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl,
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion,
        };
        var decoded = Read(existing);
        if (decoded.Document is null) throw new ArgumentException(decoded.Cause, nameof(existing));
        return JsonNode.Parse(existing.Span, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        }) as JsonObject ?? throw new ArgumentException("Authored settings must contain a JSON object.", nameof(existing));
    }

    private static byte[] WriteSettingsObject(JsonObject settings)
        => Encoding.UTF8.GetBytes(settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
}
