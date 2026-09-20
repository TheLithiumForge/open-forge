using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(OpenForge.Cli.Core.Presentation.Cleanup.Models.CleanupData))]
internal sealed partial class CleanupDataJsonContext : JsonSerializerContext
{
}
