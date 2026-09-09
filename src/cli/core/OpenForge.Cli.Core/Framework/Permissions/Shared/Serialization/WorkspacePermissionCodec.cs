using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;

namespace OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

internal static class WorkspacePermissionCodec
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static WorkspacePermissionDecode Read(ReadOnlyMemory<byte> bytes)
    {
        try
        {
            _ = StrictUtf8.GetCharCount(bytes.Span);
            using var json = JsonDocument.Parse(bytes);
            var root = json.RootElement;
            RequireObject(root, ["schemaVersion", "extensions", "libraries"]);
            var version = root.GetProperty("schemaVersion");
            if (version.ValueKind != JsonValueKind.Number
                || version.GetRawText() != WorkspacePermissionDefinitions.SchemaVersion.ToString(System.Globalization.CultureInfo.InvariantCulture))
            {
                throw new JsonException("The permission schema version is unsupported.");
            }

            var extensions = ImmutableArray.CreateBuilder<ExtensionPermissionGrant>();
            var extensionIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in RequireArray(root.GetProperty("extensions")))
            {
                RequireObject(entry, ["id", "paths"]);
                var id = RequireString(entry.GetProperty("id"));
                if (!ExtensionIdentity.IsValidStableId(id) || !extensionIds.Add(id))
                {
                    throw new JsonException("Extension permission IDs must be valid and unique.");
                }
                extensions.Add(new(id, ReadPaths(entry.GetProperty("paths"))));
            }

            var libraries = ImmutableArray.CreateBuilder<LibraryPermissionGrant>();
            var libraryIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in RequireArray(root.GetProperty("libraries")))
            {
                RequireObject(entry, ["id", "sourceRoot", "paths", "directories"]);
                var id = LibraryId.Create(RequireString(entry.GetProperty("id"))).Value;
                var sourceRoot = WorkspaceRelativeDirectory.Create(RequireString(entry.GetProperty("sourceRoot"))).Value;
                if (!libraryIds.Add(id))
                {
                    throw new JsonException("Library permission IDs must be unique.");
                }
                libraries.Add(new(id, sourceRoot, ReadPaths(entry.GetProperty("paths")), ReadDirectories(entry.GetProperty("directories"))));
            }
            return new(new(extensions.ToImmutable(), libraries.ToImmutable()), Cause: null);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
        {
            return new(Document: null, Cause: "The workspace permission file is not exact schema v1.");
        }
    }

    internal static byte[] Write(WorkspacePermissionDocument document)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            writer.WriteStartObject();
            writer.WriteNumber("schemaVersion", WorkspacePermissionDefinitions.SchemaVersion);
            writer.WriteStartArray("extensions");
            foreach (var grant in document.Extensions.OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("id", grant.Id);
                WritePaths(writer, grant.Paths);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteStartArray("libraries");
            foreach (var grant in document.Libraries.OrderBy(value => value.Id, StringComparer.Ordinal))
            {
                writer.WriteStartObject();
                writer.WriteString("id", grant.Id);
                writer.WriteString("sourceRoot", grant.SourceRoot);
                WritePaths(writer, grant.Paths);
                writer.WriteStartArray("directories");
                foreach (var directory in grant.Directories.Order(StringComparer.Ordinal))
                {
                    writer.WriteStringValue(directory);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        stream.WriteByte((byte)'\n');
        return stream.ToArray();
    }

    private static ImmutableArray<string> ReadDirectories(JsonElement value)
    {
        var directories = ImmutableArray.CreateBuilder<string>();
        var portableDirectories = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in RequireArray(value))
        {
            var directory = RequireString(item);
            if (!PortableWorkspacePath.TryNormalize(directory, out var normalized)
                || normalized != directory
                || normalized.Equals(WorkspacePermissionDefinitions.ImplicitDirectoryPath, StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.OrdinalIgnoreCase)
                || !portableDirectories.Add(PortableWorkspacePath.CreatePortableKey(normalized)))
            {
                throw new JsonException("Library permission directories must be unique canonical portable external subtrees.");
            }
            directories.Add(normalized);
        }
        return directories.ToImmutable();
    }

    private static ImmutableArray<string> ReadPaths(JsonElement value)
    {
        var paths = ImmutableArray.CreateBuilder<string>();
        var portablePaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in RequireArray(value))
        {
            var path = RequireString(item);
            if (!PortableWorkspacePath.TryNormalize(path, out var normalized)
                || normalized.Equals(WorkspacePermissionDefinitions.ImplicitDirectoryPath, StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.OrdinalIgnoreCase)
                || !portablePaths.Add(PortableWorkspacePath.CreatePortableKey(normalized)))
            {
                throw new JsonException("Permission paths must be unique portable external file paths.");
            }
            paths.Add(normalized);
        }
        return paths.ToImmutable();
    }

    private static void WritePaths(Utf8JsonWriter writer, ImmutableArray<string> paths)
    {
        writer.WriteStartArray("paths");
        foreach (var path in paths.Order(StringComparer.Ordinal))
        {
            writer.WriteStringValue(path);
        }
        writer.WriteEndArray();
    }

    private static void RequireObject(JsonElement value, string[] names)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("A permission object is required.");
        }
        var remaining = new HashSet<string>(names, StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!remaining.Remove(property.Name))
            {
                throw new JsonException("A permission property is unknown or duplicated.");
            }
        }
        if (remaining.Count != 0)
        {
            throw new JsonException("A required permission property is absent.");
        }
    }

    private static JsonElement.ArrayEnumerator RequireArray(JsonElement value)
        => value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
            : throw new JsonException("A permission array is required.");

    private static string RequireString(JsonElement value)
        => value.ValueKind == JsonValueKind.String && value.GetString() is { } text
            ? text
            : throw new JsonException("A permission string is required.");
}
