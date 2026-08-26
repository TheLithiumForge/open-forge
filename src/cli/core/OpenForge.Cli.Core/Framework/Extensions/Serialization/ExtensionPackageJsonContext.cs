using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Extensions.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ExtensionManifestDocument))]
[JsonSerializable(typeof(EmbeddedExtensionInventoryDocument))]
internal sealed partial class ExtensionPackageJsonContext : JsonSerializerContext;
