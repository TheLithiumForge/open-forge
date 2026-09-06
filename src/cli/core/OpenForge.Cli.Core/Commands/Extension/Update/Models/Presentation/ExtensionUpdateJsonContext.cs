using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Presentation;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionUpdateJsonDocument))]
internal sealed partial class ExtensionUpdateJsonContext : JsonSerializerContext;
