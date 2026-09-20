using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class DirectoryCreationApplier
{
    private async ValueTask<DirectoryCreationReceipt> VerifyAsync(
        ApplicationContext context)
    {
        var result = await ObserveAfterAsync(context).ConfigureAwait(false);
        if (result.State == FileExpectationValidationState.Matched
            && result.Actual is { } actual)
        {
            return IsOrdinaryDirectory(context.IntendedPhysicalPath)
                ? DirectoryCreationReceipt.Verified(
                    context.Creation,
                    context.Before,
                    context.IntendedPhysicalPath,
                    actual)
                : DirectoryCreationReceipt.VerificationUnavailable(
                    context.Creation,
                    context.Before,
                    context.IntendedPhysicalPath,
                    "The created target did not remain an ordinary physical directory.");
        }

        if (result.Actual is { } observed)
        {
            return DirectoryCreationReceipt.VerificationFailed(
                context.Creation,
                context.Before,
                context.IntendedPhysicalPath,
                observed,
                result.Cause
                    ?? "The created directory did not match its intended physical state.");
        }

        return DirectoryCreationReceipt.VerificationUnavailable(
            context.Creation,
            context.Before,
            context.IntendedPhysicalPath,
            result.Cause
                ?? result.Failure?.DirectCause
                ?? "The created directory state could not be verified.");
    }

    private async ValueTask<DirectoryCreationReceipt> ResolveEffectFailureAsync(
        ApplicationContext context,
        Exception effectException)
    {
        var result = await ObserveAfterAsync(context).ConfigureAwait(false);
        if (result.State == FileExpectationValidationState.Matched
            && result.Actual is { } intended)
        {
            return IsOrdinaryDirectory(context.IntendedPhysicalPath)
                ? DirectoryCreationReceipt.Verified(
                    context.Creation,
                    context.Before,
                    context.IntendedPhysicalPath,
                    intended)
                : DirectoryCreationReceipt.CompletionUnknown(
                    context.Creation,
                    context.Before,
                    context.IntendedPhysicalPath,
                    after: null,
                    "The directory effect completed with an unavailable ordinary-target identity.");
        }

        var effectCause = FilesystemFailure.FromException(
            FilesystemFailure.ClassifyException(effectException),
            effectException).DirectCause;
        if (result.Actual is { } observed)
        {
            if (observed.Expectation == context.Before.Expectation)
            {
                return NotStarted(
                    context,
                    FilesystemNotStartedReason.ApplicationFailed,
                    effectCause);
            }

            return DirectoryCreationReceipt.CompletionUnknown(
                context.Creation,
                context.Before,
                context.IntendedPhysicalPath,
                observed,
                result.Cause ?? effectCause);
        }

        return DirectoryCreationReceipt.CompletionUnknown(
            context.Creation,
            context.Before,
            context.IntendedPhysicalPath,
            after: null,
            result.Cause
                ?? result.Failure?.DirectCause
                ?? effectCause);
    }

    private async ValueTask<FileExpectationValidationResult> ObserveAfterAsync(
        ApplicationContext context)
    {
        var expectedAfter = FileExpectation.Directory(
            context.Creation.LogicalPath,
            context.IntendedPhysicalPath);
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
                    FilesystemFailure.ClassifyException(exception),
                    exception));
        }
    }
}
