using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ExtensionManifestDocument))]
internal sealed partial class ExtensionPackageJsonContext : JsonSerializerContext;
