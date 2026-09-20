using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal static partial class RouteRemoveRecoveryLifecycle
{
    internal static async ValueTask<RouteRemoveRecoveryDeletionResult> DeleteExactAsync(
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
            ? catalogue.Candidates.SingleOrDefault(value => RecoveryBundleIdentity.Matches(value, input.Preparation))
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

    private static async ValueTask<RecoveryBundleCatalogueResult> ReadDeletionCatalogueAsync(
        RouteRemoveRecoveryDeletionInput input,
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

    private static async ValueTask<RouteRemoveRecoveryDeletionResult> DeleteCandidateAsync(
        RouteRemoveRecoveryDeletionInput input,
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

    private static RouteRemoveRecoveryDeletionResult FromDeletion(
        RouteRemoveRecoveryDeletionInput input,
        RecoveryBundleDeletionResult deletion)
        => (deletion.State, deletion.Disposition) switch
        {
            (RecoveryBundleDeletionState.Deleted, RecoveryBundleDisposition.Removed) => new()
            {
                Recovery = Recovery(input.Held.Plan, RouteRemoveRecoveryState.Removed, residualPath: null),
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
                input.Held.Plan,
                retainedPath is null
                    ? RouteRemoveRecoveryState.Unknown
                    : RouteRemoveRecoveryState.Retained,
                retainedPath),
            Finding = Finding(
                input.Held.Plan,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Remove recovery deletion was interrupted."),
        };

    private static RouteRemoveRecoveryDeletionResult RetainedDeletion(
        RouteRemoveRecoveryDeletionInput input,
        string cause)
        => new()
        {
            Recovery = Recovery(
                input.Held.Plan,
                RouteRemoveRecoveryState.Retained,
                input.Preparation.BundlePath),
            Finding = Finding(
                input.Held.Plan,
                RouteRemoveFindingCode.RecoveryArtifactRetained,
                CliSemanticStatus.Attention,
                cause),
        };

    private static RouteRemoveRecoveryDeletionResult UnknownDeletion(
        RouteRemoveRecoveryDeletionInput input,
        string cause)
        => new()
        {
            Recovery = Recovery(input.Held.Plan, RouteRemoveRecoveryState.Unknown, residualPath: null),
            Finding = Finding(
                input.Held.Plan,
                RouteRemoveFindingCode.RecoveryFailed,
                CliSemanticStatus.Failed,
                cause),
        };
}
