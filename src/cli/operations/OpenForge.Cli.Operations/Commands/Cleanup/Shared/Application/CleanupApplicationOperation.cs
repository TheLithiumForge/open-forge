using OpenForge.Cli.Core.Commands.Cleanup.Models.Planning;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Commands.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;

internal sealed class CleanupApplicationOperation(WorkspaceLockManager lockManager)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;

    internal async ValueTask ApplyAsync(
        IReadOnlyList<CleanupStoreApplication> stores,
        CancellationToken cancellationToken)
    {
        var selected = stores.Where(store => !store.Result.Plan.DeletionEntries.IsEmpty).ToArray();
        var request = selected[0].Result.Plan.Request
            ?? throw new InvalidOperationException("Cleanup application requires an established request.");
        var acquired = await _lockManager.AcquireAsync(
            new WorkspaceLockRequest(request.Workspace, CleanupDefinitions.CommandIdentity, CleanupPlanner.LeaseOperationId(request)),
            cancellationToken).ConfigureAwait(false);
        var leaseFact = new CleanupLease
        {
            State = ReadLeaseState(acquired.State),
            Cause = acquired.Cause,
        };
        foreach (var store in selected)
        {
            store.Result.Lease = leaseFact;
        }

        if (acquired.Lease is not { } lease)
        {
            foreach (var store in selected)
            {
                var result = store.Result;
                result.PreserveUnattempted(observed: null);
                var code = acquired.State == WorkspaceLockState.Cancelled ? CleanupFindingCode.Interrupted : CleanupFindingCode.WorkspaceLockUnavailable;
                result.Findings = result.Findings.Add(CleanupFinding.Create(code, acquired.Cause ?? "Workspace lease acquisition was interrupted.") with
                {
                    LockFailure = WorkspaceLockFailureProjection.From(acquired.LockFailure ?? throw new InvalidOperationException("A failed workspace lock requires its typed failure.")),
                });
            }

            return;
        }

        await using (lease.ConfigureAwait(false))
        {
            foreach (var store in selected)
            {
                await ApplyStoreAsync(store, lease, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static async ValueTask ApplyStoreAsync(CleanupStoreApplication store, WorkspaceLockLease lease, CancellationToken cancellationToken)
    {
        var result = store.Result;
        var request = result.Plan.Request ?? throw new InvalidOperationException("A store plan requires a request.");
        var validation = result.Plan.LayoutRead is { } layoutRead
            ? await new WorkspaceLayoutRevalidator(new PhysicalPathResolver()).ValidateAsync(lease, layoutRead, cancellationToken).ConfigureAwait(false)
            : MutationValidationResult.Blocked("The Cleanup plan has no workspace layout observation.");
        if (validation.State != MutationValidationState.Valid)
        {
            result.Revalidation = new CleanupCatalogueComparison
            {
                State = validation.State == MutationValidationState.Cancelled
                    ? CleanupCatalogueComparisonState.Cancelled : CleanupCatalogueComparisonState.Blocked,
                Planned = result.Plan.Catalogue,
                Cause = validation.Cause,
            };
            result.PreserveUnattempted(observed: null);
            result.AddFinding(validation.State == MutationValidationState.Cancelled
                ? CleanupFindingCode.Interrupted : CleanupFindingCode.StoreValidationUnavailable,
                validation.Cause ?? "The planned workspace layout changed.");
            return;
        }
        var frozenStore = store.Observation;
        var opened = await RecoveryBundleDeletionGuard.OpenSessionAsync(lease, frozenStore, cancellationToken).ConfigureAwait(false);
        result.Revalidation = new CleanupCatalogueComparison
        {
            State = ReadComparison(opened.State),
            Planned = result.Plan.Catalogue,
            Observed = opened.ObservedCatalogue is { } observed ? CleanupPlanner.ReadCatalogue(request, observed) : null,
            Cause = opened.Cause,
        };
        if (opened.Session is not { } session)
        {
            result.PreserveUnattempted(opened.ObservedCatalogue);
            var code = opened.State switch
            {
                RecoveryBundleDeletionSessionOpenState.Cancelled => CleanupFindingCode.Interrupted,
                RecoveryBundleDeletionSessionOpenState.Blocked or RecoveryBundleDeletionSessionOpenState.Unavailable
                    => CleanupFindingCode.StoreValidationUnavailable,
                _ => CleanupFindingCode.CatalogueChangedDuringApply,
            };
            result.AddFinding(code, opened.Cause ?? "Cleanup was interrupted while validating the frozen catalogue.", frozenStore.Location?.WorkspaceDirectory);
            return;
        }

        var baselineFindings = result.Findings;
        var progress = new CleanupDeletionProgress(result.Plan);
        while (progress.Next is { } entry)
        {
            RecoveryBundleDeletionResult deletion;
            try
            {
                deletion = await session.DeleteAsync(entry.Candidate.Snapshot, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                deletion = new RecoveryBundleDeletionResult(
                    RecoveryBundleDeletionState.Cancelled, RecoveryBundleDisposition.Unknown, entry.Path, failure: null, cause: null);
            }
            catch (Exception exception)
            {
                deletion = RecoveryBundleDeletionResult.FailedUnknown(entry.Path, $"The deletion did not establish its disposition: {exception.Message}");
            }

            progress.Record(deletion);
            result.Effects = progress.Effects;
            result.Findings = [.. baselineFindings, .. progress.Findings];
        }

        result.Verification = new CleanupVerification
        {
            State = ReadVerification(progress.Effects),
            Cause = progress.Findings.FirstOrDefault()?.Cause,
        };
    }

    private static CleanupLeaseState ReadLeaseState(WorkspaceLockState state)
        => state switch
        {
            WorkspaceLockState.Acquired => CleanupLeaseState.Acquired,
            WorkspaceLockState.Failed => CleanupLeaseState.Failed,
            WorkspaceLockState.Cancelled => CleanupLeaseState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The workspace lease state is not defined."),
        };

    private static CleanupCatalogueComparisonState ReadComparison(RecoveryBundleDeletionSessionOpenState state)
        => state switch
        {
            RecoveryBundleDeletionSessionOpenState.Opened => CleanupCatalogueComparisonState.Matched,
            RecoveryBundleDeletionSessionOpenState.Changed => CleanupCatalogueComparisonState.Changed,
            RecoveryBundleDeletionSessionOpenState.Unavailable => CleanupCatalogueComparisonState.Incomplete,
            RecoveryBundleDeletionSessionOpenState.Blocked => CleanupCatalogueComparisonState.Blocked,
            RecoveryBundleDeletionSessionOpenState.Cancelled => CleanupCatalogueComparisonState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The deletion session opening state is not defined."),
        };

    private static CleanupVerificationState ReadVerification(IEnumerable<CleanupEffect> effects)
    {
        var materialized = effects.ToArray();
        if (materialized.All(effect => effect.Outcome == CleanupEffectOutcome.Verified))
        {
            return CleanupVerificationState.Verified;
        }

        if (materialized.Any(effect => effect.Outcome == CleanupEffectOutcome.VerificationFailed))
        {
            return CleanupVerificationState.Failed;
        }

        return CleanupVerificationState.Unknown;
    }
}
