using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteCreateJsonDocument))]
internal sealed partial class RouteCreateJsonContext : JsonSerializerContext;
