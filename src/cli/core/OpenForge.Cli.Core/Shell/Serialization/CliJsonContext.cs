using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.Core.Shell.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(CliProcessCompletion))]
internal sealed partial class CliJsonContext : JsonSerializerContext;
