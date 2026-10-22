using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

internal static partial class WorkspaceSettingsCodec
{
    private const string RootItemDescription = "canonical workspace-relative file paths outside .agents";

    /// <summary>
    /// Whether a path can be registered in `roots`: canonical, outside `.agents/`, and not Git
    /// metadata or a control file. Whether the file exists is the caller's question.
    /// </summary>
    internal static bool IsValidRootPath(string? path)
        => IsCanonicalRemovalPath(path)
            && !path!.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The bytes of <paramref name="existing"/> with <paramref name="path"/> appended to `roots`,
    /// or null when it is already registered. Other keys keep their values and order.
    /// </summary>
    internal static byte[]? AddRoot(ReadOnlyMemory<byte> existing, string path)
    {
        ValidateRoot(path);
        var settings = ReadRootsObject(existing);
        if (settings[WorkspaceSettingsDefinitions.RootsProperty] is not JsonArray roots)
        {
            roots = [];
            settings[WorkspaceSettingsDefinitions.RootsProperty] = roots;
        }

        if (roots.Any(entry => string.Equals(entry?.GetValue<string>(), path, StringComparison.Ordinal)))
        {
            return null;
        }

        // The generic JsonArray.Add<T> is not AOT-safe because it can box a
        // non-primitive; add an already-built JSON node through the list API.
        ((IList<JsonNode?>)roots).Add(JsonValue.Create(path));
        return WriteRootsObject(settings);
    }

    /// <summary>
    /// The bytes of <paramref name="existing"/> without <paramref name="path"/> in `roots`, or null
    /// when it was not registered. An emptied list stays as an empty array.
    /// </summary>
    internal static byte[]? RemoveRoot(ReadOnlyMemory<byte> existing, string path)
    {
        ValidateRoot(path);
        if (existing.IsEmpty)
        {
            return null;
        }

        var settings = ReadRootsObject(existing);
        if (settings[WorkspaceSettingsDefinitions.RootsProperty] is not JsonArray roots)
        {
            return null;
        }

        var changed = false;
        for (var index = roots.Count - 1; index >= 0; index--)
        {
            if (string.Equals(roots[index]?.GetValue<string>(), path, StringComparison.Ordinal))
            {
                roots.RemoveAt(index);
                changed = true;
            }
        }

        return changed ? WriteRootsObject(settings) : null;
    }

    private static ImmutableArray<string> ReadRoots(JsonElement root)
        => ReadValidatedArray(
            root,
            WorkspaceSettingsDefinitions.RootsProperty,
            IsValidRootPath,
            RootItemDescription,
            nullIsEmpty: true);

    private static void ValidateRoot(string path)
    {
        if (!IsValidRootPath(path))
        {
            throw new ArgumentException(
                $"\"{WorkspaceSettingsDefinitions.RootsProperty}\" must contain only {RootItemDescription}.",
                nameof(path));
        }
    }

    private static JsonObject ReadRootsObject(ReadOnlyMemory<byte> existing)
    {
        // New settings declare their shape. Existing authored files retain the
        // declarations and ordering their author supplied.
        if (existing.IsEmpty)
        {
            return new JsonObject
            {
                [WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl,
                [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion,
            };
        }

        var decoded = Read(existing);
        if (decoded.Document is null)
        {
            throw new ArgumentException(decoded.Cause, nameof(existing));
        }

        return JsonNode.Parse(existing.Span, documentOptions: new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        }) as JsonObject ?? throw new ArgumentException("Authored settings must contain a JSON object.", nameof(existing));
    }

    private static byte[] WriteRootsObject(JsonObject settings)
        => Encoding.UTF8.GetBytes(settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
}
