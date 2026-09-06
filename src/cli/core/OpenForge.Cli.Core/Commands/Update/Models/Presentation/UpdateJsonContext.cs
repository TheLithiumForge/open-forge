using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Update.Models.Presentation;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(UpdateJsonDocument))]
internal sealed partial class UpdateJsonContext : JsonSerializerContext;
