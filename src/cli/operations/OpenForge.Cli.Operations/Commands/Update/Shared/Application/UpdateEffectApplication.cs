using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal sealed class UpdateEffectApplication(FileChangeApplier fileApplier)
{
    private readonly FileChangeApplier _fileApplier = fileApplier;

    internal async ValueTask<UpdateApplicationAttempt> ApplyAsync(
        UpdatePlanExecution execution,
        WorkspaceLockLease lease,
        IReadOnlyList<FileExpectationValidationResult> checks,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(execution);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(checks);
        var changes = execution.Effects.Count
            + (execution.OwnershipChange is null ? 0 : 1);
        if (checks.Count != changes)
        {
            return Failed(
                [],
                ownershipReceipt: null,
                UpdateFindingCode.OperationFailed,
                "Update application requires one exact revalidation check per planned effect.");
        }

        var receipts = new List<FileChangeReceipt>();
        var checkIndex = 0;
        foreach (var effect in execution.Effects)
        {
            var receipt = await ApplyAsync(
                    lease,
                    effect.FileChange,
                    checks[checkIndex++],
                    preparation,
                    cancellationToken)
                .ConfigureAwait(false);
            receipts.Add(receipt);
            if (!IsVerified(receipt))
            {
                return Failed(
                    receipts,
                        ownershipReceipt: null,
                    ReadFinding(receipt),
                    receipt.Cause ?? "An Update target effect was not verified.");
            }
        }

        FileChangeReceipt? ownershipReceipt = null;
        if (execution.OwnershipChange is { } ownership)
        {
            ownershipReceipt = await ApplyAsync(
                    lease,
                    ownership,
                    checks[checkIndex++],
                    preparation,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!IsVerified(ownershipReceipt))
            {
                return Failed(
                    receipts,
                    ownershipReceipt,
                    ReadFinding(ownershipReceipt),
                    ownershipReceipt.Cause ?? "Update ownership publication was not verified.");
            }
        }

        return new UpdateApplicationAttempt(
            receipts,
            ownershipReceipt,
            Finding: null);
    }

    private async ValueTask<FileChangeReceipt> ApplyAsync(
        WorkspaceLockLease lease,
        PlannedFileChange change,
        FileExpectationValidationResult check,
        RecoveryBundlePreparation? preparation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _fileApplier.ApplyAsync(
                    lease,
                    change,
                    check,
                    change.Kind == PlannedFileChangeKind.Create ? null : preparation,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                change,
                check.Actual ?? throw new InvalidOperationException(
                    "A validated Update effect requires its exact before-state."),
                FilesystemNotStartedReason.Cancelled,
                "Update application was interrupted before effect verification.");
        }
    }

    private static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    private static UpdateFindingCode ReadFinding(FileChangeReceipt receipt)
    {
        if (receipt.NotStartedReason == FilesystemNotStartedReason.Cancelled)
        {
            return UpdateFindingCode.Interrupted;
        }

        if (receipt.EffectState == FilesystemEffectState.Applied)
        {
            return UpdateFindingCode.VerificationFailed;
        }

        return UpdateFindingCode.WriteFailed;
    }

    private static UpdateApplicationAttempt Failed(
        IReadOnlyList<FileChangeReceipt> receipts,
        FileChangeReceipt? ownershipReceipt,
        UpdateFindingCode code,
        string cause)
        => new(
            receipts,
            ownershipReceipt,
            new UpdateFinding(code, target: null, cause));
}
