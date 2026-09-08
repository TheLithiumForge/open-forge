using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;

namespace OpenForge.Cli.Core.Framework.Permissions.Shared.Completion;

internal static class WorkspacePermissionReceiptProjection
{
    internal static WorkspacePermissionOutcome ReadOutcome(FileChangeReceipt receipt)
        => ReadOutcome(receipt.EffectState, receipt.VerificationState);

    internal static WorkspacePermissionOutcome ReadOutcome(
        FilesystemEffectState effectState,
        FilesystemVerificationState verificationState)
        => effectState switch
        {
            FilesystemEffectState.NotStarted => WorkspacePermissionOutcome.NotStarted,
            FilesystemEffectState.Unknown => WorkspacePermissionOutcome.CompletionUnknown,
            FilesystemEffectState.Applied => verificationState switch
            {
                FilesystemVerificationState.Verified => WorkspacePermissionOutcome.Verified,
                FilesystemVerificationState.Failed => WorkspacePermissionOutcome.VerificationFailed,
                FilesystemVerificationState.NotStarted => throw new InvalidOperationException("An applied file receipt requires verification facts."),
                _ => throw new ArgumentOutOfRangeException(nameof(verificationState), verificationState, "The file verification state is not defined."),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(effectState), effectState, "The file effect state is not defined."),
        };
}
