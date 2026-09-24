using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal static partial class RouteRemoveRecoveryLifecycle
{
    private static RouteRemoveRecovery Recovery(
        RouteRemovePlan plan,
        RouteRemoveRecoveryState state,
        string? residualPath)
        => new()
        {
            State = state,
            ProtectedPaths = state == RouteRemoveRecoveryState.Removed
                ? []
                : plan.Preview.Recovery.ProtectedPaths,
            ResidualPath = residualPath,
        };

    private static RouteRemoveFinding Finding(
        RouteRemovePlan plan,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string cause,
        string? target = null)
        => new(code, status, target ?? plan.Preview.Source.Path, cause);
}
