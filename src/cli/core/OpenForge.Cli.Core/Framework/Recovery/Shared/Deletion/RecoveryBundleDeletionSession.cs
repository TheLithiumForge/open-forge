using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;

internal sealed class RecoveryBundleDeletionSession
{
    private readonly WorkspaceLockLease _lease;
    private readonly ImmutableArray<RecoveryBundleCandidateSnapshot> _candidates;
    private readonly HashSet<string> _removed = new(StringComparer.Ordinal);

    private RecoveryBundleDeletionSession(
        WorkspaceLockLease lease,
        ImmutableArray<RecoveryBundleCandidateSnapshot> candidates)
    {
        _lease = lease;
        _candidates = candidates;
    }

    internal static async ValueTask<RecoveryBundleDeletionSessionOpenResult> OpenAsync(
        WorkspaceLockLease lease,
        RecoveryBundleCatalogueResult frozenCatalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(frozenCatalogue);
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Blocked, "A deletion session requires a held workspace lease.");
        }

        var storageFailure = RecoveryDeletionStorageBoundary.ReadWorkspace(lease.Request.Workspace);
        if (storageFailure is not null)
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Blocked, storageFailure.DirectCause, failure: storageFailure);
        }

        if (frozenCatalogue.State != RecoveryBundleCatalogueState.Available
            || frozenCatalogue.Candidates.IsDefault
            || frozenCatalogue.Candidates.Any(candidate => !IsAdmitted(candidate, lease)))
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Blocked, "A deletion session requires a complete validated frozen catalogue.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Cancelled);
        }

        var observed = await RecoveryBundleCatalogue.ReadAsync(lease.Request.Workspace, cancellationToken).ConfigureAwait(false);
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Blocked, "The workspace lease was released during session opening.", observed);
        }

        storageFailure = RecoveryDeletionStorageBoundary.ReadWorkspace(lease.Request.Workspace);
        if (storageFailure is not null)
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Blocked, storageFailure.DirectCause, failure: storageFailure);
        }

        if (observed.State == RecoveryBundleCatalogueState.Cancelled)
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Cancelled, observed: observed);
        }

        if (observed.State != RecoveryBundleCatalogueState.Available)
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Unavailable, observed.Cause, observed);
        }

        if (!SameCatalogue(frozenCatalogue.Candidates, observed.Candidates))
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Changed, "The exact recovery catalogue changed before deletion.", observed);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Closed(RecoveryBundleDeletionSessionOpenState.Cancelled, observed: observed);
        }

        return new RecoveryBundleDeletionSessionOpenResult
        {
            State = RecoveryBundleDeletionSessionOpenState.Opened,
            ObservedCatalogue = observed,
            Session = new RecoveryBundleDeletionSession(lease, frozenCatalogue.Candidates),
            Failure = null,
            Cause = null,
        };
    }

    internal async ValueTask<RecoveryBundleDeletionResult> DeleteAsync(
        RecoveryBundleCandidateSnapshot candidate,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        if (!_lease.IsHeldFor(_lease.Request.Workspace)
            || !IsAdmitted(candidate, _lease)
            || _removed.Contains(candidate.Path)
            || !_candidates.Any(expected => SameCandidate(expected, candidate)))
        {
            return RecoveryBundleDeletionResult.BlockedUnknown("Deletion requires an unchanged session member and its live workspace lease.");
        }

        var storageFailure = RecoveryDeletionStorageBoundary.ReadWorkspace(_lease.Request.Workspace);
        if (storageFailure is not null)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(storageFailure.DirectCause, candidate.Path);
        }

        var cause = ReadFileBoundaryFailure(candidate);
        if (cause is not null)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(cause, candidate.Path);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleDeletionResult.CancelledRetained(candidate.Path);
        }

        if (candidate.Kind == RecoveryBundleCandidateKind.Final)
        {
            var read = await RecoveryBundleReader.ReadFinalAsync(_lease.Request.Workspace, candidate.Path, cancellationToken).ConfigureAwait(false);
            if (!_lease.IsHeldFor(_lease.Request.Workspace))
            {
                return RecoveryBundleDeletionResult.BlockedUnknown("The workspace lease was released during semantic validation.", candidate.Path);
            }

            storageFailure = RecoveryDeletionStorageBoundary.ReadWorkspace(_lease.Request.Workspace);
            if (storageFailure is not null)
            {
                return RecoveryBundleDeletionResult.BlockedUnknown(storageFailure.DirectCause, candidate.Path);
            }

            if (read.State == RecoveryBundleReadState.Cancelled)
            {
                return RecoveryBundleDeletionResult.CancelledRetained(candidate.Path);
            }

            if (read.State != RecoveryBundleReadState.Valid
                || read.Verified is not { } verified
                || !SameCandidate(candidate, RecoveryBundleCandidateSnapshot.VerifiedFinal(verified)))
            {
                return RecoveryBundleDeletionResult.BlockedUnknown(read.Cause ?? "The final bundle's semantic facts changed during deletion.", candidate.Path);
            }
        }

        if (!_lease.IsHeldFor(_lease.Request.Workspace))
        {
            return RecoveryBundleDeletionResult.BlockedUnknown("The workspace lease was released before deletion.", candidate.Path);
        }

        storageFailure = RecoveryDeletionStorageBoundary.ReadWorkspace(_lease.Request.Workspace);
        if (storageFailure is not null)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(storageFailure.DirectCause, candidate.Path);
        }

        cause = ReadFileBoundaryFailure(candidate);
        if (cause is not null)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(cause, candidate.Path);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return RecoveryBundleDeletionResult.CancelledRetained(candidate.Path);
        }

        var deletion = RecoveryBundleDeletionValidation.DeleteOrdinaryFile(candidate);
        if (deletion.State == RecoveryBundleDeletionState.Deleted)
        {
            _removed.Add(candidate.Path);
        }

        return deletion;
    }

    private static bool IsAdmitted(RecoveryBundleCandidateSnapshot candidate, WorkspaceLockLease lease)
        => RecoveryBundleDeletionValidation.BelongsToWorkspace(candidate, lease)
            && RecoveryBundleFormatV1.TryParseCandidateFileName(Path.GetFileName(candidate.Path), out _, out var kind)
            && candidate.Kind == kind
            && (candidate.Kind, candidate.Integrity) is
                (RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Verified)
                or (RecoveryBundleCandidateKind.Draft, RecoveryBundleIntegrity.Incomplete);

    private static bool SameCatalogue(
        ImmutableArray<RecoveryBundleCandidateSnapshot> expected,
        ImmutableArray<RecoveryBundleCandidateSnapshot> observed)
    {
        if (expected.Length != observed.Length)
        {
            return false;
        }

        for (var index = 0; index < expected.Length; index++)
        {
            if (!SameCandidate(expected[index], observed[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameCandidate(RecoveryBundleCandidateSnapshot expected, RecoveryBundleCandidateSnapshot actual)
        => RecoveryBundleDeletionValidation.MatchesExactSnapshot(expected, actual)
            && string.Equals(expected.Verified?.WorkspacePhysicalPath, actual.Verified?.WorkspacePhysicalPath, RecoveryBundleDeletionValidation.PathComparison());

    private static string? ReadFileBoundaryFailure(RecoveryBundleCandidateSnapshot candidate)
    {
        if (!RecoveryBundleStorage.TryObservePath(candidate.Path, out var attributes, out var failure))
        {
            return failure?.DirectCause ?? "The exact recovery candidate cannot be observed.";
        }

        if (attributes is null)
        {
            return "The exact recovery candidate disappeared before deletion.";
        }

        return (attributes.Value & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0
            ? null
            : "The exact recovery candidate is no longer an ordinary file.";
    }

    private static RecoveryBundleDeletionSessionOpenResult Closed(
        RecoveryBundleDeletionSessionOpenState state,
        string? cause = null,
        RecoveryBundleCatalogueResult? observed = null,
        FilesystemFailure? failure = null)
        => new()
        {
            State = state,
            ObservedCatalogue = observed,
            Session = null,
            Failure = failure ?? observed?.Failure,
            Cause = cause,
        };
}
