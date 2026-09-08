using OpenForge.Cli.Core.Commands.Extension.Models.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Remove;

internal static class ExtensionRemoveDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "extension remove";

    internal static readonly CliSyntaxDefinition RemoveCommand = new(
        name: "remove",
        description: "Release selected managed Extension ownership and remove only eligible content.");

    internal static readonly CliSyntaxDefinition StableId = new(
        name: "stable-id",
        description: "Select one exact managed Extension stable ID.");

    internal static readonly CliOptionDefinition<bool> Prune = new(
        name: "--prune",
        description: "Delete eligible changed final-owner content in this request.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        name: "--automatic",
        description: "Disable prompts without granting selection or delete authority.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        name: "--dry-run",
        description: "Preview the complete removal without writing files.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static IReadOnlyList<ExtensionRemoveFindingCode> FindingCodes { get; } =
        Array.AsReadOnly(Enum.GetValues<ExtensionRemoveFindingCode>());

    internal static ExtensionRemoveFindingCode ReadPermissionFinding(ExtensionPermissionFailure failure)
        => failure switch
        {
            ExtensionPermissionFailure.Required => ExtensionRemoveFindingCode.PermissionRequired,
            ExtensionPermissionFailure.Declined => ExtensionRemoveFindingCode.PermissionDeclined,
            ExtensionPermissionFailure.Invalid => ExtensionRemoveFindingCode.PermissionsInvalid,
            ExtensionPermissionFailure.Unavailable => ExtensionRemoveFindingCode.PermissionsUnavailable,
            ExtensionPermissionFailure.Changed => ExtensionRemoveFindingCode.PermissionsChanged,
            ExtensionPermissionFailure.WriteFailed => ExtensionRemoveFindingCode.PermissionWriteFailed,
            ExtensionPermissionFailure.Interrupted => ExtensionRemoveFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The permission failure is not defined."),
        };

    internal static string ReadMachineName(ExtensionRemoveFindingCode code)
        => code switch
        {
            ExtensionRemoveFindingCode.InvalidInput => "extension-remove.invalid-input",
            ExtensionRemoveFindingCode.SelectionRequired => "extension-remove.selection-required",
            ExtensionRemoveFindingCode.InteractionEnded => "extension-remove.interaction-ended",
            ExtensionRemoveFindingCode.FrameworkUnavailable => "extension-remove.framework-unavailable",
            ExtensionRemoveFindingCode.FrameworkUnsafe => "extension-remove.framework-unsafe",
            ExtensionRemoveFindingCode.LifecycleUnavailable => "extension-remove.lifecycle-unavailable",
            ExtensionRemoveFindingCode.LifecycleBlocked => "extension-remove.lifecycle-blocked",
            ExtensionRemoveFindingCode.DependencyBlocked => "extension-remove.dependency-blocked",
            ExtensionRemoveFindingCode.OwnershipConflict => "extension-remove.ownership-conflict",
            ExtensionRemoveFindingCode.ManagedDivergence => "extension-remove.managed-divergence",
            ExtensionRemoveFindingCode.PermissionRequired => "extension-remove.permission-required",
            ExtensionRemoveFindingCode.PermissionDeclined => "extension-remove.permission-declined",
            ExtensionRemoveFindingCode.PermissionsInvalid => "extension-remove.permissions-invalid",
            ExtensionRemoveFindingCode.PermissionsUnavailable => "extension-remove.permissions-unavailable",
            ExtensionRemoveFindingCode.PermissionsChanged => "extension-remove.permissions-changed",
            ExtensionRemoveFindingCode.PermissionWriteFailed => "extension-remove.permission-write-failed",
            ExtensionRemoveFindingCode.TargetUnsafe => "extension-remove.target-unsafe",
            ExtensionRemoveFindingCode.ProjectionUnavailable => "extension-remove.projection-unavailable",
            ExtensionRemoveFindingCode.GeneratedRegionUnsafe => "extension-remove.generated-region-unsafe",
            ExtensionRemoveFindingCode.WorkspaceLockUnavailable => "extension-remove.workspace-lock-unavailable",
            ExtensionRemoveFindingCode.TargetChanged => "extension-remove.target-changed",
            ExtensionRemoveFindingCode.RecoveryConflict => "extension-remove.recovery-conflict",
            ExtensionRemoveFindingCode.RecoveryUnavailable => "extension-remove.recovery-unavailable",
            ExtensionRemoveFindingCode.LifecycleObservation => "extension-remove.lifecycle-observation",
            ExtensionRemoveFindingCode.RecoveryArtifactRetained => "extension-remove.recovery-artifact-retained",
            ExtensionRemoveFindingCode.WriteFailed => "extension-remove.write-failed",
            ExtensionRemoveFindingCode.TopologyVerificationFailed => "extension-remove.topology-verification-failed",
            ExtensionRemoveFindingCode.LifecyclePublicationFailed => "extension-remove.lifecycle-publication-failed",
            ExtensionRemoveFindingCode.VerificationFailed => "extension-remove.verification-failed",
            ExtensionRemoveFindingCode.RecoveryFailed => "extension-remove.recovery-failed",
            ExtensionRemoveFindingCode.OperationFailed => "extension-remove.operation-failed",
            ExtensionRemoveFindingCode.Interrupted => "extension-remove.interrupted",
            _ => Undefined(nameof(code), code),
        };

    internal static CliSemanticStatus ReadStatus(ExtensionRemoveFindingCode code)
        => code switch
        {
            ExtensionRemoveFindingCode.InvalidInput
                or ExtensionRemoveFindingCode.SelectionRequired
                or ExtensionRemoveFindingCode.InteractionEnded => CliSemanticStatus.Invalid,
            ExtensionRemoveFindingCode.FrameworkUnavailable
                or ExtensionRemoveFindingCode.LifecycleUnavailable
                or ExtensionRemoveFindingCode.ProjectionUnavailable
                or ExtensionRemoveFindingCode.RecoveryUnavailable
                or ExtensionRemoveFindingCode.PermissionsUnavailable => CliSemanticStatus.Incomplete,
            ExtensionRemoveFindingCode.FrameworkUnsafe
                or ExtensionRemoveFindingCode.LifecycleBlocked
                or ExtensionRemoveFindingCode.DependencyBlocked
                or ExtensionRemoveFindingCode.OwnershipConflict
                or ExtensionRemoveFindingCode.PermissionRequired
                or ExtensionRemoveFindingCode.PermissionDeclined
                or ExtensionRemoveFindingCode.PermissionsInvalid
                or ExtensionRemoveFindingCode.PermissionsChanged
                or ExtensionRemoveFindingCode.TargetUnsafe
                or ExtensionRemoveFindingCode.GeneratedRegionUnsafe
                or ExtensionRemoveFindingCode.WorkspaceLockUnavailable
                or ExtensionRemoveFindingCode.TargetChanged
                or ExtensionRemoveFindingCode.RecoveryConflict => CliSemanticStatus.Blocked,
            ExtensionRemoveFindingCode.LifecycleObservation
                or ExtensionRemoveFindingCode.RecoveryArtifactRetained
                or ExtensionRemoveFindingCode.ManagedDivergence => CliSemanticStatus.Attention,
            ExtensionRemoveFindingCode.WriteFailed
                or ExtensionRemoveFindingCode.TopologyVerificationFailed
                or ExtensionRemoveFindingCode.LifecyclePublicationFailed
                or ExtensionRemoveFindingCode.VerificationFailed
                or ExtensionRemoveFindingCode.RecoveryFailed
                or ExtensionRemoveFindingCode.OperationFailed
                or ExtensionRemoveFindingCode.PermissionWriteFailed => CliSemanticStatus.Failed,
            ExtensionRemoveFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Extension Remove finding code is not defined."),
        };

    internal static string ReadMachineName(ExtensionRemoveMode mode)
        => mode switch
        {
            ExtensionRemoveMode.Apply => "apply",
            ExtensionRemoveMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(ExtensionRemoveSelectionKind kind)
        => kind switch
        {
            ExtensionRemoveSelectionKind.ExplicitIds => "explicit-ids",
            ExtensionRemoveSelectionKind.InteractiveIds => "interactive-ids",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(ExtensionRemovePathClassification classification)
        => classification switch
        {
            ExtensionRemovePathClassification.Shared => "shared",
            ExtensionRemovePathClassification.UnchangedFinalOwner => "unchanged-final-owner",
            ExtensionRemovePathClassification.ChangedFinalOwner => "changed-final-owner",
            ExtensionRemovePathClassification.Missing => "missing",
            _ => Undefined(nameof(classification), classification),
        };

    internal static string ReadMachineName(ExtensionRemovePathAction action)
        => action switch
        {
            ExtensionRemovePathAction.RetainShared => "retain-shared",
            ExtensionRemovePathAction.Delete => "delete",
            ExtensionRemovePathAction.KeepAsUnmanaged => "keep-as-unmanaged",
            ExtensionRemovePathAction.ReleaseOwnership => "release-ownership",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(ExtensionRemoveEffectKind kind)
        => kind switch
        {
            ExtensionRemoveEffectKind.PackageFile => "package-file",
            ExtensionRemoveEffectKind.GeneratedRegion => "generated-region",
            ExtensionRemoveEffectKind.Lifecycle => "lifecycle",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(ExtensionRemoveEffectAction action)
        => action switch
        {
            ExtensionRemoveEffectAction.ReleaseOwnership => "release-ownership",
            ExtensionRemoveEffectAction.Delete => "delete",
            ExtensionRemoveEffectAction.Retain => "retain",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(ExtensionRemoveEffectOutcome outcome)
        => outcome switch
        {
            ExtensionRemoveEffectOutcome.Planned => "planned",
            ExtensionRemoveEffectOutcome.NotStarted => "not-started",
            ExtensionRemoveEffectOutcome.Verified => "verified",
            ExtensionRemoveEffectOutcome.VerificationFailed => "verification-failed",
            ExtensionRemoveEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(ExtensionRemoveEffectResidual residual)
        => residual switch
        {
            ExtensionRemoveEffectResidual.None => "none",
            ExtensionRemoveEffectResidual.Retained => "retained",
            ExtensionRemoveEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string ReadMachineName(ExtensionRemoveChangedContentPolicy policy)
        => policy switch
        {
            ExtensionRemoveChangedContentPolicy.KeepAsUnmanaged => "keep-as-unmanaged",
            ExtensionRemoveChangedContentPolicy.Delete => "delete",
            _ => Undefined(nameof(policy), policy),
        };

    internal static string ReadMachineName(ExtensionRemoveGeneratedRegionState state)
        => state switch
        {
            ExtensionRemoveGeneratedRegionState.Unchanged => "unchanged",
            ExtensionRemoveGeneratedRegionState.Changed => "changed",
            ExtensionRemoveGeneratedRegionState.Unavailable => "unavailable",
            ExtensionRemoveGeneratedRegionState.Blocked => "blocked",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(ExtensionRemoveLifecycleTrust trust)
        => trust switch
        {
            ExtensionRemoveLifecycleTrust.NotRequested => "not-requested",
            ExtensionRemoveLifecycleTrust.Trusted => "trusted",
            ExtensionRemoveLifecycleTrust.Unavailable => "unavailable",
            ExtensionRemoveLifecycleTrust.Blocked => "blocked",
            _ => Undefined(nameof(trust), trust),
        };

    internal static string ReadMachineName(ExtensionRemoveLifecycleCoverage coverage)
        => coverage switch
        {
            ExtensionRemoveLifecycleCoverage.NotRequested => "not-requested",
            ExtensionRemoveLifecycleCoverage.Complete => "complete",
            ExtensionRemoveLifecycleCoverage.Incomplete => "incomplete",
            ExtensionRemoveLifecycleCoverage.Blocked => "blocked",
            _ => Undefined(nameof(coverage), coverage),
        };

    internal static string ReadMachineName(ExtensionRemoveLifecycleAction action)
        => action switch
        {
            ExtensionRemoveLifecycleAction.None => "none",
            ExtensionRemoveLifecycleAction.Preserve => "preserve",
            ExtensionRemoveLifecycleAction.Publish => "publish",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(ExtensionRemoveLifecycleOutcome outcome)
        => outcome switch
        {
            ExtensionRemoveLifecycleOutcome.NotRequested => "not-requested",
            ExtensionRemoveLifecycleOutcome.Planned => "planned",
            ExtensionRemoveLifecycleOutcome.AlreadyCurrent => "already-current",
            ExtensionRemoveLifecycleOutcome.NotStarted => "not-started",
            ExtensionRemoveLifecycleOutcome.Verified => "verified",
            ExtensionRemoveLifecycleOutcome.VerificationFailed => "verification-failed",
            ExtensionRemoveLifecycleOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(ExtensionRemoveRecoveryState state)
        => state switch
        {
            ExtensionRemoveRecoveryState.NotRequired => "not-required",
            ExtensionRemoveRecoveryState.NotCreated => "not-created",
            ExtensionRemoveRecoveryState.Removed => "removed",
            ExtensionRemoveRecoveryState.Retained => "retained",
            ExtensionRemoveRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(ExtensionRemoveVerificationState state)
        => state switch
        {
            ExtensionRemoveVerificationState.NotRequested => "not-requested",
            ExtensionRemoveVerificationState.Planned => "planned",
            ExtensionRemoveVerificationState.Verified => "verified",
            ExtensionRemoveVerificationState.Failed => "failed",
            ExtensionRemoveVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<ExtensionRemoveFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge extension remove --help",
                "Correct the named Extension Remove input, then rerun the request."),
            CliSemanticStatus.Attention when findings.Any(finding =>
                finding.Code == ExtensionRemoveFindingCode.RecoveryArtifactRetained) => new CliNextAction(
                "open-forge cleanup",
                "Review and remove the reported recovery artifact."),
            CliSemanticStatus.Attention
                or CliSemanticStatus.Incomplete
                or CliSemanticStatus.Blocked
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Extension Remove status is not defined."),
        };
    }

    private static string Undefined<T>(string parameterName, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            parameterName,
            value,
            "The Extension Remove value is not defined.");
}
