using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Update.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteUpdateData))]
internal sealed partial class RouteUpdateDataJsonContext : JsonSerializerContext;
