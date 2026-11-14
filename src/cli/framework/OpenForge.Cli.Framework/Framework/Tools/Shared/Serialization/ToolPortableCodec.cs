using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

internal sealed partial class ToolPortableCodec(IToolVersionService versions)
{
    internal IReadOnlyDictionary<string, ToolSetting> ReadSettings(JsonObject root)
    {
        var result = new SortedDictionary<string, ToolSetting>(StringComparer.Ordinal);
        foreach (var (id, value) in Tools(root))
        {
            ValidateId(id);
            var entry = Object(value, id);
            var range = versions.ReadRange(String(entry, ToolRecordDefinitions.VersionProperty));
            if (range is null) throw Invalid($"tools.{id}.version", "a supported major.minor.x range");
            result.Add(id, new ToolSetting(range));
        }
        return result;
    }

    internal IReadOnlyDictionary<string, ToolLockRecord> ReadLock(JsonObject root)
    {
        var result = new SortedDictionary<string, ToolLockRecord>(StringComparer.Ordinal);
        foreach (var (id, value) in Tools(root))
        {
            ValidateId(id);
            var entry = Object(value, id);
            var version = String(entry, ToolRecordDefinitions.VersionProperty);
            ValidateVersion(version);
            if (entry[ToolRecordDefinitions.ChangesProperty] is not JsonArray changes)
                throw Invalid($"tools.{id}.changes", "an array");
            var additions = changes.Select(change => ReadChange(Object(change, $"tools.{id}.changes"))).ToArray();
            if (additions.Select(change => change.Id).Distinct(StringComparer.Ordinal).Count() != additions.Length)
                throw Invalid($"tools.{id}.changes", "unique change IDs");
            var digest = OptionalString(entry, ToolRecordDefinitions.ProfileDigestProperty);
            if (digest is not null) ValidateDigest(digest);
            result.Add(id, new ToolLockRecord
            {
                Version = version,
                Channel = String(entry, ToolRecordDefinitions.ChannelProperty),
                ProfileDigest = digest,
                Changes = additions,
            });
        }
        return result;
    }

    internal JsonObject WriteSettings(JsonObject original, IReadOnlyDictionary<string, ToolSetting> settings)
    {
        var entries = new JsonObject();
        foreach (var (id, setting) in settings.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            ValidateId(id);
            var range = setting.Version;
            if (range.Major < 0 || range.Minor < 0) throw Invalid(id, "a non-negative version range");
            entries[id] = MergeEntry(original, id, new JsonObject
            {
                [ToolRecordDefinitions.VersionProperty] = string.Create(CultureInfo.InvariantCulture, $"{range.Major}.{range.Minor}.x"),
            });
        }
        var result = (JsonObject)original.DeepClone();
        result[ToolRecordDefinitions.ToolsProperty] = entries;
        return result;
    }

    internal JsonObject WriteLock(JsonObject original, IReadOnlyDictionary<string, ToolLockRecord> records)
    {
        var entries = new JsonObject();
        foreach (var (id, record) in records.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            ValidateId(id);
            ValidateVersion(record.Version);
            if (record.ProfileDigest is not null) ValidateDigest(record.ProfileDigest);
            var old = original[ToolRecordDefinitions.ToolsProperty]?[id] as JsonObject;
            var changes = new JsonArray();
            foreach (var change in record.Changes)
            {
                var prior = (old?[ToolRecordDefinitions.ChangesProperty] as JsonArray)?
                    .OfType<JsonObject>().FirstOrDefault(item => OptionalString(item, ToolRecordDefinitions.IdProperty) == change.Id);
                var written = WriteChange(change);
                var merged = ToolRecordJson.Merge(prior, written);
                foreach (var payload in new[] { ToolRecordDefinitions.ItemsProperty, ToolRecordDefinitions.ValueProperty })
                    if (written.ContainsKey(payload)) merged[payload] = written[payload]?.DeepClone();
                ((IList<JsonNode?>)changes).Add(merged);
            }
            var entry = MergeEntry(original, id, new JsonObject
            {
                [ToolRecordDefinitions.VersionProperty] = record.Version,
                [ToolRecordDefinitions.ChannelProperty] = record.Channel,
            });
            entry[ToolRecordDefinitions.ChangesProperty] = changes;
            entry.Remove(ToolRecordDefinitions.ProfileDigestProperty);
            if (record.ProfileDigest is not null) entry[ToolRecordDefinitions.ProfileDigestProperty] = record.ProfileDigest;
            entries[id] = entry;
        }
        var result = (JsonObject)original.DeepClone();
        result[ToolRecordDefinitions.ToolsProperty] = entries;
        _ = ReadLock(result);
        return result;
    }

    internal void ValidateVersion(string version)
    {
        var spec = versions.ReadSpec($"record@{version}");
        if (spec?.Version.Kind != ToolVersionSelectionKind.Exact) throw Invalid(ToolRecordDefinitions.VersionProperty, "an exact stable SemVer");
    }

    internal static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id is "." or ".." || id.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
            throw Invalid(ToolRecordDefinitions.IdProperty, "a tool ID without path characters");
    }

    internal static void ValidateDigest(string value)
    {
        if (value.Length != 64 || value.Any(character => !char.IsAsciiHexDigit(character)))
            throw Invalid(ToolRecordDefinitions.Sha256Property, "64 hexadecimal characters");
    }

    private static JsonObject Tools(JsonObject root)
        => !root.ContainsKey(ToolRecordDefinitions.ToolsProperty) ? new JsonObject() : Object(root[ToolRecordDefinitions.ToolsProperty], ToolRecordDefinitions.ToolsProperty);

    private static JsonObject MergeEntry(JsonObject original, string id, JsonObject entry)
        => ToolRecordJson.Merge((original[ToolRecordDefinitions.ToolsProperty] as JsonObject)?[id] as JsonObject, entry);

    internal static JsonObject Object(JsonNode? node, string key) => node as JsonObject ?? throw Invalid(key, "an object");
    internal static string String(JsonObject entry, string key) => OptionalString(entry, key) ?? throw Invalid(key, "a non-empty string");
    private static string? OptionalString(JsonObject entry, string key)
    {
        if (!entry.ContainsKey(key)) return null;
        if (entry[key] is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text)) return text;
        throw Invalid(key, "a non-empty string");
    }
    private static JsonElement Element(JsonNode? node)
    {
        using var document = JsonDocument.Parse(node?.ToJsonString() ?? "null");
        return document.RootElement.Clone();
    }
    private static string Pointer(JsonObject entry)
    {
        var pointer = String(entry, ToolRecordDefinitions.PointerProperty);
        if (!pointer.StartsWith('/')) throw Invalid(ToolRecordDefinitions.PointerProperty, "a JSON Pointer");
        return pointer;
    }
    private static ArgumentException Invalid(string key, string shape) => new($"\"{key}\" requires {shape}.");
}
