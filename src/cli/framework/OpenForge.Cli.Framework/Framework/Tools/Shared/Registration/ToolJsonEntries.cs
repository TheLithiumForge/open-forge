using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

// JSON syntax and values come from System.Text.Json. Token offsets keep unrelated bytes intact.
internal sealed class ToolJsonEntries : IDisposable
{
    internal const string Command = "command";
    internal const string Args = "args";
    private readonly byte[] _bytes;
    private readonly int _offset;
    private readonly JsonDocument _document;

    internal ToolJsonEntries(byte[] bytes)
    {
        _bytes = bytes;
        _offset = bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }) ? 3 : 0;
        JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(bytes.AsSpan(_offset));
        _document = JsonDocument.Parse(bytes.AsMemory(_offset));
    }

    public void Dispose() => _document.Dispose();

    internal JsonElement? Value(string pointer)
    {
        var value = _document.RootElement;
        foreach (var part in Parts(pointer))
        {
            if (value.ValueKind == JsonValueKind.Object && value.TryGetProperty(part, out var member)) value = member;
            else if (value.ValueKind == JsonValueKind.Array && Index(part, value.GetArrayLength()) is { } index) value = value[index];
            else return null;
        }
        return value;
    }

    internal bool Matches(string pointer, ToolCommandValue expected)
    {
        if (Value(pointer) is not { ValueKind: JsonValueKind.Object } entry) return false;
        return entry.TryGetProperty(Command, out var command) && command.ValueKind == JsonValueKind.String && command.GetString() == expected.Command
            && entry.TryGetProperty(Args, out var args) && args.ValueKind == JsonValueKind.Array
            && args.EnumerateArray().All(item => item.ValueKind == JsonValueKind.String)
            && args.EnumerateArray().Select(item => item.GetString()).SequenceEqual(expected.Args, StringComparer.Ordinal);
    }

    internal ToolEntrySpan[] Rewrite(ToolJsonCommandRewrite rewrite)
    {
        if (!Matches(rewrite.Pointer, rewrite.Expected)) throw new InvalidDataException("The JSON command differs from its declared original.");
        return [ReplaceValue(rewrite.Pointer + "/" + Command, JsonSerializer.SerializeToUtf8Bytes(rewrite.Replacement.Command, ToolJsonContext.Default.String)),
            ReplaceValue(rewrite.Pointer + "/" + Args, JsonSerializer.SerializeToUtf8Bytes(rewrite.Replacement.Args, ToolJsonContext.Default.StringArray))];
    }

    internal ToolEntrySpan Rewrite(ToolHookCommandRewrite rewrite)
    {
        if (rewrite.MatchCommand)
        {
            var match = ToolHookEntries.Find(this, rewrite.Pointer, rewrite.Expected)
                ?? throw new InvalidDataException("The declared hook command is missing from its event.");
            return ReplaceValue(match.EntryPointer + "/" + Command, JsonSerializer.SerializeToUtf8Bytes(rewrite.Replacement, ToolJsonContext.Default.String));
        }
        if (Value(rewrite.Pointer) is not { ValueKind: JsonValueKind.String } command || command.GetString() != rewrite.Expected)
            throw new InvalidDataException("The hook command differs from its declared original.");
        return ReplaceValue(rewrite.Pointer, JsonSerializer.SerializeToUtf8Bytes(rewrite.Replacement, ToolJsonContext.Default.String));
    }

    internal void RequireOriginal(ToolJsonEntryRemoval removal)
    {
        if (removal.MatchCommand)
        {
            var match = ToolHookEntries.Find(this, removal.Pointer, ToolHookEntries.ExpectedCommand(removal))
                ?? throw new InvalidDataException("The declined hook is missing from its event.");
            if (!JsonElement.DeepEquals(match.Entry, removal.Expected)) throw new InvalidDataException("The declined hook differs from its complete declared original.");
            return;
        }
        if (removal.Expected.ValueKind == JsonValueKind.Undefined || Value(removal.Pointer) is not { } value || !JsonElement.DeepEquals(value, removal.Expected))
            throw new InvalidDataException("The declined entry differs from its complete declared original.");
        var parent = Parent(removal.Pointer);
        if (Value(parent) is { ValueKind: JsonValueKind.Array } array
            && array.EnumerateArray().Count(item => JsonElement.DeepEquals(item, removal.Expected)) != 1)
            throw new InvalidDataException("The declined array entry is ambiguous.");
    }

    internal bool IsAbsent(ToolJsonEntryRemoval removal)
    {
        if (removal.MatchCommand)
            return new[] { ToolHookEntries.ExpectedCommand(removal) }.Concat(removal.AbsentCommands ?? [])
                .All(command => ToolHookEntries.Find(this, removal.Pointer, command) is null);
        // Removing an array member shifts its siblings. Absence concerns the complete declared entry.
        var parent = Value(Parent(removal.Pointer));
        if (parent is { ValueKind: JsonValueKind.Array } array)
            return !array.EnumerateArray().Any(item => JsonElement.DeepEquals(item, removal.Expected));
        if (parent is { } present && present.ValueKind != JsonValueKind.Object)
            throw new InvalidDataException("The declined entry parent has an unexpected shape.");
        return Value(removal.Pointer) is null;
    }

    internal ToolEntrySpan[] Remove(ToolJsonEntryRemoval[] removals, ToolEntryRewriteRequest? request = null)
    {
        foreach (var removal in removals) RequireOriginal(removal);
        if (removals.Any(removal => removal.MatchCommand)) removals = ToolHookEntries.ResolveRemovals(this, removals, request);
        var pointers = removals.Select(removal => removal.Pointer).ToHashSet(StringComparer.Ordinal);
        if (pointers.Count != removals.Length) throw new InvalidDataException("The declined entry selector is duplicated.");
        var changes = new List<ToolEntrySpan>();
        foreach (var group in removals.GroupBy(removal => Parent(removal.Pointer), StringComparer.Ordinal))
        {
            var children = Children(group.Key);
            for (var index = 0; index < children.Length; index++)
            {
                if (!pointers.Contains(children[index].Pointer)) continue;
                var first = index;
                while (index + 1 < children.Length && pointers.Contains(children[index + 1].Pointer)) index++;
                var start = first > 0 ? children[first - 1].End : children[first].Start;
                var end = children[index].End;
                if (first == 0 && index + 1 < children.Length) end = children[index + 1].Start;
                changes.Add(new(start, end, []));
            }
        }
        return [.. changes];
    }

    internal bool Matches(ToolHookCommandRewrite rewrite)
    {
        if (!rewrite.MatchCommand) return Value(rewrite.Pointer) is { ValueKind: JsonValueKind.String } value && value.GetString() == rewrite.Replacement;
        return ToolHookEntries.Find(this, rewrite.Pointer, rewrite.Replacement) is not null
            && (rewrite.Expected == rewrite.Replacement || ToolHookEntries.Find(this, rewrite.Pointer, rewrite.Expected) is null);
    }

    private ToolEntrySpan ReplaceValue(string pointer, byte[] value)
    {
        var selection = Children(Parent(pointer)).SingleOrDefault(child => child.Pointer == pointer)
            ?? throw new InvalidDataException("The JSON selector is missing.");
        return new(selection.ValueStart, selection.End, value);
    }

    private ToolJsonSelection[] Children(string pointer)
    {
        var reader = new Utf8JsonReader(_bytes.AsSpan(_offset));
        reader.Read();
        foreach (var part in Parts(pointer))
        {
            var found = false;
            var arrayIndex = 0;
            var container = reader.TokenType;
            while (reader.Read() && reader.TokenType is not (JsonTokenType.EndObject or JsonTokenType.EndArray))
            {
                var key = container == JsonTokenType.StartObject ? reader.GetString() : arrayIndex++.ToString(CultureInfo.InvariantCulture);
                if (container == JsonTokenType.StartObject) reader.Read();
                if (key == part) { found = true; break; }
                reader.Skip();
            }
            if (!found) throw new InvalidDataException("The JSON parent selector is missing.");
        }
        if (reader.TokenType is not (JsonTokenType.StartObject or JsonTokenType.StartArray))
            throw new InvalidDataException("The JSON parent is not an object or array.");
        var isObject = reader.TokenType == JsonTokenType.StartObject;
        var selections = new List<ToolJsonSelection>();
        var ordinal = 0;
        while (reader.Read() && reader.TokenType is not (JsonTokenType.EndObject or JsonTokenType.EndArray))
        {
            var start = checked((int)reader.TokenStartIndex) + _offset;
            var key = isObject ? reader.GetString() ?? throw new JsonException() : ordinal++.ToString(CultureInfo.InvariantCulture);
            if (isObject) reader.Read();
            var valueStart = checked((int)reader.TokenStartIndex) + _offset;
            reader.Skip();
            selections.Add(new(pointer + "/" + Escape(key), start, valueStart, checked((int)reader.BytesConsumed) + _offset));
        }
        return [.. selections];
    }

    internal static string[] Parts(string pointer)
    {
        if (pointer.Length == 0) return [];
        if (!pointer.StartsWith('/')) throw new InvalidDataException("A JSON Pointer must start with a slash.");
        var parts = pointer[1..].Split('/');
        for (var index = 0; index < parts.Length; index++)
        {
            var part = parts[index];
            for (var offset = 0; offset < part.Length; offset++)
            {
                if (part[offset] != '~') continue;
                if (++offset == part.Length || part[offset] is not ('0' or '1')) throw new InvalidDataException("A JSON Pointer escape is invalid.");
            }
            parts[index] = part.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);
        }
        return parts;
    }

    internal static string Parent(string pointer)
    {
        _ = Parts(pointer);
        var slash = pointer.LastIndexOf('/');
        if (slash < 0) throw new InvalidDataException("A JSON entry selector must have a parent.");
        return pointer[..slash];
    }

    private static string Escape(string value) => value.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
    private static int? Index(string value, int length)
        => int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index >= 0 && index < length
            && index.ToString(CultureInfo.InvariantCulture) == value ? index : null;
}
