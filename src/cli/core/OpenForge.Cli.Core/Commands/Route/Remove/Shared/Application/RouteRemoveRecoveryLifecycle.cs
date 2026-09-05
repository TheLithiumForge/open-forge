using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveRecoveryLifecycle
{
    private readonly RecoveryBundleCatalogue _catalogue;
    private readonly RecoveryBundleStore _store;
    private readonly RecoveryBundleDeletionGuard _deletionGuard;

    internal RouteRemoveRecoveryLifecycle(
        RecoveryBundleCatalogue catalogue,
        RecoveryBundleStore store,
        RecoveryBundleDeletionGuard deletionGuard)
    {
        _catalogue = catalogue;
        _store = store;
        _deletionGuard = deletionGuard;
    }

    internal static RouteRemoveRecoveryLifecycle Create()
    {
        var reader = new RecoveryBundleReader();
        var catalogue = new RecoveryBundleCatalogue(reader);
        return new RouteRemoveRecoveryLifecycle(
            catalogue,
            new RecoveryBundleStore(reader),
            new RecoveryBundleDeletionGuard(catalogue, reader));
    }

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
        string cause)
        => new(code, status, plan.Preview.Source.Path, cause);
}
