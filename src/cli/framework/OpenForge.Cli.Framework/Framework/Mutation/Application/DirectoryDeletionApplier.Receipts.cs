using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryDeletionApplier
{
    private static DirectoryDeletionReceipt Cancelled(ApplicationContext context)
        => NotStarted(
            context,
            FilesystemNotStartedReason.Cancelled,
            CancellationBeforeEffectCause);

    private static DirectoryDeletionReceipt NotStarted(
        ApplicationContext context,
        FilesystemNotStartedReason reason,
        string cause)
        => NotStarted(context, context.Before, reason, cause);

    private static DirectoryDeletionReceipt NotStarted(
        ApplicationContext context,
        FileStateSnapshot after,
        FilesystemNotStartedReason reason,
        string cause)
        => new(
            context.Deletion,
            context.Before,
            after,
            new DirectoryDeletionReceiptState(
                FilesystemEffectState.NotStarted,
                FilesystemVerificationState.NotStarted,
                reason,
                DirectoryDeletionDisposition.Retained),
            cause);

    private static DirectoryDeletionReceipt VerifiedRemoved(
        ApplicationContext context,
        FileStateSnapshot after)
        => new(
            context.Deletion,
            context.Before,
            after,
            new DirectoryDeletionReceiptState(
                FilesystemEffectState.Applied,
                FilesystemVerificationState.Verified,
                notStartedReason: null,
                DirectoryDeletionDisposition.Removed),
            cause: null);

    private static DirectoryDeletionReceipt CompletionUnknown(
        ApplicationContext context,
        FileStateSnapshot? after,
        string cause)
        => new(
            context.Deletion,
            context.Before,
            after,
            new DirectoryDeletionReceiptState(
                FilesystemEffectState.Unknown,
                FilesystemVerificationState.Failed,
                notStartedReason: null,
                ReadDisposition(after)),
            cause);

    private static FilesystemNotStartedReason ReadNotStartedReason(
        MutationValidationState state)
        => state switch
        {
            MutationValidationState.Mismatched => FilesystemNotStartedReason.TargetChanged,
            MutationValidationState.Blocked => FilesystemNotStartedReason.ContractRejected,
            MutationValidationState.Failed => FilesystemNotStartedReason.ApplicationFailed,
            MutationValidationState.Cancelled => FilesystemNotStartedReason.Cancelled,
            MutationValidationState.Valid => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A valid deletion revalidation has no not-started reason."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The mutation validation state is not defined."),
        };

    private static string ReadNotStartedCause(MutationValidationState state)
        => state switch
        {
            MutationValidationState.Mismatched =>
                "The target changed before its directory deletion effect.",
            MutationValidationState.Blocked =>
                "Directory deletion revalidation rejected the target contract.",
            MutationValidationState.Failed =>
                "The directory state could not be revalidated before deletion.",
            MutationValidationState.Cancelled => CancellationBeforeEffectCause,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The mutation validation state has no not-started cause."),
        };

    private static DirectoryDeletionDisposition ReadDisposition(FileStateSnapshot? snapshot)
        => snapshot?.Kind switch
        {
            FileExpectationKind.Missing => DirectoryDeletionDisposition.Removed,
            FileExpectationKind.Directory => DirectoryDeletionDisposition.Retained,
            _ => DirectoryDeletionDisposition.Unknown,
        };

    private static bool IsFilesystemException(Exception exception)
        => exception is UnauthorizedAccessException
            or IOException
            or NotSupportedException
            or PlatformNotSupportedException
            or ArgumentException
            or PathTooLongException;

    private static string DirectCause(Exception exception)
        => FilesystemFailure.FromException(
            FilesystemFailure.ClassifyException(exception),
            exception).DirectCause;
}
