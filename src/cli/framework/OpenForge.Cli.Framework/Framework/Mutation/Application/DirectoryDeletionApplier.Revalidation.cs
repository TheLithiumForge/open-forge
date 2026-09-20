using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryDeletionApplier
{
    private async ValueTask<DirectoryDeletionReceipt?> RevalidateBeforeEffectAsync(
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        MutationValidationResult result;
        try
        {
            result = await _revalidator.ValidateAsync(
                context.Lease,
                context.Deletion,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(context);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return NotStarted(
                context,
                FilesystemNotStartedReason.ApplicationFailed,
                DirectCause(exception));
        }

        if (result.State != MutationValidationState.Valid)
        {
            return NotStarted(
                context,
                ReadNotStartedReason(result.State),
                result.Cause
                    ?? result.Failure?.DirectCause
                    ?? ReadNotStartedCause(result.State));
        }

        if (result.Checks.Count != 1
            || !IsSameMatchedDirectoryCheck(context, result.Checks[0]))
        {
            return NotStarted(
                context,
                FilesystemNotStartedReason.TargetChanged,
                "The target identity changed before its directory deletion effect.");
        }

        return null;
    }

    private static DirectoryDeletionReceipt? InspectEmptiness(
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            using var entries = Directory.EnumerateFileSystemEntries(context.PhysicalPath)
                .GetEnumerator();
            if (cancellationToken.IsCancellationRequested)
            {
                return Cancelled(context);
            }

            return entries.MoveNext()
                ? NotStarted(
                    context,
                    FilesystemNotStartedReason.TargetChanged,
                    "The directory is no longer empty before its deletion effect.")
                : null;
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return NotStarted(
                context,
                FilesystemNotStartedReason.ApplicationFailed,
                DirectCause(exception));
        }
    }
}
