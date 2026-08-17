using System.Text.Json.Serialization;

namespace OpenForge.Cli.Foundation;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(FoundationSnapshot))]
internal partial class FoundationJsonContext : JsonSerializerContext;
