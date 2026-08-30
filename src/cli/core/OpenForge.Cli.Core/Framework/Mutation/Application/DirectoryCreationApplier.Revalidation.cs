using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryCreationApplier
{
    private async ValueTask<DirectoryCreationReceipt?> RevalidateBeforeEffectAsync(
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        MutationValidationResult result;
        try
        {
            result = await _revalidator.ValidateAsync(
                context.Lease,
                directoryCreations: [context.Creation],
                fileChanges: [],
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
                FilesystemFailure.FromException(
                    FailureKind(exception),
                    exception).DirectCause);
        }

        if (result.State != MutationValidationState.Valid)
        {
            return NotStarted(
                context,
                ReadNotStartedReason(result.State),
                ReadNotStartedCause(result));
        }

        if (result.Checks.Count != 1)
        {
            return NotStarted(
                context,
                FilesystemNotStartedReason.ContractRejected,
                "Directory creation revalidation did not return its one expected target check.");
        }

        var current = result.Checks[0];
        if (!IsSameMatchedMissingCheck(context, current))
        {
            return NotStarted(
                context,
                FilesystemNotStartedReason.TargetChanged,
                "The target identity changed before its directory creation effect.");
        }

        return cancellationToken.IsCancellationRequested
            ? Cancelled(context)
            : null;
    }

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
                "A valid mutation revalidation has no not-started reason."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The mutation validation state is not defined."),
        };

    private static string ReadNotStartedCause(MutationValidationResult result)
    {
        if (result.Cause is not null)
        {
            return result.Cause;
        }

        return result.State switch
        {
            MutationValidationState.Mismatched =>
                "The target changed before its directory creation effect.",
            MutationValidationState.Blocked =>
                "Directory creation revalidation rejected the target safety or contract boundary.",
            MutationValidationState.Failed =>
                "The target state could not be revalidated before directory creation.",
            MutationValidationState.Cancelled =>
                "Directory creation revalidation was cancelled before its target effect.",
            MutationValidationState.Valid => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "A valid mutation revalidation has no not-started cause."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(result),
                result.State,
                "The mutation validation state is not defined."),
        };
    }

    private static DirectoryCreationReceipt Cancelled(ApplicationContext context)
        => NotStarted(
            context,
            FilesystemNotStartedReason.Cancelled,
            CancellationBeforeEffectCause);

    private static DirectoryCreationReceipt NotStarted(
        ApplicationContext context,
        FilesystemNotStartedReason reason,
        string cause)
        => DirectoryCreationReceipt.NotStarted(
            context.Creation,
            context.Before,
            context.IntendedPhysicalPath,
            reason,
            cause);
}
