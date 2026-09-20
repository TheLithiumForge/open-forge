using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteInspectData))]
internal sealed partial class RouteInspectDataJsonContext : JsonSerializerContext;
