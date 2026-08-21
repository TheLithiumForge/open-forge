using System.Text.Json;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteListResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        var document = RouteListJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(
            document,
            CliJsonContext.Default.RouteListJsonDocument);
    }
}
