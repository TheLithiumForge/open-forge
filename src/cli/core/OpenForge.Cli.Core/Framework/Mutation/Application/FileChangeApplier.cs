using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class FileChangeApplier(
    MutationRevalidator revalidator,
    FileExpectationValidator validator)
{
    private const string CancellationBeforeEffectCause =
        "File application was cancelled before its target effect.";

    private readonly MutationRevalidator _revalidator = revalidator;
    private readonly FileExpectationValidator _validator = validator;

    internal async ValueTask<FileChangeReceipt> ApplyAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileExpectationValidationResult check,
        RecoveryBundlePreparation? recoveryPreparation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(change);
        ArgumentNullException.ThrowIfNull(check);

        var context = new ApplicationContext(
            lease,
            change,
            check,
            ValidateMatchedCheck(change, check));
        if (!TryValidateRecoveryPreparation(
            lease,
            change,
            recoveryPreparation,
            out var recoveryCause))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.ContractRejected,
                recoveryCause);
        }

        if (!TryValidateLeaseAndTarget(context, out var cause))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.ContractRejected,
                cause);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.Cancelled,
                CancellationBeforeEffectCause);
        }

        if (change.Kind != PlannedFileChangeKind.Delete)
        {
            var preStageRevalidation = await RevalidateBeforeEffectAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            if (preStageRevalidation is not null)
            {
                return preStageRevalidation;
            }
        }

        return change.Kind == PlannedFileChangeKind.Delete
            ? await ApplyDeleteAsync(context, cancellationToken).ConfigureAwait(false)
            : await ApplyWriteAsync(context, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<FileChangeReceipt> ApplyDeleteAsync(
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        var revalidation = await RevalidateBeforeEffectAsync(
            context,
            cancellationToken).ConfigureAwait(false);
        if (revalidation is not null)
        {
            return revalidation;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.Cancelled,
                CancellationBeforeEffectCause);
        }

        try
        {
            File.Delete(context.Check.PhysicalPath
                ?? throw new InvalidOperationException("A matched file check requires a physical target."));
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return await ResolveEffectFailureAsync(
                context,
                context.Check.PhysicalPath,
                exception).ConfigureAwait(false);
        }

        return await VerifyAsync(
            context,
            context.Check.PhysicalPath).ConfigureAwait(false);
    }

    private async ValueTask<FileChangeReceipt?> RevalidateBeforeEffectAsync(
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.Cancelled,
                CancellationBeforeEffectCause);
        }

        MutationValidationResult result;
        try
        {
            result = await _revalidator.ValidateAsync(
                context.Lease,
                [context.Change],
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.Cancelled,
                CancellationBeforeEffectCause);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.ApplicationFailed,
                FilesystemFailure.FromException(
                    FailureKind(exception),
                    exception).DirectCause);
        }

        if (result.State != MutationValidationState.Valid)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                ReadNotStartedReason(result.State),
                ReadNotStartedCause(result));
        }

        if (result.Checks.Count != 1)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.ContractRejected,
                "File application revalidation did not return its one expected target check.");
        }

        var current = result.Checks[0];
        if (current.State != FileExpectationValidationState.Matched
            || current.Actual is null
            || current.Expectation != context.Change.Expectation
            || current.Actual.Expectation != context.Change.Expectation
            || !string.Equals(
                current.PhysicalPath,
                context.Check.PhysicalPath,
                PathComparison()))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.TargetChanged,
                "The target identity changed before its file effect.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemNotStartedReason.Cancelled,
                CancellationBeforeEffectCause);
        }

        return null;
    }

    private static FilesystemNotStartedReason ReadNotStartedReason(MutationValidationState state)
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
            MutationValidationState.Mismatched => "The target changed before its file effect.",
            MutationValidationState.Blocked => "File application revalidation rejected the target safety or contract boundary.",
            MutationValidationState.Failed => "The target state could not be revalidated before file application.",
            MutationValidationState.Cancelled => "File application revalidation was cancelled before its target effect.",
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

    private sealed record ApplicationContext(
        WorkspaceLockLease Lease,
        PlannedFileChange Change,
        FileExpectationValidationResult Check,
        FileStateSnapshot Before);

    private static bool TryValidateRecoveryPreparation(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        RecoveryBundlePreparation? preparation,
        out string cause)
    {
        cause = string.Empty;
        if (change.Kind == PlannedFileChangeKind.Create)
        {
            if (preparation is null)
            {
                return true;
            }

            cause = "Create file application must not receive recovery bundle preparation.";
            return false;
        }

        if (preparation is null)
        {
            cause = "Existing-target file application requires a verified recovery bundle preparation.";
            return false;
        }

        if (!preparation.MatchesChange(lease.Request, change))
        {
            cause = "The recovery bundle preparation does not match this operation and target change.";
            return false;
        }

        return true;
    }

}
