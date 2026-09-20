using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(OpenForge.Cli.Core.Presentation.Install.Models.InstallData))]
internal sealed partial class InstallDataJsonContext : JsonSerializerContext
{
}
