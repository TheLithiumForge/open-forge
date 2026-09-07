using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Commands.Cleanup.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Application;

internal sealed class CleanupApplicationOperation(WorkspaceLockManager lockManager, RecoveryBundleDeletionGuard guard)
{
    private readonly WorkspaceLockManager _lockManager = lockManager;
    private readonly RecoveryBundleDeletionGuard _guard = guard;

    internal async ValueTask ApplyAsync(
        CleanupResultBuilder result,
        RecoveryBundleCatalogueResult frozenCatalogue,
        CancellationToken cancellationToken)
    {
        var request = result.Plan.Request
            ?? throw new InvalidOperationException("Cleanup application requires an established request.");
        var acquired = await _lockManager.AcquireAsync(
            new WorkspaceLockRequest(request.Workspace, CleanupDefinitions.CommandIdentity, CleanupPlanner.LeaseOperationId(request)),
            cancellationToken).ConfigureAwait(false);
        result.Lease = new CleanupLease
        {
            State = ReadLeaseState(acquired.State),
            Cause = acquired.Cause,
        };
        if (acquired.Lease is not { } lease)
        {
            result.PreserveUnattempted(observed: null);
            var code = acquired.State == WorkspaceLockState.Cancelled ? CleanupFindingCode.Interrupted : CleanupFindingCode.WorkspaceLockUnavailable;
            result.AddFinding(code, acquired.Cause ?? "Workspace lease acquisition was interrupted.");
            return;
        }

        await using (lease.ConfigureAwait(false))
        {
            var opened = await _guard.OpenSessionAsync(lease, frozenCatalogue, cancellationToken).ConfigureAwait(false);
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
                var code = opened.State == RecoveryBundleDeletionSessionOpenState.Cancelled
                    ? CleanupFindingCode.Interrupted
                    : CleanupFindingCode.CatalogueChangedDuringApply;
                result.AddFinding(code, opened.Cause ?? "Cleanup was interrupted while validating the frozen catalogue.");
                return;
            }

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
                result.Findings = progress.Findings;
            }

            result.Verification = new CleanupVerification
            {
                State = ReadVerification(progress.Effects),
                Cause = progress.Findings.FirstOrDefault()?.Cause,
            };
        }
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
