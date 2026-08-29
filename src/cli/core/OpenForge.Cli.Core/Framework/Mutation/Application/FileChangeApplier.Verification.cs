using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class FileChangeApplier
{
    private async ValueTask<FileChangeReceipt> VerifyAsync(
        ApplicationContext context,
        string? physicalPath)
    {
        FileExpectation expectedAfter;
        if (context.Change.Kind == PlannedFileChangeKind.Delete)
        {
            expectedAfter = FileExpectation.Missing(context.Change.LogicalPath);
        }
        else
        {
            expectedAfter = FileExpectation.File(
                context.Change.LogicalPath,
                physicalPath
                    ?? throw new InvalidOperationException(
                        "A file replacement requires a verified physical target."),
                FileExpectation.Hash(context.Change.IntendedBytes.AsSpan()));
        }

        FileExpectationValidationResult result;
        try
        {
            result = await _validator.ValidateAsync(
                context.Lease.Request.Workspace,
                expectedAfter,
                CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return FileChangeReceipt.VerificationUnavailable(
                context.Change,
                context.Before,
                FilesystemFailure.FromException(
                    FailureKind(exception),
                    exception).DirectCause);
        }

        if (result.State == FileExpectationValidationState.Matched
            && result.Actual is { } actual)
        {
            return FileChangeReceipt.Verified(context.Change, context.Before, actual);
        }

        if (result.Actual is { } observed)
        {
            return FileChangeReceipt.VerificationFailed(
                context.Change,
                context.Before,
                observed,
                result.Cause ?? "The applied file state did not match the intended state.");
        }

        return FileChangeReceipt.VerificationUnavailable(
            context.Change,
            context.Before,
            result.Cause
                ?? result.Failure?.DirectCause
                ?? "The applied file state could not be observed.");
    }

    private async ValueTask<FileChangeReceipt> ResolveEffectFailureAsync(
        ApplicationContext context,
        string? physicalPath,
        Exception effectException)
    {
        var result = await ObserveAfterAsync(context, physicalPath).ConfigureAwait(false);
        if (result.State == FileExpectationValidationState.Matched
            && result.Actual is { } intended)
        {
            return FileChangeReceipt.Verified(
                context.Change,
                context.Before,
                intended);
        }

        if (result.Actual is { } observed)
        {
            if (observed.Expectation == context.Before.Expectation)
            {
                return FileChangeReceipt.NotStarted(
                    context.Change,
                    context.Before,
                    FileChangeNotStartedReason.ApplicationFailed,
                    FilesystemFailure.FromException(
                        FailureKind(effectException),
                        effectException).DirectCause);
            }

            return FileChangeReceipt.CompletionUnknown(
                context.Change,
                context.Before,
                observed,
                result.Cause
                    ?? FilesystemFailure.FromException(
                        FailureKind(effectException),
                        effectException).DirectCause);
        }

        return FileChangeReceipt.CompletionUnknown(
            context.Change,
            context.Before,
            after: null,
            result.Cause
                ?? result.Failure?.DirectCause
                ?? FilesystemFailure.FromException(
                    FailureKind(effectException),
                    effectException).DirectCause);
    }

    private async ValueTask<FileExpectationValidationResult> ObserveAfterAsync(
        ApplicationContext context,
        string? physicalPath)
    {
        var expectedAfter = context.Change.Kind == PlannedFileChangeKind.Delete
            ? FileExpectation.Missing(context.Change.LogicalPath)
            : FileExpectation.File(
                context.Change.LogicalPath,
                physicalPath
                    ?? context.Change.Expectation.PhysicalPath
                    ?? throw new InvalidOperationException(
                        "A replacement failure requires its expected physical target."),
                FileExpectation.Hash(context.Change.IntendedBytes.AsSpan()));
        try
        {
            return await _validator.ValidateAsync(
                context.Lease.Request.Workspace,
                expectedAfter,
                CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return FileExpectationValidationResult.Failed(
                expectedAfter,
                FilesystemFailure.FromException(
                    FailureKind(exception),
                    exception));
        }
    }
}
