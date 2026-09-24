using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Remove.Models;

namespace OpenForge.Cli.Core.Presentation.Remove.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(RemoveData))]
internal sealed partial class RemoveDataJsonContext : JsonSerializerContext;
