using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Selection;

internal static class InstallWireVocabulary
{
    internal const string OwnershipRecordPath = ".agents/open-forge.lock.json";

    internal static string Name(InstallFindingCode code)
        => code switch
        {
            InstallFindingCode.InvalidInput => "install.invalid-input",
            InstallFindingCode.ConfirmationRequired => "install.confirmation-required",
            InstallFindingCode.WorkspaceUnavailable => "install.workspace-unavailable",
            InstallFindingCode.WorkspaceUnsafe => "install.workspace-unsafe",
            InstallFindingCode.ManagedDivergence => "install.managed-divergence",
            InstallFindingCode.TargetOccupied => "install.target-occupied",
            InstallFindingCode.OwnershipConflict => "install.ownership-conflict",
            InstallFindingCode.TargetUnsafe => "install.target-unsafe",
            InstallFindingCode.GeneratedRegionUnsafe => "install.generated-region-unsafe",
            InstallFindingCode.LifecycleBlocked => "install.lifecycle-blocked",
            InstallFindingCode.RecoveryConflict => "install.recovery-conflict",
            InstallFindingCode.PayloadUnavailable => "install.payload-unavailable",
            InstallFindingCode.PayloadInvalid => "install.payload-invalid",
            InstallFindingCode.LifecycleUnavailable => "install.lifecycle-unavailable",
            InstallFindingCode.ProjectionUnavailable => "install.projection-unavailable",
            InstallFindingCode.RecoveryUnavailable => "install.recovery-unavailable",
            InstallFindingCode.RecoveryArtifactRetained => "install.recovery-artifact-retained",
            InstallFindingCode.WriteFailed => "install.write-failed",
            InstallFindingCode.VerificationFailed => "install.verification-failed",
            InstallFindingCode.LifecyclePublicationFailed => "install.lifecycle-publication-failed",
            InstallFindingCode.RecoveryFailed => "install.recovery-failed",
            InstallFindingCode.OperationFailed => "install.operation-failed",
            InstallFindingCode.Interrupted => "install.interrupted",
            _ => throw Undefined(nameof(code), code),
        };

    internal static string Name(InstallMode mode)
        => mode switch
        {
            InstallMode.Apply => "apply",
            InstallMode.DryRun => "dry-run",
            _ => throw Undefined(nameof(mode), mode),
        };

    internal static string Name(InstallManagementClassification classification)
        => classification switch
        {
            InstallManagementClassification.SafeAbsence => "safe-absence",
            InstallManagementClassification.TrustedExact => "trusted-exact",
            InstallManagementClassification.ManagedDivergence => "managed-divergence",
            InstallManagementClassification.EligibleInitialOccupant => "eligible-initial-occupant",
            _ => throw Undefined(nameof(classification), classification),
        };

    internal static string Name(InstallEffectKind kind)
        => kind switch
        {
            InstallEffectKind.Directory => "directory",
            InstallEffectKind.File => "file",
            InstallEffectKind.ManagedRegion => "managed-region",
            InstallEffectKind.GeneratedRegion => "generated-region",
            _ => throw Undefined(nameof(kind), kind),
        };

    internal static string Name(InstallEffectAction action)
        => action switch
        {
            InstallEffectAction.Create => "create",
            InstallEffectAction.Append => "append",
            InstallEffectAction.Replace => "replace",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string Name(InstallEffectOutcome outcome)
        => outcome switch
        {
            InstallEffectOutcome.Planned => "planned",
            InstallEffectOutcome.NotStarted => "not-started",
            InstallEffectOutcome.Verified => "verified",
            InstallEffectOutcome.VerificationFailed => "verification-failed",
            InstallEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string Name(InstallEffectResidual residual)
        => residual switch
        {
            InstallEffectResidual.None => "none",
            InstallEffectResidual.Retained => "retained",
            InstallEffectResidual.Unknown => "unknown",
            _ => throw Undefined(nameof(residual), residual),
        };

    internal static string Name(InstallLifecycleAction action)
        => action switch
        {
            InstallLifecycleAction.None => "none",
            InstallLifecycleAction.Preserve => "preserve",
            InstallLifecycleAction.Publish => "publish",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string Name(InstallLifecycleOutcome outcome)
        => outcome switch
        {
            InstallLifecycleOutcome.NotRequested => "not-requested",
            InstallLifecycleOutcome.Planned => "planned",
            InstallLifecycleOutcome.AlreadyCurrent => "already-current",
            InstallLifecycleOutcome.NotStarted => "not-started",
            InstallLifecycleOutcome.Verified => "verified",
            InstallLifecycleOutcome.VerificationFailed => "verification-failed",
            InstallLifecycleOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string Name(InstallResultRecoveryState state)
        => state switch
        {
            InstallResultRecoveryState.NotRequired => "not-required",
            InstallResultRecoveryState.NotCreated => "not-created",
            InstallResultRecoveryState.Removed => "removed",
            InstallResultRecoveryState.Retained => "retained",
            InstallResultRecoveryState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static string Name(InstallResultVerificationState state)
        => state switch
        {
            InstallResultVerificationState.NotRequested => "not-requested",
            InstallResultVerificationState.Verified => "verified",
            InstallResultVerificationState.Failed => "failed",
            InstallResultVerificationState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Install {name} value is not defined.");
}
