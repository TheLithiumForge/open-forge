using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveRecoveryLifecycle
{
    private readonly RecoveryBundleCatalogue _catalogue;
    private readonly RecoveryBundleStore _store;
    private readonly RecoveryBundleDeletionGuard _deletionGuard;

    internal RouteMoveRecoveryLifecycle(
        RecoveryBundleCatalogue catalogue,
        RecoveryBundleStore store,
        RecoveryBundleDeletionGuard deletionGuard)
    {
        _catalogue = catalogue;
        _store = store;
        _deletionGuard = deletionGuard;
    }

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
