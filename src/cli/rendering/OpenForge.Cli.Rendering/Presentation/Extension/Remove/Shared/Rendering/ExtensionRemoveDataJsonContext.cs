using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(ExtensionRemoveData))]
internal sealed partial class ExtensionRemoveDataJsonContext : JsonSerializerContext;
