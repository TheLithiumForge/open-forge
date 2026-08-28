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
                recoveryCause);
        }

        if (!TryValidateLeaseAndTarget(context, out var cause))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                cause);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                "File application was cancelled before its target effect.");
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
                "File application was cancelled before its target effect.");
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
                "File application was cancelled before its target effect.");
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
                "File application was cancelled before its target effect.");
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FilesystemFailure.FromException(
                    FailureKind(exception),
                    exception).DirectCause);
        }

        if (result.State != MutationValidationState.Valid
            || result.Checks.Count != 1)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                result.Cause ?? "The target changed before its file effect.");
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
                "The target identity changed before its file effect.");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                "File application was cancelled before its target effect.");
        }

        return null;
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
