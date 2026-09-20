using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = new[] { typeof(ExtensionInspectDataJsonConverter) })]
[JsonSerializable(typeof(ExtensionInspectData))]
internal sealed partial class ExtensionInspectDataJsonContext : JsonSerializerContext;
