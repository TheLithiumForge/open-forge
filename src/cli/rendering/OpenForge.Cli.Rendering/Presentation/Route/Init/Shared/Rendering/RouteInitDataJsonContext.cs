using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Init.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteInitData))]
internal sealed partial class RouteInitDataJsonContext : JsonSerializerContext;
