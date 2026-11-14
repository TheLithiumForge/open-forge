using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenForge.Cli.Composition.Shared.Tools.Npm.Models;

internal sealed record ToolNpmPackument
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("versions")] public Dictionary<string, ToolNpmPublishedVersion?>? Versions { get; init; }
}

internal sealed record ToolNpmPublishedVersion
{
    [JsonPropertyName("version")] public string? Version { get; init; }
    [JsonPropertyName("bin")] public JsonElement Bin { get; init; }
    [JsonPropertyName("engines")] public Dictionary<string, string>? Engines { get; init; }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ToolNpmPackument))]
internal sealed partial class ToolNpmMetadataJsonContext : JsonSerializerContext;
