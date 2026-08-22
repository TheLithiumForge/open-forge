using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Serialization;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectJsonRenderer
{
    internal static string Render(CliPresentationRequest<RouteInspectResult> presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var document = RouteInspectJsonProjection.Create(presentation.Result);
        return JsonSerializer.Serialize(
            document,
            CliJsonContext.Default.RouteInspectJsonDocument);
    }
}
