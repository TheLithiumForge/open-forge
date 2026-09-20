using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.List.Models;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteListData))]
internal sealed partial class RouteListDataJsonContext : JsonSerializerContext;
