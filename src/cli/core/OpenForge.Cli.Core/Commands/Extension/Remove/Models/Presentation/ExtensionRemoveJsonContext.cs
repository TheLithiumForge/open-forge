using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Presentation;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(ExtensionRemoveJsonDocument))]
internal sealed partial class ExtensionRemoveJsonContext : JsonSerializerContext;
