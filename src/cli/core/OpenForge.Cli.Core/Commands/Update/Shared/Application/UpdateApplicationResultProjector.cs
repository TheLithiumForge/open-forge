using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal static class UpdateApplicationResultProjector
{
    internal static IReadOnlyList<UpdatePhysicalEffect> Effects(
        UpdatePlanExecution execution,
        UpdateApplicationAttempt attempt)
    {
        var results = new List<UpdatePhysicalEffect>(execution.Effects.Count);
        for (var index = 0; index < execution.Effects.Count; index++)
        {
            var planned = execution.Effects[index].ResultEffect;
            var receipt = index < attempt.EffectReceipts.Count
                ? attempt.EffectReceipts[index]
                : null;
            results.Add(new UpdatePhysicalEffect(
                planned.Path,
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
        if (execution.LifecycleChange is null)
        {
            return new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.Trusted,
                Coverage = UpdateLifecycleCoverage.Complete,
                Action = UpdateLifecycleAction.Preserve,
                Outcome = UpdateLifecycleOutcome.AlreadyCurrent,
            };
        }

        var receipt = attempt.LifecycleReceipt;
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
}
