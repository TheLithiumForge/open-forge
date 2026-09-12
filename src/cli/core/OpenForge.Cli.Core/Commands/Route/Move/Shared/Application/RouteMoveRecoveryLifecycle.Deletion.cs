using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal static partial class RouteMoveRecoveryLifecycle
{
    internal static async ValueTask<RouteMoveRecoveryDeletionResult> DeleteExactAsync(
        RouteMoveRecoveryDeletionInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var catalogue = await ReadDeletionCatalogueAsync(input, cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return InterruptedDeletion(input, retainedPath: null);
        }

        var candidate = catalogue.State == RecoveryBundleCatalogueState.Available
            ? catalogue.Candidates.SingleOrDefault(value => RecoveryBundleIdentity.Matches(value, input.Preparation))
            : null;
        if (candidate is null)
        {
            return UnknownDeletion(
                input,
                catalogue.Cause
                    ?? "The exact prepared Route Move recovery final could not be selected for deletion.");
        }

        return await DeleteCandidateAsync(input, candidate, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask<RecoveryBundleCatalogueResult> ReadDeletionCatalogueAsync(
        RouteMoveRecoveryDeletionInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            return await RecoveryBundleCatalogue.ReadAsync(input.Held.Plan.Request.Workspace, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleCatalogueResult.Cancelled();
        }
        catch (Exception)
        {
            return RecoveryBundleCatalogueResult.Unavailable(
                "The recovery catalogue failed during deletion.");
        }
    }

    private static async ValueTask<RouteMoveRecoveryDeletionResult> DeleteCandidateAsync(
        RouteMoveRecoveryDeletionInput input,
        RecoveryBundleCandidateSnapshot candidate,
        CancellationToken cancellationToken)
    {
        try
        {
            var deletion = await RecoveryBundleDeletionGuard.DeleteAsync(
                input.Held.Lease,
                candidate,
                cancellationToken).ConfigureAwait(false);
            return FromDeletion(input, deletion);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return InterruptedDeletion(input, candidate.Path);
        }
        catch (Exception)
        {
            return UnknownDeletion(input, "Recovery deletion failed unexpectedly.");
        }
    }

    private static RouteMoveRecoveryDeletionResult FromDeletion(
        RouteMoveRecoveryDeletionInput input,
        RecoveryBundleDeletionResult deletion)
        => (deletion.State, deletion.Disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) => new()
            {
                Recovery = Recovery(input.Held.Plan, RouteMoveRecoveryState.Removed, residualPath: null),
            },
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained) =>
                InterruptedDeletion(input, input.Preparation.BundlePath),
            (RecoveryBundleDeletionState.Cancelled, _) =>
                InterruptedDeletion(input, retainedPath: null),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained) =>
                RetainedDeletion(
                    input,
                    deletion.Cause ?? "The verified Route Move recovery artifact was retained."),
            _ => UnknownDeletion(
                input,
                deletion.Cause ?? "The Route Move recovery artifact disposition is unknown."),
        };

    private static RouteMoveRecoveryDeletionResult InterruptedDeletion(
        RouteMoveRecoveryDeletionInput input,
        string? retainedPath)
        => new()
        {
            Recovery = Recovery(
                input.Held.Plan,
                retainedPath is null
                    ? RouteMoveRecoveryState.Unknown
                    : RouteMoveRecoveryState.Retained,
                retainedPath),
            Finding = Finding(
                input.Held.Plan,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Move recovery deletion was interrupted."),
        };

    private static RouteMoveRecoveryDeletionResult RetainedDeletion(
        RouteMoveRecoveryDeletionInput input,
        string cause)
        => new()
        {
            Recovery = Recovery(
                input.Held.Plan,
                RouteMoveRecoveryState.Retained,
                input.Preparation.BundlePath),
            Finding = Finding(
                input.Held.Plan,
                RouteMoveFindingCode.RecoveryArtifactRetained,
                CliSemanticStatus.Attention,
                cause),
        };

    private static RouteMoveRecoveryDeletionResult UnknownDeletion(
        RouteMoveRecoveryDeletionInput input,
        string cause)
        => new()
        {
            Recovery = Recovery(input.Held.Plan, RouteMoveRecoveryState.Unknown, residualPath: null),
            Finding = Finding(
                input.Held.Plan,
                RouteMoveFindingCode.RecoveryFailed,
                CliSemanticStatus.Failed,
                cause),
        };
}
