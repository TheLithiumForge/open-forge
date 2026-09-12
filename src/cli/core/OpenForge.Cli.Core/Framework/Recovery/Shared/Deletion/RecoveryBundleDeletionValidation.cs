using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;

internal static class RecoveryBundleDeletionValidation
{
    internal static RecoveryBundleDeletionResult DeleteOrdinaryFile(RecoveryBundleCandidateSnapshot candidate)
    {
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

    internal static bool BelongsToWorkspace(
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

    internal static bool MatchesExactSnapshot(
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
            && expectedVerified.Attribution == actualVerified.Attribution
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

    internal static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
