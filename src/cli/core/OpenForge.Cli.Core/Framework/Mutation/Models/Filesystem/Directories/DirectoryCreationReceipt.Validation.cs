using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;

internal sealed partial record DirectoryCreationReceipt
{
    private const int MaximumCauseLength = 256;

    private static void ValidateBefore(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before)
    {
        ArgumentNullException.ThrowIfNull(creation);
        ArgumentNullException.ThrowIfNull(before);
        if (before.Expectation != creation.Expectation)
        {
            throw new ArgumentException(
                "A directory receipt before-state must equal the planned missing expectation.",
                nameof(before));
        }
    }

    private static void ValidateVerifiedAfter(
        PlannedDirectoryCreation creation,
        string intendedPhysicalPath,
        FileStateSnapshot after)
    {
        if (!MatchesIntendedAfter(creation, intendedPhysicalPath, after))
        {
            throw new ArgumentException(
                "A verified directory creation requires the intended ordinary directory after-state.",
                nameof(after));
        }
    }

    private static void ValidateVerificationFailedAfter(
        PlannedDirectoryCreation creation,
        string intendedPhysicalPath,
        FileStateSnapshot after)
    {
        if (MatchesIntendedAfter(creation, intendedPhysicalPath, after))
        {
            throw new ArgumentException(
                "A verification-failed directory receipt cannot carry the intended after-state.",
                nameof(after));
        }
    }

    private static void ValidateCompletionUnknownAfter(
        PlannedDirectoryCreation creation,
        FileStateSnapshot before,
        string intendedPhysicalPath,
        FileStateSnapshot? after)
    {
        if (after is null)
        {
            return;
        }

        if (MatchesIntendedAfter(creation, intendedPhysicalPath, after))
        {
            throw new ArgumentException(
                "An unknown-completion directory receipt cannot carry the intended after-state.",
                nameof(after));
        }

        if (after.Expectation == before.Expectation)
        {
            throw new ArgumentException(
                "An unknown-completion directory receipt cannot carry the unchanged before-state.",
                nameof(after));
        }
    }

    private static bool MatchesIntendedAfter(
        PlannedDirectoryCreation creation,
        string intendedPhysicalPath,
        FileStateSnapshot after)
    {
        ValidateAfterTarget(creation, after);
        return after.Kind == FileExpectationKind.Directory
            && after.PhysicalPath is { } afterPhysicalPath
            && PhysicalIdentityTracker.PathComparer.Equals(
                ValidateIntendedPhysicalPath(intendedPhysicalPath),
                afterPhysicalPath);
    }

    private static void ValidateAfterTarget(
        PlannedDirectoryCreation creation,
        FileStateSnapshot after)
    {
        ArgumentNullException.ThrowIfNull(after);
        if (!string.Equals(
            creation.LogicalPath,
            after.LogicalPath,
            StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A directory receipt after-state must describe the planned logical target.",
                nameof(after));
        }
    }

    private static string ValidateIntendedPhysicalPath(string intendedPhysicalPath)
        => FileExpectation.NormalizeAbsolutePath(
            intendedPhysicalPath,
            nameof(intendedPhysicalPath));

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    private static void ValidateNotStartedReason(FilesystemNotStartedReason reason)
    {
        _ = reason switch
        {
            FilesystemNotStartedReason.Cancelled
                or FilesystemNotStartedReason.TargetChanged
                or FilesystemNotStartedReason.ApplicationFailed
                or FilesystemNotStartedReason.ContractRejected => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(reason),
                reason,
                "The filesystem not-started reason is not defined."),
        };
    }
}
