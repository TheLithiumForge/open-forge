using OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static partial class RouteRemoveJsonProjection
{
    private static RouteRemoveJsonEffect Effect(RouteRemoveEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = RouteRemoveDefinitions.ReadMachineName(effect.Kind),
            Action = RouteRemoveDefinitions.ReadMachineName(effect.Action),
            Before = PathState(effect.Before),
            Expected = PathState(effect.Expected),
            Outcome = RouteRemoveDefinitions.ReadMachineName(effect.Outcome),
            Residual = RouteRemoveDefinitions.ReadMachineName(effect.Residual),
        };

    private static RouteRemoveJsonPathState PathState(RouteRemovePathState state)
        => new()
        {
            Kind = RouteRemoveDefinitions.ReadMachineName(state.Kind),
            ContentSha256 = state.ContentSha256,
        };
}
