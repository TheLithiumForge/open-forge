using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Update.Models;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Rendering;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(UpdateData))]
internal sealed partial class UpdateDataJsonContext : JsonSerializerContext;
