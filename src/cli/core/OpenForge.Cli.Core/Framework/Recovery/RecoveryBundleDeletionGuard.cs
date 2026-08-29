using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal sealed class RecoveryBundleDeletionGuard(
    RecoveryBundleCatalogue catalogue,
    RecoveryBundleReader reader)
{
    private readonly RecoveryBundleCatalogue _catalogue = catalogue;
    private readonly RecoveryBundleReader _reader = reader;

    internal async ValueTask<RecoveryBundleDeletionResult> DeleteAsync(
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

        if (!BelongsToWorkspace(candidate, lease))
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

        var catalogue = await _catalogue.ReadAsync(
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
            PathComparison()));
        if (current is null)
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "The selected recovery candidate changed before deletion.");
        }

        if (!MatchesExactSnapshot(candidate, current))
        {
            return RecoveryBundleDeletionResult.BlockedUnknown(
                "The selected recovery candidate changed before deletion.",
                candidate.Path);
        }

        if (candidate.Kind == RecoveryBundleCandidateKind.Final)
        {
            var finalRead = await _reader.ReadFinalAsync(
                lease.Request.Workspace,
                candidate.Path,
                cancellationToken).ConfigureAwait(false);
            if (finalRead.State == RecoveryBundleReadState.Cancelled)
            {
                return RecoveryBundleDeletionResult.CancelledRetained(candidate.Path);
            }

            if (finalRead.State != RecoveryBundleReadState.Valid
                || finalRead.Verified is not { } verified
                || !MatchesExactSnapshot(
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

        try
        {
            RecoveryBundleStorage.ValidateOrdinaryFile(candidate.Path);
            File.Delete(candidate.Path);
            if (!RecoveryBundleStorage.TryObservePath(
                candidate.Path,
                out var attributes,
                out var observationFailure))
            {
                var failure = observationFailure
                    ?? throw new InvalidOperationException(
                        "An unavailable recovery deletion observation requires a typed failure.");
                return RecoveryBundleDeletionResult.FailedUnknown(
                    candidate.Path,
                    $"The recovery candidate absence could not be confirmed: {failure.DirectCause}",
                    failure);
            }

            if (attributes is not null)
            {
                var failure = new FilesystemFailure(
                    FilesystemFailureKind.InputOutput,
                    "The recovery candidate remains after its delete operation.");
                return RecoveryBundleDeletionResult.FailedRetained(
                    candidate.Path,
                    failure.DirectCause,
                    failure);
            }

            return RecoveryBundleDeletionResult.Deleted();
        }
        catch (UnauthorizedAccessException exception)
        {
            return FailedUnknown(candidate.Path, FilesystemFailureKind.AccessDenied, exception);
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return FailedUnknown(candidate.Path, FilesystemFailureKind.InvalidPath, exception);
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            return FailedUnknown(candidate.Path, FilesystemFailureKind.Unsupported, exception);
        }
        catch (IOException exception)
        {
            return FailedUnknown(candidate.Path, FilesystemFailureKind.InputOutput, exception);
        }
    }

    private static bool BelongsToWorkspace(
        RecoveryBundleCandidateSnapshot candidate,
        WorkspaceLockLease lease)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return false;
        }

        var expectedDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            lease.Request.Workspace.PhysicalRoot);
        return string.Equals(
            Path.GetDirectoryName(candidate.Path),
            expectedDirectory,
            PathComparison());
    }

    private static bool MatchesExactSnapshot(
        RecoveryBundleCandidateSnapshot expected,
        RecoveryBundleCandidateSnapshot actual)
    {
        if (!string.Equals(expected.Path, actual.Path, PathComparison())
            || expected.Kind != actual.Kind
            || expected.Integrity != actual.Integrity)
        {
            return false;
        }

        if (expected.Verified is not { } expectedVerified)
        {
            return actual.Verified is null;
        }

        return actual.Verified is { } actualVerified
            && expectedVerified.OperationId == actualVerified.OperationId
            && string.Equals(expectedVerified.Command, actualVerified.Command, StringComparison.Ordinal)
            && string.Equals(expectedVerified.WorkspaceKey, actualVerified.WorkspaceKey, StringComparison.Ordinal)
            && expectedVerified.Entries.SequenceEqual(actualVerified.Entries);
    }

    private static RecoveryBundleDeletionResult FailedUnknown(
        string residualPath,
        FilesystemFailureKind kind,
        Exception exception)
    {
        var failure = FilesystemFailure.FromException(kind, exception);
        return RecoveryBundleDeletionResult.FailedUnknown(
            residualPath,
            $"The recovery candidate disposition is unknown after deletion failed: {failure.DirectCause}",
            failure);
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
