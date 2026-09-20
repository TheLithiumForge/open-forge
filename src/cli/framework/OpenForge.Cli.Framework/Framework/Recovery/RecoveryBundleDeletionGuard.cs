using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal static class RecoveryBundleDeletionGuard
{
    internal static ValueTask<RecoveryBundleDeletionSessionOpenResult> OpenSessionAsync(
        WorkspaceLockLease lease,
        RecoveryBundleCatalogueResult frozenCatalogue,
        CancellationToken cancellationToken)
        => RecoveryBundleDeletionSession.OpenAsync(lease, frozenCatalogue, cancellationToken);

    internal static async ValueTask<RecoveryBundleDeletionResult> DeleteAsync(
        WorkspaceLockLease lease,
        RecoveryBundleCandidateSnapshot candidate,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(candidate);
        if (!lease.IsHeld)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "Recovery deletion requires a held workspace lock lease.");
        }

        if (!RecoveryBundleDeletionValidation.BelongsToWorkspace(candidate, lease))
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "Recovery deletion requires an exact candidate from the held lease workspace.");
        }

        if (candidate.Kind == RecoveryBundleCandidateKind.Final
            && candidate.Integrity != RecoveryBundleIntegrity.Verified)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "Recovery deletion requires semantic validation for a final bundle.");
        }

        if (candidate.Kind == RecoveryBundleCandidateKind.Draft
            && candidate.Integrity != RecoveryBundleIntegrity.Incomplete)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "Recovery deletion requires an exact path-only incomplete draft.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleDeletionResult.CancelledUnknown();
        }

        var catalogue = await RecoveryBundleCatalogue.ReadAsync(
            lease.Request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (catalogue.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return RecoveryBundleDeletionResult.CancelledUnknown();
        }

        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                catalogue.Cause ?? "The recovery catalogue could not be re-enumerated under the lease.");
        }

        var current = catalogue.Candidates.SingleOrDefault(item => string.Equals(
            item.Path,
            candidate.Path,
            RecoveryBundleDeletionValidation.PathComparison()));
        if (current is null)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "The selected recovery candidate changed before deletion.");
        }

        if (!RecoveryBundleDeletionValidation.MatchesExactSnapshot(candidate, current))
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "The selected recovery candidate changed before deletion.",
                candidate.Path);
        }

        if (candidate.Kind == RecoveryBundleCandidateKind.Final)
        {
            var finalRead = await RecoveryBundleReader.ReadFinalAsync(
                lease.Request.Workspace,
                candidate.Path,
                cancellationToken).ConfigureAwait(false);
            if (finalRead.State == RecoveryBundleReadState.Cancelled)
            {
                return RecoveryBundleDeletionResult.CancelledRetained(candidate.Path);
            }

            if (finalRead.State != RecoveryBundleReadState.Valid
                || finalRead.Verified is not { } verified
                || !RecoveryBundleDeletionValidation.MatchesExactSnapshot(
                    candidate,
                    RecoveryBundleCandidateSnapshot.VerifiedFinal(verified)))
            {
                return RecoveryBundleDeletionResult.BlockedUnknown(
                    finalRead.Cause ?? "The selected final bundle failed semantic revalidation.",
                    candidate.Path);
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleDeletionResult.CancelledRetained(candidate.Path);
        }

        return RecoveryBundleDeletionValidation.DeleteOrdinaryFile(candidate);
    }
}
