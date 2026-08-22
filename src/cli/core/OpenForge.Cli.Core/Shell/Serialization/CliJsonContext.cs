using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(CliProcessCompletion))]
[JsonSerializable(typeof(RouteListJsonDocument))]
[JsonSerializable(typeof(RouteInspectJsonDocument))]
internal sealed partial class CliJsonContext : JsonSerializerContext;
