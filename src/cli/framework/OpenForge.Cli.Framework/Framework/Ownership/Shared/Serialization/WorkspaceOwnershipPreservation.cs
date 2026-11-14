using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

internal static class WorkspaceOwnershipPreservation
{
    private static readonly IReadOnlyDictionary<Type, string[]> RowIdentities = new Dictionary<Type, string[]>
    {
        [typeof(ExtensionOwnershipEntry)] = [Property(nameof(ExtensionOwnershipEntry.Id))],
        [typeof(LibraryOwnershipEntry)] = [Property(nameof(LibraryOwnershipEntry.Id))],
        [typeof(OwnedRegionEntry)] = [Property(nameof(OwnedRegionEntry.Path)), Property(nameof(OwnedRegionEntry.Region))],
        [typeof(GitIgnoredRouteEntry)] = [Property(nameof(GitIgnoredRouteEntry.Directory)), Property(nameof(GitIgnoredRouteEntry.Entrypoint))],
    };

    internal static byte[] Merge(JsonElement original, byte[] generated)
    {
        if (original.ValueKind != JsonValueKind.Object) return generated;
        var before = JsonNode.Parse(original.GetRawText()) as JsonObject;
        var after = JsonNode.Parse(generated) as JsonObject
            ?? throw new InvalidOperationException("The ownership writer requires an object.");
        if (before?.TryGetPropertyValue(WorkspaceOwnershipDefinitions.SchemaProperty, out var schema) == true)
            after[WorkspaceOwnershipDefinitions.SchemaProperty] = schema?.DeepClone();
        var merged = MergeObject(before, after, WorkspaceOwnershipJsonContext.Default.WorkspaceOwnershipEnvelope);
        return Encoding.UTF8.GetBytes(merged.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static JsonObject MergeObject(JsonObject? before, JsonObject after, JsonTypeInfo schema)
    {
        var result = before is null ? new JsonObject() : (JsonObject)before.DeepClone();
        // Generated metadata owns known members, including omitted defaults and nulls.
        foreach (var property in schema.Properties)
            if (!after.ContainsKey(property.Name)) result.Remove(property.Name);
        foreach (var (key, value) in after)
        {
            var property = schema.Properties.FirstOrDefault(candidate => candidate.Name == key);
            var childSchema = property is null ? null : WorkspaceOwnershipJsonContext.Default.GetTypeInfo(property.PropertyType);
            if (value is JsonObject child && childSchema?.Kind == JsonTypeInfoKind.Object)
                result[key] = MergeObject(result[key] as JsonObject, child, childSchema);
            else if (value is JsonArray array)
            {
                var rowSchema = RowSchema(schema, key, childSchema);
                result[key] = MergeArray(result[key] as JsonArray, array, rowSchema);
            }
            else result[key] = value?.DeepClone();
        }
        return result;
    }

    private static JsonTypeInfo? RowSchema(JsonTypeInfo parent, string key, JsonTypeInfo? property)
    {
        if (parent.Type == typeof(FrameworkOwnershipEntry) && key == Property(nameof(FrameworkOwnershipEntry.GitIgnoredRoutes)))
            return WorkspaceOwnershipJsonContext.Default.GitIgnoredRouteEntry;
        return property?.ElementType is { } element ? WorkspaceOwnershipJsonContext.Default.GetTypeInfo(element) : null;
    }

    private static JsonArray MergeArray(JsonArray? before, JsonArray after, JsonTypeInfo? rowSchema)
    {
        if (rowSchema is null || !RowIdentities.TryGetValue(rowSchema.Type, out var identity)) return (JsonArray)after.DeepClone();
        var merged = new JsonArray();
        foreach (var item in after)
        {
            if (item is not JsonObject entry) { ((IList<JsonNode?>)merged).Add(item?.DeepClone()); continue; }
            var old = before?.OfType<JsonObject>().FirstOrDefault(candidate => identity.All(key =>
                entry[key] is not null && JsonNode.DeepEquals(candidate[key], entry[key])));
            ((IList<JsonNode?>)merged).Add(MergeObject(old, entry, rowSchema));
        }
        return merged;
    }

    private static string Property(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);
}
