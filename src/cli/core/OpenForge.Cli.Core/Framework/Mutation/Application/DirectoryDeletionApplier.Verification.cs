using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryDeletionApplier
{
    private async ValueTask<DirectoryDeletionReceipt> VerifyAsync(
        ApplicationContext context)
    {
        var result = await ObserveMissingAsync(context).ConfigureAwait(false);
        if (result.State == FileExpectationValidationState.Matched
            && result.Actual is { } missing)
        {
            return VerifiedRemoved(context, missing);
        }

        return CompletionUnknown(
            context,
            result.Actual,
            result.Cause
                ?? result.Failure?.DirectCause
                ?? "The deleted directory state could not be verified.");
    }

    private async ValueTask<DirectoryDeletionReceipt> ResolveEffectFailureAsync(
        ApplicationContext context,
        Exception effectException)
    {
        var missing = await ObserveMissingAsync(context).ConfigureAwait(false);
        if (missing.State == FileExpectationValidationState.Matched
            && missing.Actual is { } removed)
        {
            return VerifiedRemoved(context, removed);
        }

        var retained = await ObserveDirectoryAsync(context).ConfigureAwait(false);
        if (retained.State == FileExpectationValidationState.Matched
            && retained.Actual is { } directory)
        {
            return NotStarted(
                context,
                directory,
                FilesystemNotStartedReason.ApplicationFailed,
                DirectCause(effectException));
        }

        return CompletionUnknown(
            context,
            retained.Actual ?? missing.Actual,
            retained.Cause
                ?? retained.Failure?.DirectCause
                ?? missing.Cause
                ?? missing.Failure?.DirectCause
                ?? DirectCause(effectException));
    }

    private ValueTask<FileExpectationValidationResult> ObserveMissingAsync(
        ApplicationContext context)
        => _validator.ValidateAsync(
            context.Lease.Request.Workspace,
            FileExpectation.Missing(context.Deletion.LogicalPath),
            CancellationToken.None);

    private ValueTask<FileExpectationValidationResult> ObserveDirectoryAsync(
        ApplicationContext context)
        => _validator.ValidateAsync(
            context.Lease.Request.Workspace,
            context.Deletion.Expectation,
            CancellationToken.None);
}
