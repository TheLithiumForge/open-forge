using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveRecoveryLifecycle
{
    internal async ValueTask<RouteRemoveRecoveryDeletionResult> DeleteExactAsync(
        RouteRemoveRecoveryDeletionInput input,
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
            ? catalogue.Candidates.SingleOrDefault(value => Matches(value, input.Preparation))
            : null;
        if (candidate is null)
        {
            return UnknownDeletion(
                input,
                catalogue.Cause
                    ?? "The exact prepared Route Remove recovery final could not be selected for deletion.");
        }

        return await DeleteCandidateAsync(input, candidate, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask<RecoveryBundleCatalogueResult> ReadDeletionCatalogueAsync(
        RouteRemoveRecoveryDeletionInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _catalogue.ReadAsync(input.Plan.Request.Workspace, cancellationToken)
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

    private async ValueTask<RouteRemoveRecoveryDeletionResult> DeleteCandidateAsync(
        RouteRemoveRecoveryDeletionInput input,
        RecoveryBundleCandidateSnapshot candidate,
        CancellationToken cancellationToken)
    {
        try
        {
            var deletion = await _deletionGuard.DeleteAsync(
                input.Lease,
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

    private static RouteRemoveRecoveryDeletionResult FromDeletion(
        RouteRemoveRecoveryDeletionInput input,
        RecoveryBundleDeletionResult deletion)
        => (deletion.State, deletion.Disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) => new()
            {
                Recovery = Recovery(input.Plan, RouteRemoveRecoveryState.Removed, residualPath: null),
            },
            (RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Retained) =>
                InterruptedDeletion(input, input.Preparation.BundlePath),
            (RecoveryBundleDeletionState.Cancelled, _) =>
                InterruptedDeletion(input, retainedPath: null),
            (RecoveryBundleDeletionState.Failed, RecoveryBundleDisposition.Retained) =>
                RetainedDeletion(
                    input,
                    deletion.Cause ?? "The verified Route Remove recovery artifact was retained."),
            _ => UnknownDeletion(
                input,
                deletion.Cause ?? "The Route Remove recovery artifact disposition is unknown."),
        };

    private static RouteRemoveRecoveryDeletionResult InterruptedDeletion(
        RouteRemoveRecoveryDeletionInput input,
        string? retainedPath)
        => new()
        {
            Recovery = Recovery(
                input.Plan,
                retainedPath is null
                    ? RouteRemoveRecoveryState.Unknown
                    : RouteRemoveRecoveryState.Retained,
                retainedPath),
            Finding = Finding(
                input.Plan,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Remove recovery deletion was interrupted."),
        };

    private static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
        => candidate.Kind == RecoveryBundleCandidateKind.Final
            && candidate.Integrity == RecoveryBundleIntegrity.Verified
            && candidate.Verified is { } verified
            && PhysicalIdentityTracker.PathComparer.Equals(candidate.Path, preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(verified.Command, preparation.Command, StringComparison.Ordinal)
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);

    private static RouteRemoveRecoveryDeletionResult RetainedDeletion(
        RouteRemoveRecoveryDeletionInput input,
        string cause)
        => new()
        {
            Recovery = Recovery(
                input.Plan,
                RouteRemoveRecoveryState.Retained,
                input.Preparation.BundlePath),
            Finding = Finding(
                input.Plan,
                RouteRemoveFindingCode.RecoveryArtifactRetained,
                CliSemanticStatus.Attention,
                cause),
        };

    private static RouteRemoveRecoveryDeletionResult UnknownDeletion(
        RouteRemoveRecoveryDeletionInput input,
        string cause)
        => new()
        {
            Recovery = Recovery(input.Plan, RouteRemoveRecoveryState.Unknown, residualPath: null),
            Finding = Finding(
                input.Plan,
                RouteRemoveFindingCode.RecoveryFailed,
                CliSemanticStatus.Failed,
                cause),
        };
}
