using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal static class UpdateApplicationResultProjector
{
    internal static IReadOnlyList<UpdatePhysicalEffect> Effects(
        UpdatePlanExecution execution,
        UpdateApplicationAttempt attempt,
        bool fullyVerified = false)
    {
        var results = new List<UpdatePhysicalEffect>(
            execution.DirectoryCreations.Count + execution.Effects.Count);
        for (var index = 0; index < execution.DirectoryCreations.Count; index++)
        {
            var creation = execution.DirectoryCreations[index];
            var receipt = index < attempt.DirectoryReceipts.Count
                ? attempt.DirectoryReceipts[index]
                : null;
            var relativePath = Path.GetRelativePath(
                    execution.Request.Workspace.LexicalRoot,
                    creation.LogicalPath)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');
            results.Add(UpdatePhysicalEffect.Directory(
                relativePath,
                ReadOutcome(receipt),
                ReadResidual(receipt, fullyVerified)));
        }

        for (var index = 0; index < execution.Effects.Count; index++)
        {
            var planned = execution.Effects[index].ResultEffect;
            var receipt = index < attempt.EffectReceipts.Count
                ? attempt.EffectReceipts[index]
                : null;
            results.Add(new UpdatePhysicalEffect(
                planned.Path,
                planned.Kind,
                planned.Action,
                planned.Changes,
                ReadOutcome(receipt),
                ReadResidual(receipt)));
        }

        return results;
    }

    internal static UpdateLifecycle Lifecycle(
        UpdatePlanExecution execution,
        UpdateApplicationAttempt attempt)
    {
        if (execution.OwnershipChange is null)
        {
            return execution.Build.Preview.Lifecycle;
        }

        var receipt = attempt.OwnershipReceipt;
        return new UpdateLifecycle
        {
            Trust = UpdateLifecycleTrust.Trusted,
            Coverage = UpdateLifecycleCoverage.Complete,
            Action = UpdateLifecycleAction.Publish,
            Outcome = receipt switch
            {
                null => UpdateLifecycleOutcome.NotStarted,
                {
                    EffectState: FilesystemEffectState.Applied,
                    VerificationState: FilesystemVerificationState.Verified,
                } =>
                    UpdateLifecycleOutcome.Verified,
                {
                    EffectState: FilesystemEffectState.Applied,
                    VerificationState: FilesystemVerificationState.Failed,
                } =>
                    UpdateLifecycleOutcome.VerificationFailed,
                {
                    EffectState: FilesystemEffectState.Unknown,
                    VerificationState: FilesystemVerificationState.Failed,
                } =>
                    UpdateLifecycleOutcome.CompletionUnknown,
                _ => UpdateLifecycleOutcome.NotStarted,
            },
        };
    }

    private static UpdatePhysicalEffectOutcome ReadOutcome(FileChangeReceipt? receipt)
        => receipt switch
        {
            null => UpdatePhysicalEffectOutcome.NotStarted,
            {
                EffectState: FilesystemEffectState.Applied,
                VerificationState: FilesystemVerificationState.Verified,
            } =>
                UpdatePhysicalEffectOutcome.Verified,
            {
                EffectState: FilesystemEffectState.Applied,
                VerificationState: FilesystemVerificationState.Failed,
            } =>
                UpdatePhysicalEffectOutcome.VerificationFailed,
            {
                EffectState: FilesystemEffectState.Unknown,
                VerificationState: FilesystemVerificationState.Failed,
            } =>
                UpdatePhysicalEffectOutcome.CompletionUnknown,
            _ => UpdatePhysicalEffectOutcome.NotStarted,
        };

    private static UpdatePhysicalEffectOutcome ReadOutcome(DirectoryCreationReceipt? receipt)
        => receipt switch
        {
            null => UpdatePhysicalEffectOutcome.NotStarted,
            {
                EffectState: FilesystemEffectState.Applied,
                VerificationState: FilesystemVerificationState.Verified,
            } => UpdatePhysicalEffectOutcome.Verified,
            {
                EffectState: FilesystemEffectState.Applied,
                VerificationState: FilesystemVerificationState.Failed,
            } => UpdatePhysicalEffectOutcome.VerificationFailed,
            {
                EffectState: FilesystemEffectState.Unknown,
            } => UpdatePhysicalEffectOutcome.CompletionUnknown,
            _ => UpdatePhysicalEffectOutcome.NotStarted,
        };

    private static UpdatePhysicalEffectResidual ReadResidual(FileChangeReceipt? receipt)
        => receipt switch
        {
            null => UpdatePhysicalEffectResidual.None,
            {
                EffectState: FilesystemEffectState.Unknown,
                VerificationState: FilesystemVerificationState.Failed,
            } =>
                UpdatePhysicalEffectResidual.Unknown,
            {
                EffectState: FilesystemEffectState.Applied,
                VerificationState: FilesystemVerificationState.Failed,
            } =>
                UpdatePhysicalEffectResidual.Retained,
            _ => UpdatePhysicalEffectResidual.None,
        };

    private static UpdatePhysicalEffectResidual ReadResidual(
        DirectoryCreationReceipt? receipt,
        bool fullyVerified)
    {
        if (receipt is null)
        {
            return UpdatePhysicalEffectResidual.None;
        }

        if (receipt.EffectState == FilesystemEffectState.Unknown
            || receipt.VerificationState == FilesystemVerificationState.Failed)
        {
            return receipt.After switch
            {
                { Kind: FileExpectationKind.Missing } => UpdatePhysicalEffectResidual.None,
                { Kind: FileExpectationKind.File or FileExpectationKind.Directory } => UpdatePhysicalEffectResidual.Retained,
                _ => UpdatePhysicalEffectResidual.Unknown,
            };
        }

        return receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified
            && !fullyVerified
                ? UpdatePhysicalEffectResidual.Retained
                : UpdatePhysicalEffectResidual.None;
    }
}
