using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

internal static class ToolRecordJson
{
    private static readonly string NativeSettingsProperty = Property(nameof(ToolWorkspaceMachineState.NativeSettings));
    private static readonly string PathChangesProperty = Property(nameof(ToolPathState.Changes));
    private static readonly string PathTargetProperty = Property(nameof(ToolPathChangeRecord.Target));
    private static readonly string[] NativeIdentity = [Property(nameof(ToolNativeSettingRecord.ToolId)), Property(nameof(ToolNativeSettingRecord.ChangeId)), Property(nameof(ToolNativeSettingRecord.NativeHomePath))];
    private static string Property(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
    private static readonly string RecordedJsonProperty = JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolRecordedValue.Json));
    internal static JsonObject Read(ReadOnlyMemory<byte> bytes)
    {
        _ = new UTF8Encoding(false, true).GetCharCount(bytes.Span);
        return JsonNode.Parse(bytes.Span, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip }) as JsonObject
            ?? throw new ArgumentException("The record requires a JSON object.");
    }

    internal static byte[] Write(JsonObject root) => Encoding.UTF8.GetBytes(root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");

    internal static JsonObject Merge(JsonObject? original, JsonObject replacement)
    {
        var result = original is null ? new JsonObject() : (JsonObject)original.DeepClone();
        foreach (var (key, value) in replacement)
        {
            if (key == RecordedJsonProperty) result[key] = value?.DeepClone();
            else if (value is JsonObject child) result[key] = Merge(result[key] as JsonObject, child);
            else if (value is JsonArray array && result[key] is JsonArray prior)
            {
                var merged = new JsonArray();
                foreach (var item in array)
                {
                    if (item is not JsonObject entry) { ((IList<JsonNode?>)merged).Add(item?.DeepClone()); continue; }
                    var identityKey = new[] { "id", "changeId", "launcherPath", "source" }.FirstOrDefault(entry.ContainsKey);
                    JsonObject? old = null;
                    if (key == NativeSettingsProperty)
                        old = prior.OfType<JsonObject>().FirstOrDefault(candidate => SameNativeIdentity(candidate, entry));
                    else if (key == PathChangesProperty && entry.ContainsKey(PathTargetProperty))
                        old = prior.OfType<JsonObject>().FirstOrDefault(candidate => SamePathIdentity(candidate, entry));
                    else if (identityKey is not null)
                        old = prior.OfType<JsonObject>().FirstOrDefault(candidate => JsonNode.DeepEquals(candidate[identityKey], entry[identityKey]));
                    ((IList<JsonNode?>)merged).Add(Merge(old, entry));
                }
                result[key] = merged;
            }
            else result[key] = value?.DeepClone();
        }
        return result;
    }
    private static bool SameNativeIdentity(JsonObject first, JsonObject second)
    {
        foreach (var part in NativeIdentity)
        {
            if (part != NativeIdentity[^1])
            {
                if (second[part] is null || !JsonNode.DeepEquals(first[part], second[part])) return false;
                continue;
            }
            if (first[part] is not JsonValue before || second[part] is not JsonValue after
                || !before.TryGetValue<string>(out var oldHome) || !after.TryGetValue<string>(out var newHome)
                || string.IsNullOrWhiteSpace(oldHome) || string.IsNullOrWhiteSpace(newHome)) return false;
            if (!PhysicalIdentityTracker.PathComparer.Equals(oldHome, newHome)) return false;
        }
        return true;
    }
    private static bool SamePathIdentity(JsonObject first, JsonObject second)
    {
        var before = PathChange(first);
        var after = PathChange(second);
        return before is not null && after is not null && before.Kind == after.Kind
            && PhysicalIdentityTracker.PathComparer.Equals(before.Target, after.Target)
            && before.StartMarker == after.StartMarker && before.EndMarker == after.EndMarker;
    }

    private static ToolPathChangeRecord? PathChange(JsonObject value)
    {
        try { return value.Deserialize(ToolMachineJsonContext.Default.ToolPathChangeRecord); }
        catch (JsonException) { return null; }
    }
}
