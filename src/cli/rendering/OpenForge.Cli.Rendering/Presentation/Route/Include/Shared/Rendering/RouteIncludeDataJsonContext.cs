using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Include.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Include.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteIncludeData))]
internal sealed partial class RouteIncludeDataJsonContext : JsonSerializerContext;
