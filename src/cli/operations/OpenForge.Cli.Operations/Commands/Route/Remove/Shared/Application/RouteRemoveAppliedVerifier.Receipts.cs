using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveAppliedVerifier
{
    private async ValueTask<RouteRemoveAppliedVerification?> VerifyReceiptsAsync(
        RouteRemoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        var projection = input.Plan.Projection;
        var expectedCount = projection.FileChanges.Length
            + projection.DirectoryDeletions.Length;
        if (!input.Progress.Findings.IsEmpty || input.Progress.Receipts.Length != expectedCount)
        {
            return Failed("Final Route Remove verification requires one clean receipt per effect.");
        }

        var index = 0;
        var boundary = await VerifyFilesAsync(input, index, cancellationToken)
            .ConfigureAwait(false);
        index += projection.FileChanges.Length;
        boundary ??= await VerifyDeletionsAsync(input, index, cancellationToken)
            .ConfigureAwait(false);
        return boundary;
    }

    private async ValueTask<RouteRemoveAppliedVerification?> VerifyFilesAsync(
        RouteRemoveAppliedVerificationInput input,
        int start,
        CancellationToken cancellationToken)
    {
        var changes = input.Plan.Projection.FileChanges;
        for (var offset = 0; offset < changes.Length; offset++)
        {
            if (input.Progress.Receipts[start + offset]
                    is not RouteRemoveFileChangeReceipt applied
                || !Matches(changes[offset], applied.Receipt)
                || applied.Receipt.After is not { } after)
            {
                return Failed("A planned Route Remove file change has no exact verified receipt.");
            }

            var boundary = await VerifyExpectationAsync(input.Plan, after, cancellationToken)
                .ConfigureAwait(false);
            if (boundary is not null)
            {
                return boundary;
            }
        }

        return null;
    }

    private async ValueTask<RouteRemoveAppliedVerification?> VerifyDeletionsAsync(
        RouteRemoveAppliedVerificationInput input,
        int start,
        CancellationToken cancellationToken)
    {
        var deletions = input.Plan.Projection.DirectoryDeletions;
        for (var offset = 0; offset < deletions.Length; offset++)
        {
            if (input.Progress.Receipts[start + offset]
                    is not RouteRemoveDirectoryDeletionReceipt applied
                || !Matches(deletions[offset], applied.Receipt)
                || applied.Receipt.After is not { } after)
            {
                return Failed("A planned Route Remove directory deletion has no exact verified receipt.");
            }

            var boundary = await VerifyExpectationAsync(input.Plan, after, cancellationToken)
                .ConfigureAwait(false);
            if (boundary is not null)
            {
                return boundary;
            }
        }

        return null;
    }

    private async ValueTask<RouteRemoveAppliedVerification?> VerifyExpectationAsync(
        RouteRemovePlan plan,
        FileStateSnapshot after,
        CancellationToken cancellationToken)
    {
        var validation = await _expectationValidator.ValidateAsync(
            plan.Request.Workspace,
            after.Expectation,
            cancellationToken).ConfigureAwait(false);
        return validation.State switch
        {
            FileExpectationValidationState.Matched => null,
            FileExpectationValidationState.Cancelled => Interrupted(),
            FileExpectationValidationState.Mismatched
                or FileExpectationValidationState.Blocked
                or FileExpectationValidationState.Failed => Failed(
                    validation.Cause
                        ?? "An applied Route Remove effect changed before final verification."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(validation), validation.State, "The file validation state is not defined."),
        };
    }

    private static bool Matches(
        PlannedFileChange change,
        FileChangeReceipt receipt)
        => receipt.Change.Kind == change.Kind
            && receipt.Change.Expectation == change.Expectation
            && receipt.Change.IntendedBytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan())
            && receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static bool Matches(
        PlannedDirectoryDeletion deletion,
        DirectoryDeletionReceipt receipt)
        => receipt.Deletion == deletion
            && receipt.State.EffectState == FilesystemEffectState.Applied
            && receipt.State.VerificationState == FilesystemVerificationState.Verified
            && receipt.State.Disposition == DirectoryDeletionDisposition.Removed
            && receipt.After is { Kind: FileExpectationKind.Missing };
}
