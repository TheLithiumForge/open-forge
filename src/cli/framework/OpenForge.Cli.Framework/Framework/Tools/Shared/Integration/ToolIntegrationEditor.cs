using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Integration;

internal enum ToolIntegrationState
{
    Absent,
    Present,
    Conflict,
}

/// <summary>
/// Pure edits for Open Forge's own integration changes. Each change identifies itself, a JSON value or a marked
/// block, so removal needs no record of what installation did. A missing file is null bytes.
/// </summary>
internal static class ToolIntegrationEditor
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private static readonly JsonDocumentOptions ReadOptions = new() { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
    private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

    internal static string Pattern(ToolIntegration integration, string archivePath) => integration switch
    {
        ToolJsonUnion union => archivePath + union.PatternSuffix,
        ToolMarkedBlock block => block.PatternPrefix + archivePath + block.PatternSuffix,
        _ => throw new ArgumentOutOfRangeException(nameof(integration)),
    };

    /// <summary>The change's state, with a plain cause for a conflict.</summary>
    internal static (ToolIntegrationState State, string? Cause) Read(ToolIntegration integration, byte[]? bytes, string archivePath)
    {
        try
        {
            return (integration switch
            {
                ToolJsonUnion union => ReadUnion(union, bytes, Pattern(integration, archivePath)),
                ToolMarkedBlock block => FindBlock(block, Text(bytes)) is null ? ToolIntegrationState.Absent : ToolIntegrationState.Present,
                _ => throw new ArgumentOutOfRangeException(nameof(integration)),
            }, null);
        }
        catch (Exception exception) when (exception is InvalidDataException or JsonException or DecoderFallbackException)
        {
            return (ToolIntegrationState.Conflict, exception.Message);
        }
    }

    /// <summary>The bytes with the change added. Call only for an absent change.</summary>
    internal static byte[] Add(ToolIntegration integration, byte[]? bytes, string archivePath)
    {
        var pattern = Pattern(integration, archivePath);
        switch (integration)
        {
            case ToolJsonUnion union:
                var root = Object(bytes);
                if (root[union.Member] is not JsonArray values)
                {
                    values = [];
                    root[union.Member] = values;
                }
                // The generic JsonArray.Add<T> isn't AOT-safe. The list interface takes a built node.
                ((IList<JsonNode?>)values).Add(JsonValue.Create(pattern));
                return Json(root);
            case ToolMarkedBlock block:
                var text = Text(bytes);
                var newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
                var separator = text.Length == 0 || text.EndsWith('\n') ? string.Empty : newline;
                return StrictUtf8.GetBytes($"{text}{separator}{block.StartMarker}{newline}{pattern}{newline}{block.EndMarker}{newline}");
            default:
                throw new ArgumentOutOfRangeException(nameof(integration));
        }
    }

    /// <summary>The bytes with the change removed, or null when nothing else is left and the file should go.</summary>
    internal static byte[]? Remove(ToolIntegration integration, byte[] bytes, string archivePath)
    {
        var pattern = Pattern(integration, archivePath);
        switch (integration)
        {
            case ToolJsonUnion union:
                var root = Object(bytes);
                var values = (JsonArray)root[union.Member]!;
                for (var index = values.Count - 1; index >= 0; index--)
                {
                    if (values[index] is JsonValue value && value.TryGetValue<string>(out var text) && text == pattern) values.RemoveAt(index);
                }
                return values.Count == 0 && root.Count == 1 ? null : Json(root);
            case ToolMarkedBlock block:
                var source = Text(bytes);
                var (start, end) = FindBlock(block, source) ?? throw new InvalidDataException("The marked block is missing.");
                var remaining = source[..start] + source[end..];
                return remaining.Length == 0 ? null : StrictUtf8.GetBytes(remaining);
            default:
                throw new ArgumentOutOfRangeException(nameof(integration));
        }
    }

    private static ToolIntegrationState ReadUnion(ToolJsonUnion union, byte[]? bytes, string pattern)
    {
        var root = Object(bytes);
        if (!root.ContainsKey(union.Member)) return ToolIntegrationState.Absent;
        if (root[union.Member] is not JsonArray values)
            throw new InvalidDataException($"\"{union.Member}\" in {union.Path} must be an array.");
        return values.Any(value => value is JsonValue item && item.TryGetValue<string>(out var text) && text == pattern)
            ? ToolIntegrationState.Present
            : ToolIntegrationState.Absent;
    }

    private static JsonObject Object(byte[]? bytes)
    {
        var text = Text(bytes);
        if (string.IsNullOrWhiteSpace(text)) return [];
        return JsonNode.Parse(text, documentOptions: ReadOptions) as JsonObject
            ?? throw new InvalidDataException("The file must contain a JSON object.");
    }

    private static byte[] Json(JsonObject root) => StrictUtf8.GetBytes(root.ToJsonString(WriteOptions) + "\n");

    private static string Text(byte[]? bytes) => bytes is null ? string.Empty : StrictUtf8.GetString(bytes);

    /// <summary>The block's character range, including the end marker's line break, or null when absent.</summary>
    private static (int Start, int End)? FindBlock(ToolMarkedBlock block, string source)
    {
        int? start = null;
        int? end = null;
        var offset = 0;
        while (offset < source.Length)
        {
            var lineEnd = source.IndexOf('\n', offset);
            var next = lineEnd < 0 ? source.Length : lineEnd + 1;
            var line = source[offset..next].TrimEnd('\n').TrimEnd('\r');
            if (line == block.StartMarker)
            {
                if (start is not null) throw new InvalidDataException("The Open Forge block appears more than once.");
                start = offset;
            }
            else if (line == block.EndMarker)
            {
                if (end is not null || start is null) throw new InvalidDataException("The Open Forge block markers are out of order.");
                end = next;
            }
            offset = next;
        }
        if (start is null && end is null) return null;
        if (start is null || end is null) throw new InvalidDataException("The Open Forge block is missing a marker.");
        return (start.Value, end.Value);
    }
}
