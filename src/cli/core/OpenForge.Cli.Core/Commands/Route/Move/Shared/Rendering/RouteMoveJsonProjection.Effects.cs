using OpenForge.Cli.Core.Commands.Route.Move.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static partial class RouteMoveJsonProjection
{
    private static RouteMoveJsonEffect Effect(RouteMoveEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = RouteMoveDefinitions.ReadMachineName(effect.Kind),
            Action = RouteMoveDefinitions.ReadMachineName(effect.Action),
            Before = PathState(effect.Before),
            Expected = PathState(effect.Expected),
            Outcome = RouteMoveDefinitions.ReadMachineName(effect.Outcome),
            Residual = RouteMoveDefinitions.ReadMachineName(effect.Residual),
        };

    private static RouteMoveJsonPathState PathState(RouteMovePathState state)
        => new()
        {
            Kind = RouteMoveDefinitions.ReadMachineName(state.Kind),
            ContentSha256 = state.ContentSha256,
        };
}
