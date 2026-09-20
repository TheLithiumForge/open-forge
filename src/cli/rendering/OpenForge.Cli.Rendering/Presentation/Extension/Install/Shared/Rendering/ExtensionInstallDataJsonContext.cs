using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(ExtensionInstallData))]
internal sealed partial class ExtensionInstallDataJsonContext : JsonSerializerContext;
