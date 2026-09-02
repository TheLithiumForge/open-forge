using OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateJsonProjection
{
    private static RouteUpdateJsonEffect Effect(RouteUpdateEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = RouteUpdateDefinitions.ReadMachineName(effect.Kind),
            Action = RouteUpdateDefinitions.ReadMachineName(effect.Action),
            Change = new RouteUpdateJsonChange
            {
                Before = effect.Change.Before,
                Expected = effect.Change.Expected,
            },
            Preview = effect.Preview.Select(Preview).ToArray(),
            Outcome = RouteUpdateDefinitions.ReadMachineName(effect.Outcome),
            Residual = RouteUpdateDefinitions.ReadMachineName(effect.Residual),
        };

    private static RouteUpdateJsonPreviewHunk Preview(RouteUpdatePreviewHunk preview)
        => new()
        {
            Kind = RouteUpdateDefinitions.ReadMachineName(preview.Kind),
            Before = preview.Before,
            Expected = preview.Expected,
        };
}
