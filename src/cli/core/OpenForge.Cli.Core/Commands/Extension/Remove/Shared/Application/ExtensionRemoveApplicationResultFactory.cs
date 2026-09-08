using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

internal static class ExtensionRemoveApplicationResultFactory
{
    internal static ExtensionRemoveResult BeforeEffects(
        ExtensionRemovePlan plan,
        ExtensionRemoveResult planned,
        ExtensionRemoveFindingCode code,
        string cause,
        ExtensionRemoveRecovery? recovery = null)
        => Result(
            plan,
            planned,
            [],
            Lifecycle(plan, ExtensionRemoveLifecycleOutcome.NotStarted),
            recovery ?? new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.NotCreated,
                [],
                residualPath: null),
            NotRequestedVerification(),
            [.. planned.Findings, new ExtensionRemoveFinding(code, cause)]);

    internal static ExtensionRemoveResult AfterPreparation(
        ExtensionRemovePlan plan,
        ExtensionRemoveResult planned,
        IReadOnlyList<ExtensionRemoveEffect> effects,
        RecoveryBundlePreparation? preparation,
        ExtensionRemoveFindingCode code,
        string cause,
        string? target = null,
        ExtensionRemoveLifecycleOutcome lifecycleOutcome = ExtensionRemoveLifecycleOutcome.NotStarted,
        ExtensionRemoveVerification? verification = null)
        => Result(
            plan,
            planned,
            effects,
            Lifecycle(plan, lifecycleOutcome),
            RecoveryAfterFailure(preparation),
            verification ?? NotRequestedVerification(),
            [.. planned.Findings, new ExtensionRemoveFinding(code, cause, target)]);

    internal static ExtensionRemoveResult Result(
        ExtensionRemovePlan plan,
        ExtensionRemoveResult planned,
        IReadOnlyList<ExtensionRemoveEffect> effects,
        ExtensionRemoveLifecycle lifecycle,
        ExtensionRemoveRecovery recovery,
        ExtensionRemoveVerification verification,
        IReadOnlyList<ExtensionRemoveFinding> findings)
        => ExtensionRemoveResultFactory.Create(
            plan.Request,
            plan.Selection,
            plan.Dependencies,
            planned.Paths,
            planned.GeneratedNavigation,
            effects,
            lifecycle,
            recovery,
            verification,
            findings) with
        { Permissions = planned.Permissions };

    internal static ExtensionRemoveLifecycle Lifecycle(
        ExtensionRemovePlan plan,
        ExtensionRemoveLifecycleOutcome outcome)
        => new(
            ExtensionRemoveLifecycleTrust.Trusted,
            ExtensionRemoveLifecycleCoverage.Complete,
            plan.LifecycleChange is null
                ? ExtensionRemoveLifecycleAction.Preserve
                : ExtensionRemoveLifecycleAction.Publish,
            outcome);

    internal static ExtensionRemoveRecovery RecoveryUnknown(string? residualPath)
        => new(ExtensionRemoveRecoveryState.Unknown, [], residualPath);

    internal static ExtensionRemoveRecovery RecoveryAfterFailure(
        RecoveryBundlePreparation? preparation)
        => preparation is null
            ? RecoveryUnknown(residualPath: null)
            : new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.Retained,
                ReadProtectedPaths(preparation),
                preparation.BundlePath);

    internal static ExtensionRemoveRecovery RecoveryAfterCleanupEscape(
        RecoveryBundlePreparation? preparation)
        => preparation is null
            ? RecoveryUnknown(residualPath: null)
            : new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.Unknown,
                ReadProtectedPaths(preparation),
                residualPath: null);

    internal static ExtensionRemoveVerification Verified()
        => new(
            ExtensionRemoveVerificationState.Verified,
            ExtensionRemoveVerificationState.Verified,
            ExtensionRemoveVerificationState.Verified);

    internal static ExtensionRemoveVerification FailedVerification()
        => Verification(ExtensionRemoveVerificationState.Failed);

    internal static ExtensionRemoveVerification UnknownVerification()
        => Verification(ExtensionRemoveVerificationState.Unknown);

    internal static ExtensionRemoveVerification NotRequestedVerification()
        => Verification(ExtensionRemoveVerificationState.NotRequested);

    internal static bool IsVerified(FileChangeReceipt receipt)
        => receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified;

    internal static ExtensionRemoveEffect WithOutcome(
        ExtensionRemoveEffect effect,
        ExtensionRemoveEffectOutcome outcome)
        => new(
            effect.Path,
            effect.PackageId,
            effect.Kind,
            effect.Action,
            outcome,
            ReadResidual(outcome));

    internal static ExtensionRemoveEffectOutcome ReadOutcome(FileChangeReceipt receipt)
        => receipt.EffectState switch
        {
            FilesystemEffectState.NotStarted => ExtensionRemoveEffectOutcome.NotStarted,
            FilesystemEffectState.Applied when receipt.VerificationState == FilesystemVerificationState.Failed
                => ExtensionRemoveEffectOutcome.VerificationFailed,
            FilesystemEffectState.Applied => ExtensionRemoveEffectOutcome.Verified,
            FilesystemEffectState.Unknown => ExtensionRemoveEffectOutcome.CompletionUnknown,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.EffectState, "The filesystem receipt state is not defined."),
        };

    internal static ExtensionRemoveFindingCode ReadFinding(FileChangeReceipt receipt)
        => receipt.NotStartedReason switch
        {
            FilesystemNotStartedReason.Cancelled => ExtensionRemoveFindingCode.Interrupted,
            FilesystemNotStartedReason.TargetChanged => ExtensionRemoveFindingCode.TargetChanged,
            FilesystemNotStartedReason.ApplicationFailed => ExtensionRemoveFindingCode.WriteFailed,
            FilesystemNotStartedReason.ContractRejected => ExtensionRemoveFindingCode.OperationFailed,
            null when receipt.VerificationState == FilesystemVerificationState.Failed
                => ExtensionRemoveFindingCode.VerificationFailed,
            null => ExtensionRemoveFindingCode.WriteFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.NotStartedReason, "The filesystem receipt state is not defined."),
        };

    internal static ExtensionRemoveLifecycleOutcome ReadLifecycleOutcome(
        FileChangeReceipt receipt)
        => receipt.EffectState switch
        {
            FilesystemEffectState.NotStarted => ExtensionRemoveLifecycleOutcome.NotStarted,
            FilesystemEffectState.Applied when receipt.VerificationState == FilesystemVerificationState.Failed
                => ExtensionRemoveLifecycleOutcome.VerificationFailed,
            FilesystemEffectState.Applied => ExtensionRemoveLifecycleOutcome.Verified,
            FilesystemEffectState.Unknown => ExtensionRemoveLifecycleOutcome.CompletionUnknown,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.EffectState, "The filesystem receipt state is not defined."),
        };

    internal static ExtensionRemoveFindingCode ReadLifecycleFinding(FileChangeReceipt receipt)
        => receipt.NotStartedReason switch
        {
            FilesystemNotStartedReason.Cancelled => ExtensionRemoveFindingCode.Interrupted,
            FilesystemNotStartedReason.TargetChanged => ExtensionRemoveFindingCode.TargetChanged,
            FilesystemNotStartedReason.ContractRejected => ExtensionRemoveFindingCode.OperationFailed,
            FilesystemNotStartedReason.ApplicationFailed
                or null => ExtensionRemoveFindingCode.LifecyclePublicationFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt.NotStartedReason, "The filesystem receipt state is not defined."),
        };

    private static ExtensionRemoveEffectResidual ReadResidual(ExtensionRemoveEffectOutcome outcome)
        => outcome switch
        {
            ExtensionRemoveEffectOutcome.Planned
                or ExtensionRemoveEffectOutcome.Verified => ExtensionRemoveEffectResidual.None,
            ExtensionRemoveEffectOutcome.NotStarted => ExtensionRemoveEffectResidual.Retained,
            ExtensionRemoveEffectOutcome.VerificationFailed
                or ExtensionRemoveEffectOutcome.CompletionUnknown => ExtensionRemoveEffectResidual.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "The effect outcome is not defined."),
        };

    private static ExtensionRemoveVerification Verification(ExtensionRemoveVerificationState state)
        => new(
            state,
            state,
            state);

    private static IReadOnlyList<string> ReadProtectedPaths(RecoveryBundlePreparation preparation)
        => [.. preparation.Entries
            .OrderBy(entry => entry.Ordinal)
            .Select(entry => entry.TargetPath)];
}
