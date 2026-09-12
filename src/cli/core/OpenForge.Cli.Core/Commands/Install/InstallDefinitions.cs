using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Install;

internal static class InstallDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "install";

    internal static readonly CliSyntaxDefinition InstallCommand = new(
        CommandIdentity,
        "Establish Framework management in the selected workspace.");

    internal static readonly CliOptionDefinition<bool> Force = new(
        "--force",
        "Replace one eligible initial occupant when establishing Framework management.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        "--automatic",
        "Suppress confirmation and use deterministic safe defaults.",
        CliOptionArity.None,
        false);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete installation without writing files.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<InstallFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<InstallFindingCode>());

    internal static string ReadMachineName(InstallFindingCode code)
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
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Install finding code is not defined."),
        };

    internal static string ReadMachineName(InstallMode mode)
        => mode switch
        {
            InstallMode.Apply => "apply",
            InstallMode.DryRun => "dry-run",
            _ => throw Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(InstallManagementClassification classification)
        => classification switch
        {
            InstallManagementClassification.SafeAbsence => "safe-absence",
            InstallManagementClassification.TrustedExact => "trusted-exact",
            InstallManagementClassification.ManagedDivergence => "managed-divergence",
            InstallManagementClassification.EligibleInitialOccupant => "eligible-initial-occupant",
            _ => throw Undefined(nameof(classification), classification),
        };

    internal static string ReadMachineName(InstallEffectKind kind)
        => kind switch
        {
            InstallEffectKind.Directory => "directory",
            InstallEffectKind.File => "file",
            InstallEffectKind.ManagedRegion => "managed-region",
            InstallEffectKind.GeneratedRegion => "generated-region",
            _ => throw Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(InstallEffectAction action)
        => action switch
        {
            InstallEffectAction.Create => "create",
            InstallEffectAction.Append => "append",
            InstallEffectAction.Replace => "replace",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(InstallEffectOutcome outcome)
        => outcome switch
        {
            InstallEffectOutcome.Planned => "planned",
            InstallEffectOutcome.NotStarted => "not-started",
            InstallEffectOutcome.Verified => "verified",
            InstallEffectOutcome.VerificationFailed => "verification-failed",
            InstallEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(InstallEffectResidual residual)
        => residual switch
        {
            InstallEffectResidual.None => "none",
            InstallEffectResidual.Retained => "retained",
            InstallEffectResidual.Unknown => "unknown",
            _ => throw Undefined(nameof(residual), residual),
        };

    internal static string ReadMachineName(InstallLifecycleAction action)
        => action switch
        {
            InstallLifecycleAction.None => "none",
            InstallLifecycleAction.Preserve => "preserve",
            InstallLifecycleAction.Publish => "publish",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(InstallLifecycleOutcome outcome)
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

    internal static string ReadMachineName(InstallResultRecoveryState state)
        => state switch
        {
            InstallResultRecoveryState.NotRequired => "not-required",
            InstallResultRecoveryState.NotCreated => "not-created",
            InstallResultRecoveryState.Removed => "removed",
            InstallResultRecoveryState.Retained => "retained",
            InstallResultRecoveryState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(InstallResultVerificationState state)
        => state switch
        {
            InstallResultVerificationState.NotRequested => "not-requested",
            InstallResultVerificationState.Verified => "verified",
            InstallResultVerificationState.Failed => "failed",
            InstallResultVerificationState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static CliSemanticStatus ReadStatus(InstallFindingCode code)
    {
        return code switch
        {
            InstallFindingCode.InvalidInput
                or InstallFindingCode.ConfirmationRequired => CliSemanticStatus.Invalid,
            InstallFindingCode.WorkspaceUnavailable
                or InstallFindingCode.WorkspaceUnsafe
                or InstallFindingCode.PayloadInvalid
                or InstallFindingCode.ManagedDivergence
                or InstallFindingCode.TargetOccupied
                or InstallFindingCode.OwnershipConflict
                or InstallFindingCode.TargetUnsafe
                or InstallFindingCode.GeneratedRegionUnsafe
                or InstallFindingCode.LifecycleBlocked
                or InstallFindingCode.RecoveryConflict => CliSemanticStatus.Blocked,
            InstallFindingCode.PayloadUnavailable
                or InstallFindingCode.LifecycleUnavailable
                or InstallFindingCode.ProjectionUnavailable
                or InstallFindingCode.RecoveryUnavailable => CliSemanticStatus.Incomplete,
            InstallFindingCode.RecoveryArtifactRetained => CliSemanticStatus.Attention,
            InstallFindingCode.WriteFailed
                or InstallFindingCode.VerificationFailed
                or InstallFindingCode.LifecyclePublicationFailed
                or InstallFindingCode.RecoveryFailed
                or InstallFindingCode.OperationFailed => CliSemanticStatus.Failed,
            InstallFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Install finding code is not defined."),
        };
    }

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Install {name} value is not defined.");

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<InstallFinding> findings,
        InstallBindingInput input)
    {
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid when findings.Any(
                finding => finding.Code == InstallFindingCode.ConfirmationRequired) => new CliNextAction(
                command: input.Force
                    ? "open-forge install --force --automatic"
                    : "open-forge install --automatic",
                reason: "Rerun the same Install request with explicit automatic mode."),
            CliSemanticStatus.Invalid => new CliNextAction(
                command: "open-forge install --help",
                reason: "Correct the named Install input, then rerun the request."),
            CliSemanticStatus.Blocked when findings.Any(
                finding => finding.Code == InstallFindingCode.ManagedDivergence) => new CliNextAction(
                command: "open-forge update",
                reason: "Update the existing managed Framework state from a fresh plan."),
            CliSemanticStatus.Blocked => new CliNextAction(
                command: "open-forge doctor",
                reason: "Inspect the blocked workspace, ownership, lifecycle, or safety boundary before rerunning Install."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                command: "open-forge doctor",
                reason: "Inspect the unavailable source, lifecycle, projection, or recovery facts before relying on Install."),
            CliSemanticStatus.Attention => new CliNextAction(
                command: "open-forge cleanup",
                reason: "Review and remove the reported recovery artifact after confirming the verified Install result."),
            CliSemanticStatus.Failed => new CliNextAction(
                command: "open-forge install --verbose",
                reason: "Report the failure and retry the same Install request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                command: "open-forge install",
                reason: "Rerun the same Install request."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Install status is not defined."),
        };
    }
}

internal enum InstallFindingCode
{
    InvalidInput,
    ConfirmationRequired,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    ManagedDivergence,
    TargetOccupied,
    OwnershipConflict,
    TargetUnsafe,
    GeneratedRegionUnsafe,
    LifecycleBlocked,
    RecoveryConflict,
    PayloadUnavailable,
    PayloadInvalid,
    LifecycleUnavailable,
    ProjectionUnavailable,
    RecoveryUnavailable,
    RecoveryArtifactRetained,
    WriteFailed,
    VerificationFailed,
    LifecyclePublicationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}
