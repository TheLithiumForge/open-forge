using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

/// <summary>
/// Reads the authored settings file.
///
/// Unknown keys are accepted and ignored. This file is written by a person, it
/// gains keys over releases, and refusing one the reader has not heard of would
/// make a newer file unusable by an older CLI for no safety gained. Only the
/// keys Open Forge acts on are validated, and a key whose value has the wrong
/// shape is reported rather than skipped, because that is a mistake a person
/// wants told.
/// </summary>
internal static class WorkspaceSettingsCodec
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static WorkspaceSettingsDecode Read(ReadOnlyMemory<byte> bytes)
    {
        try
        {
            _ = StrictUtf8.GetCharCount(bytes.Span);
            using var json = JsonDocument.Parse(
                bytes,
                new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });

            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Invalid($"{WorkspaceSettingsDefinitions.FileName} must contain a JSON object.");
            }

            return new WorkspaceSettingsDecode(
                new WorkspaceSettingsDocument(
                    ReadSchemaVersion(root),
                    ReadStringArray(root, WorkspaceSettingsDefinitions.AllowInstallPathsProperty),
                    ReadRemovedCategories(root),
                    ReadRemovedFiles(root))
                {
                    RemovedDirectories = ReadRemovedDirectories(root),
                    RemovedExtensions = ReadRemovedExtensions(root),
                    RemovedLibraries = ReadRemovedLibraries(root),
                },
                Cause: null);
        }
        catch (JsonException exception)
        {
            return Invalid(exception.Message);
        }
        catch (DecoderFallbackException)
        {
            return Invalid($"{WorkspaceSettingsDefinitions.FileName} must be valid UTF-8.");
        }
        catch (ArgumentException exception)
        {
            return Invalid(exception.Message);
        }
    }

    /// <summary>
    /// The bytes of <paramref name="existing"/> with <paramref name="path"/> added
    /// to <c>allowInstallPaths</c>, or null when it is already admitted.
    ///
    /// The document is round-tripped through the JSON object model rather than
    /// rewritten from the parsed settings, so every other key survives — including
    /// ones this CLI does not recognise — in the order the author wrote them.
    /// Comments do not survive: the serializer cannot carry them, and hand-rolling
    /// a text splice to keep them would be a second parser.
    /// </summary>
    internal static byte[]? AddAllowInstallPath(ReadOnlyMemory<byte> existing, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        JsonNode? root = existing.IsEmpty
            ? null
            : JsonNode.Parse(
                existing.Span,
                nodeOptions: null,
                documentOptions: new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });

        // A file the CLI creates declares what it is: the schema an editor can
        // validate against, and the shape this release wrote. A file the author
        // already has is left as they wrote it, because adding keys they did not
        // ask for to a document they own is not this command's business.
        root ??= new JsonObject
        {
            [WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl,
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion,
        };

        if (root is not JsonObject settings)
        {
            throw new ArgumentException(
                $"{WorkspaceSettingsDefinitions.FileName} must contain a JSON object.",
                nameof(existing));
        }

        if (settings[WorkspaceSettingsDefinitions.AllowInstallPathsProperty] is not JsonArray allowed)
        {
            if (settings.ContainsKey(WorkspaceSettingsDefinitions.AllowInstallPathsProperty)
                && settings[WorkspaceSettingsDefinitions.AllowInstallPathsProperty] is not null)
            {
                throw new ArgumentException(
                    $"\"{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}\" must be an array of strings.",
                    nameof(existing));
            }

            allowed = [];
            settings[WorkspaceSettingsDefinitions.AllowInstallPathsProperty] = allowed;
        }

        foreach (var entry in allowed)
        {
            if (entry is JsonValue value
                && value.TryGetValue<string>(out var text)
                && WorkspaceAllowList.Admits([text], path))
            {
                return null;
            }
        }

        // The generic JsonArray.Add<T> is not AOT-safe because it can box a
        // non-primitive; the list interface takes an already-built node.
        ((IList<JsonNode?>)allowed).Add(JsonValue.Create(path));
        return Encoding.UTF8.GetBytes(
            settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// Adds the persistent removal selection defined by
    /// <c>.agents/memory/working/cli-development/tasks/task50-unified-remove.md</c>.
    /// The JSON object model preserves unknown authored keys and their order.
    /// </summary>
    internal static byte[]? AddRemovals(
        ReadOnlyMemory<byte> existing,
        WorkspaceRemovalSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (IsEmpty(selection))
        {
            return null;
        }

        ValidateSelection(selection);
        JsonNode? root = null;
        if (!existing.IsEmpty)
        {
            var decoded = Read(existing);
            if (decoded.Document is null)
            {
                throw new ArgumentException(
                    decoded.Cause ?? $"{WorkspaceSettingsDefinitions.FileName} is invalid.",
                    nameof(existing));
            }

            root = JsonNode.Parse(
                existing.Span,
                nodeOptions: null,
                documentOptions: new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });
        }

        // New settings declare their shape. Existing authored files retain the
        // declarations and ordering their author supplied.
        root ??= new JsonObject
        {
            [WorkspaceSettingsDefinitions.SchemaProperty] = WorkspaceSettingsDefinitions.SchemaUrl,
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = WorkspaceSettingsDefinitions.SchemaVersion,
        };

        if (root is not JsonObject settings)
        {
            throw new ArgumentException(
                $"{WorkspaceSettingsDefinitions.FileName} must contain a JSON object.",
                nameof(existing));
        }

        var changed = AppendUnique(settings, WorkspaceSettingsDefinitions.RemovedCategoriesProperty, selection.Categories);
        changed |= AppendUnique(settings, WorkspaceSettingsDefinitions.RemovedFilesProperty, selection.Files);
        changed |= AppendUnique(settings, WorkspaceSettingsDefinitions.RemovedDirectoriesProperty, selection.Directories);
        changed |= AppendUnique(settings, WorkspaceSettingsDefinitions.RemovedExtensionsProperty, selection.Extensions);
        changed |= AppendUnique(settings, WorkspaceSettingsDefinitions.RemovedLibrariesProperty, selection.Libraries);
        if (!changed)
        {
            return null;
        }

        return Encoding.UTF8.GetBytes(
            settings.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    /// <summary>
    /// The declared shape, or this release's own when the file does not say. An
    /// absent version means a file written before the key existed, or by hand,
    /// and neither is a mistake worth reporting. A version this release does not
    /// recognise is carried through so a caller can report it; it never stops the
    /// read.
    /// </summary>
    private static int ReadSchemaVersion(JsonElement root)
    {
        if (!root.TryGetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty, out var value)
            || value.ValueKind == JsonValueKind.Null)
        {
            return WorkspaceSettingsDefinitions.SchemaVersion;
        }

        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var version))
        {
            throw new ArgumentException(
                $"\"{WorkspaceSettingsDefinitions.SchemaVersionProperty}\" must be a whole number.");
        }

        return version;
    }

    private static ImmutableArray<string> ReadStringArray(JsonElement root, string property)
    {
        if (!root.TryGetProperty(property, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return [];
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException($"\"{property}\" must be an array of strings.");
        }

        var values = ImmutableArray.CreateBuilder<string>();
        foreach (var entry in value.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException($"\"{property}\" must contain only strings.");
            }

            var text = entry.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException($"\"{property}\" must not contain an empty entry.");
            }

            values.Add(text);
        }

        return values.ToImmutable();
    }

    private static ImmutableArray<string> ReadRemovedCategories(JsonElement root)
        => ReadValidatedArray(
            root,
            WorkspaceSettingsDefinitions.RemovedCategoriesProperty,
            IsValidRemovalCategory,
            "single canonical workspace category names",
            nullIsEmpty: true);

    private static ImmutableArray<string> ReadRemovedFiles(JsonElement root)
        => ReadValidatedArray(
            root,
            WorkspaceSettingsDefinitions.RemovedFilesProperty,
            IsCanonicalRemovalPath,
            "canonical workspace-relative file paths");

    private static ImmutableArray<string> ReadRemovedDirectories(JsonElement root)
        => ReadValidatedArray(
            root,
            WorkspaceSettingsDefinitions.RemovedDirectoriesProperty,
            IsCanonicalRemovalPath,
            "canonical workspace-relative directory paths");

    private static ImmutableArray<string> ReadRemovedExtensions(JsonElement root)
        => ReadValidatedArray(
            root,
            WorkspaceSettingsDefinitions.RemovedExtensionsProperty,
            ExtensionIdentity.IsValidStableId,
            "valid stable Extension IDs");

    private static ImmutableArray<string> ReadRemovedLibraries(JsonElement root)
        => ReadValidatedArray(
            root,
            WorkspaceSettingsDefinitions.RemovedLibrariesProperty,
            static value => LibraryId.TryCreate(value) is not null,
            "valid Library IDs");

    private static ImmutableArray<string> ReadValidatedArray(
        JsonElement root,
        string property,
        Func<string, bool> isValid,
        string itemDescription,
        bool nullIsEmpty = false)
    {
        if (!root.TryGetProperty(property, out var value))
        {
            return [];
        }

        if (nullIsEmpty && value.ValueKind == JsonValueKind.Null)
        {
            return [];
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException($"\"{property}\" must be an array of {itemDescription}.");
        }

        var values = ImmutableArray.CreateBuilder<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in value.EnumerateArray())
        {
            var item = entry.ValueKind == JsonValueKind.String ? entry.GetString() : null;
            if (string.IsNullOrWhiteSpace(item) || !isValid(item))
            {
                throw new ArgumentException($"\"{property}\" must contain only {itemDescription}.");
            }

            if (!seen.Add(item))
            {
                throw new ArgumentException($"\"{property}\" must not contain duplicate entries.");
            }

            values.Add(item);
        }

        return values.ToImmutable();
    }

    private static bool IsEmpty(WorkspaceRemovalSelection selection)
        => selection.Categories.IsDefaultOrEmpty
            && selection.Files.IsDefaultOrEmpty
            && selection.Directories.IsDefaultOrEmpty
            && selection.Extensions.IsDefaultOrEmpty
            && selection.Libraries.IsDefaultOrEmpty;

    private static void ValidateSelection(WorkspaceRemovalSelection selection)
    {
        ValidateSelectionItems(selection.Categories, IsValidRemovalCategory, WorkspaceSettingsDefinitions.RemovedCategoriesProperty, "single canonical workspace category names");
        ValidateSelectionItems(selection.Files, IsCanonicalRemovalPath, WorkspaceSettingsDefinitions.RemovedFilesProperty, "canonical workspace-relative file paths");
        ValidateSelectionItems(selection.Directories, IsCanonicalRemovalPath, WorkspaceSettingsDefinitions.RemovedDirectoriesProperty, "canonical workspace-relative directory paths");
        ValidateSelectionItems(selection.Extensions, ExtensionIdentity.IsValidStableId, WorkspaceSettingsDefinitions.RemovedExtensionsProperty, "valid stable Extension IDs");
        ValidateSelectionItems(selection.Libraries, static value => LibraryId.TryCreate(value) is not null, WorkspaceSettingsDefinitions.RemovedLibrariesProperty, "valid Library IDs");
    }

    private static void ValidateSelectionItems(
        ImmutableArray<string> values,
        Func<string, bool> isValid,
        string property,
        string itemDescription)
    {
        if (values.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value) || !isValid(value))
            {
                throw new ArgumentException(
                    $"\"{property}\" must contain only {itemDescription}.",
                    "selection");
            }
        }
    }

    private static bool IsValidRemovalCategory(string category)
        => !category.Contains('/', StringComparison.Ordinal)
            && IsCanonicalRemovalPath(WorkspaceSettingsDefinitions.ImplicitPathPrefix + category);

    private static bool IsCanonicalRemovalPath(string? path)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized)
            || !string.Equals(path, normalized, StringComparison.Ordinal))
        {
            return false;
        }

        return !IsReservedControlStatePath(normalized);
    }

    private static bool IsReservedControlStatePath(string path)
    {
        if (string.Equals(path, WorkspaceSettingsDefinitions.DirectoryName, StringComparison.OrdinalIgnoreCase)
            || path.Split('/').Any(segment =>
                string.Equals(segment, WorkspaceSettingsDefinitions.GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return IsAtOrBelow(path, WorkspaceSettingsDefinitions.RelativePath)
            || IsAtOrBelow(path, WorkspaceOwnershipDefinitions.RelativePath);
    }

    private static bool IsAtOrBelow(string path, string reservedPath)
        => string.Equals(path, reservedPath, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(reservedPath + "/", StringComparison.OrdinalIgnoreCase);

    private static bool AppendUnique(
        JsonObject settings,
        string property,
        ImmutableArray<string> selected)
    {
        if (selected.IsDefaultOrEmpty)
        {
            return false;
        }

        var array = settings[property] as JsonArray;
        var existing = array is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : array.Select(entry => entry?.GetValue<string>()
                ?? throw new InvalidOperationException("Validated removal entries must be non-null strings."))
                .ToHashSet(StringComparer.Ordinal);
        var additions = selected
            .Distinct(StringComparer.Ordinal)
            .Where(value => !existing.Contains(value))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (additions.Length == 0)
        {
            return false;
        }

        array ??= [];
        settings[property] ??= array;
        foreach (var addition in additions)
        {
            // The generic JsonArray.Add<T> is not AOT-safe because it can box a
            // non-primitive; add an already-built JSON node through the list API.
            ((IList<JsonNode?>)array).Add(JsonValue.Create(addition));
        }

        return true;
    }

    private static WorkspaceSettingsDecode Invalid(string cause)
        => new(Document: null, cause);
}
