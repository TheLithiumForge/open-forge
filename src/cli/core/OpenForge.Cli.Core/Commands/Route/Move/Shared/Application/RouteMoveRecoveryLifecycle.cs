using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal static partial class RouteMoveRecoveryLifecycle
{
    private static RouteMoveRecovery Recovery(
        RouteMovePlan plan,
        RouteMoveRecoveryState state,
        string? residualPath)
        => new()
        {
            State = state,
            ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
            ResidualPath = residualPath,
        };

    private static RouteMoveFinding Finding(
        RouteMovePlan plan,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new(code, status, plan.Preview.Destination.Path, cause);
}
