using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

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
                    ReadStringArray(root, WorkspaceSettingsDefinitions.RemovedCategoriesProperty)),
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

    private static WorkspaceSettingsDecode Invalid(string cause)
        => new(Document: null, cause);
}
