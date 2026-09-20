using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Remove.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Rendering;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(RouteRemoveData))]
internal sealed partial class RouteRemoveDataJsonContext : JsonSerializerContext;
