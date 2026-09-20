using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = new[] { typeof(ExtensionListDataInstalledJsonConverter) })]
[JsonSerializable(typeof(ExtensionListData))]
internal sealed partial class ExtensionListDataJsonContext : JsonSerializerContext;
