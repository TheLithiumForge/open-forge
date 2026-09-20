using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Repair.Models;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(RepairData))]
internal sealed partial class RepairDataJsonContext : JsonSerializerContext
{
}
