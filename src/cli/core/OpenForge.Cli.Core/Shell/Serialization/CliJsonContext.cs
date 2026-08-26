using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Context.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.References.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(CliProcessCompletion))]
[JsonSerializable(typeof(ContextJsonDocument))]
[JsonSerializable(typeof(FindJsonDocument))]
[JsonSerializable(typeof(ReferencesJsonDocument))]
[JsonSerializable(typeof(RouteListJsonDocument))]
[JsonSerializable(typeof(RouteInspectJsonDocument))]
internal sealed partial class CliJsonContext : JsonSerializerContext;
