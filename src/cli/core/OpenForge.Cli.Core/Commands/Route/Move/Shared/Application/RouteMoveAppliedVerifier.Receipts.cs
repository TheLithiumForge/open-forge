using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveAppliedVerifier
{
    private async ValueTask<RouteMoveAppliedVerification?> VerifyReceiptsAsync(
        RouteMoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        var projection = input.Plan.Projection;
        var expectedCount = projection.DirectoryCreations.Length
            + projection.FileChanges.Length
            + projection.DirectoryDeletions.Length;
        if (!input.Progress.Findings.IsEmpty || input.Progress.Receipts.Length != expectedCount)
        {
            return Failed("Final Route Move verification requires one clean receipt per effect.");
        }

        var index = 0;
        var boundary = await VerifyCreationsAsync(input, index, cancellationToken)
            .ConfigureAwait(false);
        index += projection.DirectoryCreations.Length;
        boundary ??= await VerifyFilesAsync(input, index, cancellationToken)
            .ConfigureAwait(false);
        index += projection.FileChanges.Length;
        boundary ??= await VerifyDeletionsAsync(input, index, cancellationToken)
            .ConfigureAwait(false);
        return boundary;
    }

    private async ValueTask<RouteMoveAppliedVerification?> VerifyCreationsAsync(
        RouteMoveAppliedVerificationInput input,
        int start,
        CancellationToken cancellationToken)
    {
        var creations = input.Plan.Projection.DirectoryCreations;
        for (var offset = 0; offset < creations.Length; offset++)
        {
            if (input.Progress.Receipts[start + offset]
                    is not RouteMoveDirectoryCreationReceipt applied
                || !Matches(creations[offset], applied.Receipt)
                || applied.Receipt.After is not { } after)
            {
                return Failed("A planned Route Move directory creation has no exact verified receipt.");
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

    private async ValueTask<RouteMoveAppliedVerification?> VerifyFilesAsync(
        RouteMoveAppliedVerificationInput input,
        int start,
        CancellationToken cancellationToken)
    {
        var changes = input.Plan.Projection.FileChanges;
        for (var offset = 0; offset < changes.Length; offset++)
        {
            if (input.Progress.Receipts[start + offset]
                    is not RouteMoveFileChangeReceipt applied
                || !Matches(changes[offset], applied.Receipt)
                || applied.Receipt.After is not { } after)
            {
                return Failed("A planned Route Move file change has no exact verified receipt.");
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

    private async ValueTask<RouteMoveAppliedVerification?> VerifyDeletionsAsync(
        RouteMoveAppliedVerificationInput input,
        int start,
        CancellationToken cancellationToken)
    {
        var deletions = input.Plan.Projection.DirectoryDeletions;
        for (var offset = 0; offset < deletions.Length; offset++)
        {
            if (input.Progress.Receipts[start + offset]
                    is not RouteMoveDirectoryDeletionReceipt applied
                || !Matches(deletions[offset], applied.Receipt)
                || applied.Receipt.After is not { } after)
            {
                return Failed("A planned Route Move directory deletion has no exact verified receipt.");
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

    private async ValueTask<RouteMoveAppliedVerification?> VerifyExpectationAsync(
        RouteMovePlan plan,
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
                        ?? "An applied Route Move effect changed before final verification."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(validation), validation.State, "The file validation state is not defined."),
        };
    }

    private static bool Matches(
        PlannedDirectoryCreation creation,
        DirectoryCreationReceipt receipt)
        => receipt.Creation == creation
            && receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified
            && receipt.After is { Kind: FileExpectationKind.Directory };

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
