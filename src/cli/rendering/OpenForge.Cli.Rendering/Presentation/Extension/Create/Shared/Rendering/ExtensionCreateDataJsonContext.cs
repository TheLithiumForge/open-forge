using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExtensionCreateData))]
internal sealed partial class ExtensionCreateDataJsonContext : JsonSerializerContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ExtensionCreateDataManifestContent))]
internal sealed partial class ExtensionCreateManifestContentJsonContext : JsonSerializerContext;
