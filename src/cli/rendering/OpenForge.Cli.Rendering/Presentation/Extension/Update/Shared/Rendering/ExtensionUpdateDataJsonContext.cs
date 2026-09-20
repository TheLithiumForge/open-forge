using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(ExtensionUpdateData))]
internal sealed partial class ExtensionUpdateDataJsonContext : JsonSerializerContext;
