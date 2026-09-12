using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteListResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        var document = RouteListJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(
            document,
            RouteListJsonContext.Default.RouteListJsonDocument);
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteListJsonDocument))]
internal sealed partial class RouteListJsonContext : JsonSerializerContext;
