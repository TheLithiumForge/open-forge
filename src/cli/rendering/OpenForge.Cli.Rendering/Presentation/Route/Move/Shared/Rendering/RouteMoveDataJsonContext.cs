using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Presentation.Route.Move.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Move.Shared.Rendering;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RouteMoveData))]
internal sealed partial class RouteMoveDataJsonContext : JsonSerializerContext;
