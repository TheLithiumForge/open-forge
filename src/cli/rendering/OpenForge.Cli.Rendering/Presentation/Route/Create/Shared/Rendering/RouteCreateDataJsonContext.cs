using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Create.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteCreateData))]
internal sealed partial class RouteCreateDataJsonContext : JsonSerializerContext;
