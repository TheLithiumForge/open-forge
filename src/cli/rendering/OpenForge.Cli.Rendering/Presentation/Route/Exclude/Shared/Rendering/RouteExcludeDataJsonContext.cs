using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteExcludeData))]
internal sealed partial class RouteExcludeDataJsonContext : JsonSerializerContext;
