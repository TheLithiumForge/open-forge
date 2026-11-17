using System.Globalization;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

// Adapted from the registration editor. System.Text.Json supplies syntax and token spans.
internal sealed class ToolJsonEntries : IDisposable
{
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

    internal byte[] Set(string pointer, JsonElement value, bool requireAbsent)
    {
        var parts = Parts(pointer);
        if (parts.Length == 0) throw new InvalidDataException("A JSON addition must select a member.");
        if (Value(pointer) is not null)
        {
            if (requireAbsent) throw new InvalidDataException("The JSON member is already present.");
            return ToolConfigBytes.Replace(_bytes, [ReplaceValue(pointer, Encode(value))]);
        }
        var parent = Parent(pointer);
        if (Value(parent) is null)
        {
            using var nested = JsonDocument.Parse(ObjectMember(parts[^1], Encode(value)));
            return Set(parent, nested.RootElement, requireAbsent: true);
        }
        if (Value(parent) is not { ValueKind: JsonValueKind.Object })
            throw new InvalidDataException("A new JSON member requires an object parent.");
        var children = Children(parent);
        var insertion = ContainerEnd(parent);
        var prefix = children.Length == 0 ? [] : new byte[] { (byte)',' };
        var member = ObjectMember(parts[^1], Encode(value));
        return ToolConfigBytes.Replace(_bytes, [new(insertion, insertion, [.. prefix, .. member.AsSpan(1, member.Length - 2)])]);
    }

    internal byte[] Union(string pointer, IReadOnlyList<JsonElement> items)
    {
        if (Value(pointer) is null)
        {
            using var array = JsonDocument.Parse(EncodeArray(items));
            return Set(pointer, array.RootElement, requireAbsent: true);
        }
        if (Value(pointer) is not { ValueKind: JsonValueKind.Array } current)
            throw new InvalidDataException("A JSON union requires an array or a missing member.");
        var missing = new List<JsonElement>();
        foreach (var item in items)
            if (!current.EnumerateArray().Concat(missing).Any(existing => JsonElement.DeepEquals(existing, item))) missing.Add(item);
        if (missing.Count == 0) return _bytes;
        var addition = EncodeArray(missing);
        var prefix = current.GetArrayLength() == 0 ? [] : new byte[] { (byte)',' };
        var insertion = ContainerEnd(pointer);
        return ToolConfigBytes.Replace(_bytes, [new(insertion, insertion, [.. prefix, .. addition.AsSpan(1, addition.Length - 2)])]);
    }

    internal byte[] Remove(string pointer)
        => RemoveChildren(Parent(pointer), new HashSet<string>(StringComparer.Ordinal) { pointer });

    internal byte[] RemoveItems(string pointer, IReadOnlyList<JsonElement> items)
    {
        if (Value(pointer) is null) return _bytes;
        if (Value(pointer) is not { ValueKind: JsonValueKind.Array } array)
            throw new InvalidDataException("A recorded JSON union is no longer an array.");
        var remaining = items.ToList();
        var pointers = new HashSet<string>(StringComparer.Ordinal);
        var index = 0;
        foreach (var item in array.EnumerateArray())
        {
            var match = remaining.FindIndex(recorded => JsonElement.DeepEquals(recorded, item));
            if (match >= 0)
            {
                pointers.Add(pointer + "/" + index.ToString(CultureInfo.InvariantCulture));
                remaining.RemoveAt(match);
            }
            index++;
        }
        return RemoveChildren(pointer, pointers);
    }

    private byte[] RemoveChildren(string parent, HashSet<string> pointers)
    {
        var children = Children(parent);
        var changes = new List<ToolEntrySpan>();
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
        return ToolConfigBytes.Replace(_bytes, changes);
    }

    private int ContainerEnd(string pointer)
    {
        var end = _bytes.Length;
        if (pointer.Length > 0)
            end = Children(Parent(pointer)).Single(child => child.Pointer == pointer).End;
        while (end > 0 && _bytes[end - 1] is (byte)' ' or (byte)'\r' or (byte)'\n' or (byte)'\t') end--;
        return end - 1;
    }

    private static byte[] ObjectMember(string name, byte[] value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WritePropertyName(name);
            writer.WriteRawValue(value);
            writer.WriteEndObject();
        }
        return stream.ToArray();
    }

    internal static byte[] EncodeArray(IEnumerable<JsonElement> values)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartArray();
            foreach (var value in values) value.WriteTo(writer);
            writer.WriteEndArray();
        }
        return stream.ToArray();
    }

    internal static byte[] Encode(JsonElement value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) value.WriteTo(writer);
        return stream.ToArray();
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
