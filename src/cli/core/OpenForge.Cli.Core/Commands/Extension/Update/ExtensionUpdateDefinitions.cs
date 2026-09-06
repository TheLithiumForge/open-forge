using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Update;

internal static class ExtensionUpdateDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "extension update";

    internal static readonly CliSyntaxDefinition UpdateCommand = new(
        name: "update",
        description: "Reconcile managed Extension packages from one reviewed source.");

    internal static readonly CliOptionDefinition<string?> Source = new(
        name: "--source",
        description: "Read one exact local package or catalogue source.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "package-or-catalogue-path");

    internal static readonly CliOptionDefinition<bool> All = new(
        name: "--all",
        description: "Select every managed package represented by the reviewed source.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> Force = new(
        name: "--force",
        description: "Replace changed or restore missing current expected managed content when eligible.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> Prune = new(
        name: "--prune",
        description: "Delete eligible retired managed content.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> Automatic = new(
        name: "--automatic",
        description: "Disable prompts without granting selection, force, or prune authority.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        name: "--dry-run",
        description: "Preview the complete update without writing files.",
        arity: CliOptionArity.None,
        defaultValue: false);

    internal static IReadOnlyList<ExtensionUpdateFindingCode> FindingCodes { get; } =
        Array.AsReadOnly(Enum.GetValues<ExtensionUpdateFindingCode>());

    internal static string ReadMachineName(ExtensionUpdateFindingCode code)
        => code switch
        {
            ExtensionUpdateFindingCode.InvalidInput => "extension-update.invalid-input",
            ExtensionUpdateFindingCode.SelectionRequired => "extension-update.selection-required",
            ExtensionUpdateFindingCode.InteractionEnded => "extension-update.interaction-ended",
            ExtensionUpdateFindingCode.SourceUnavailable => "extension-update.source-unavailable",
            ExtensionUpdateFindingCode.SourceInvalid => "extension-update.source-invalid",
            ExtensionUpdateFindingCode.SourceOverlap => "extension-update.source-overlap",
            ExtensionUpdateFindingCode.SourceIdentityConflict => "extension-update.source-identity-conflict",
            ExtensionUpdateFindingCode.FrameworkUnavailable => "extension-update.framework-unavailable",
            ExtensionUpdateFindingCode.FrameworkUnsafe => "extension-update.framework-unsafe",
            ExtensionUpdateFindingCode.LifecycleUnavailable => "extension-update.lifecycle-unavailable",
            ExtensionUpdateFindingCode.LifecycleBlocked => "extension-update.lifecycle-blocked",
            ExtensionUpdateFindingCode.ManagedDivergence => "extension-update.managed-divergence",
            ExtensionUpdateFindingCode.OwnershipConflict => "extension-update.ownership-conflict",
            ExtensionUpdateFindingCode.TargetOutsideAgents => "extension-update.target-outside-agents",
            ExtensionUpdateFindingCode.TargetUnsafe => "extension-update.target-unsafe",
            ExtensionUpdateFindingCode.ProjectionUnavailable => "extension-update.projection-unavailable",
            ExtensionUpdateFindingCode.GeneratedRegionUnsafe => "extension-update.generated-region-unsafe",
            ExtensionUpdateFindingCode.WorkspaceLockUnavailable => "extension-update.workspace-lock-unavailable",
            ExtensionUpdateFindingCode.TargetChanged => "extension-update.target-changed",
            ExtensionUpdateFindingCode.RecoveryConflict => "extension-update.recovery-conflict",
            ExtensionUpdateFindingCode.RecoveryUnavailable => "extension-update.recovery-unavailable",
            ExtensionUpdateFindingCode.LifecycleObservation => "extension-update.lifecycle-observation",
            ExtensionUpdateFindingCode.RecoveryArtifactRetained => "extension-update.recovery-artifact-retained",
            ExtensionUpdateFindingCode.WriteFailed => "extension-update.write-failed",
            ExtensionUpdateFindingCode.TopologyVerificationFailed => "extension-update.topology-verification-failed",
            ExtensionUpdateFindingCode.LifecyclePublicationFailed => "extension-update.lifecycle-publication-failed",
            ExtensionUpdateFindingCode.VerificationFailed => "extension-update.verification-failed",
            ExtensionUpdateFindingCode.RecoveryFailed => "extension-update.recovery-failed",
            ExtensionUpdateFindingCode.OperationFailed => "extension-update.operation-failed",
            ExtensionUpdateFindingCode.Interrupted => "extension-update.interrupted",
            _ => Undefined(nameof(code), code),
        };

    internal static CliSemanticStatus ReadStatus(ExtensionUpdateFindingCode code)
        => code switch
        {
            ExtensionUpdateFindingCode.InvalidInput
                or ExtensionUpdateFindingCode.SelectionRequired
                or ExtensionUpdateFindingCode.InteractionEnded
                or ExtensionUpdateFindingCode.SourceInvalid => CliSemanticStatus.Invalid,
            ExtensionUpdateFindingCode.SourceUnavailable
                or ExtensionUpdateFindingCode.FrameworkUnavailable
                or ExtensionUpdateFindingCode.LifecycleUnavailable
                or ExtensionUpdateFindingCode.ProjectionUnavailable
                or ExtensionUpdateFindingCode.RecoveryUnavailable => CliSemanticStatus.Incomplete,
            ExtensionUpdateFindingCode.FrameworkUnsafe
                or ExtensionUpdateFindingCode.SourceOverlap
                or ExtensionUpdateFindingCode.SourceIdentityConflict
                or ExtensionUpdateFindingCode.LifecycleBlocked
                or ExtensionUpdateFindingCode.OwnershipConflict
                or ExtensionUpdateFindingCode.TargetOutsideAgents
                or ExtensionUpdateFindingCode.TargetUnsafe
                or ExtensionUpdateFindingCode.GeneratedRegionUnsafe
                or ExtensionUpdateFindingCode.WorkspaceLockUnavailable
                or ExtensionUpdateFindingCode.TargetChanged
                or ExtensionUpdateFindingCode.RecoveryConflict => CliSemanticStatus.Blocked,
            ExtensionUpdateFindingCode.LifecycleObservation
                or ExtensionUpdateFindingCode.RecoveryArtifactRetained
                or ExtensionUpdateFindingCode.ManagedDivergence => CliSemanticStatus.Attention,
            ExtensionUpdateFindingCode.WriteFailed
                or ExtensionUpdateFindingCode.TopologyVerificationFailed
                or ExtensionUpdateFindingCode.LifecyclePublicationFailed
                or ExtensionUpdateFindingCode.VerificationFailed
                or ExtensionUpdateFindingCode.RecoveryFailed
                or ExtensionUpdateFindingCode.OperationFailed => CliSemanticStatus.Failed,
            ExtensionUpdateFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Extension Update finding code is not defined."),
        };

    internal static string ReadMachineName(ExtensionUpdateMode mode)
        => mode switch
        {
            ExtensionUpdateMode.Apply => "apply",
            ExtensionUpdateMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(ExtensionUpdateSelectionKind value)
        => value switch
        {
            ExtensionUpdateSelectionKind.ExplicitIds => "explicit-ids",
            ExtensionUpdateSelectionKind.ExplicitAll => "explicit-all",
            ExtensionUpdateSelectionKind.SinglePackageInference => "single-package-inference",
            ExtensionUpdateSelectionKind.InteractiveIds => "interactive-ids",
            ExtensionUpdateSelectionKind.InteractiveAll => "interactive-all",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateSourceKind value)
        => value switch
        {
            ExtensionUpdateSourceKind.Embedded => "embedded",
            ExtensionUpdateSourceKind.Package => "package",
            ExtensionUpdateSourceKind.Catalogue => "catalogue",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateComparisonTargetKind value)
        => value switch
        {
            ExtensionUpdateComparisonTargetKind.PackageFile => "package-file",
            ExtensionUpdateComparisonTargetKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateComparisonFingerprintKind value)
        => value switch
        {
            ExtensionUpdateComparisonFingerprintKind.OpenForgeMarkdownV1 => "open-forge-markdown-v1",
            ExtensionUpdateComparisonFingerprintKind.ExactBytes => "exact-bytes",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateComparisonCurrentState value)
        => value switch
        {
            ExtensionUpdateComparisonCurrentState.Missing => "missing",
            ExtensionUpdateComparisonCurrentState.BaselineEquivalent => "baseline-equivalent",
            ExtensionUpdateComparisonCurrentState.FormatOnly => "format-only",
            ExtensionUpdateComparisonCurrentState.Changed => "changed",
            ExtensionUpdateComparisonCurrentState.Unavailable => "unavailable",
            ExtensionUpdateComparisonCurrentState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateComparisonIntendedState value)
        => value switch
        {
            ExtensionUpdateComparisonIntendedState.Same => "same",
            ExtensionUpdateComparisonIntendedState.Changed => "changed",
            ExtensionUpdateComparisonIntendedState.New => "new",
            ExtensionUpdateComparisonIntendedState.Retired => "retired",
            ExtensionUpdateComparisonIntendedState.Unavailable => "unavailable",
            ExtensionUpdateComparisonIntendedState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateRetirementEligibility value)
        => value switch
        {
            ExtensionUpdateRetirementEligibility.NotApplicable => "not-applicable",
            ExtensionUpdateRetirementEligibility.Eligible => "eligible",
            ExtensionUpdateRetirementEligibility.Ineligible => "ineligible",
            ExtensionUpdateRetirementEligibility.Unavailable => "unavailable",
            ExtensionUpdateRetirementEligibility.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateEffectKind value)
        => value switch
        {
            ExtensionUpdateEffectKind.PackageFile => "package-file",
            ExtensionUpdateEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateEffectAction value)
        => value switch
        {
            ExtensionUpdateEffectAction.Create => "create",
            ExtensionUpdateEffectAction.Replace => "replace",
            ExtensionUpdateEffectAction.Delete => "delete",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateChangeAction value)
        => value switch
        {
            ExtensionUpdateChangeAction.Create => "create",
            ExtensionUpdateChangeAction.Replace => "replace",
            ExtensionUpdateChangeAction.Restore => "restore",
            ExtensionUpdateChangeAction.Delete => "delete",
            ExtensionUpdateChangeAction.Preserve => "preserve",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateEffectOutcome value)
        => value switch
        {
            ExtensionUpdateEffectOutcome.Planned => "planned",
            ExtensionUpdateEffectOutcome.NotStarted => "not-started",
            ExtensionUpdateEffectOutcome.Verified => "verified",
            ExtensionUpdateEffectOutcome.VerificationFailed => "verification-failed",
            ExtensionUpdateEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateEffectResidual value)
        => value switch
        {
            ExtensionUpdateEffectResidual.None => "none",
            ExtensionUpdateEffectResidual.Retained => "retained",
            ExtensionUpdateEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateGeneratedRegionState value)
        => value switch
        {
            ExtensionUpdateGeneratedRegionState.Unchanged => "unchanged",
            ExtensionUpdateGeneratedRegionState.Changed => "changed",
            ExtensionUpdateGeneratedRegionState.New => "new",
            ExtensionUpdateGeneratedRegionState.Retired => "retired",
            ExtensionUpdateGeneratedRegionState.Unavailable => "unavailable",
            ExtensionUpdateGeneratedRegionState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateLifecycleTrust value)
        => value switch
        {
            ExtensionUpdateLifecycleTrust.NotRequested => "not-requested",
            ExtensionUpdateLifecycleTrust.Trusted => "trusted",
            ExtensionUpdateLifecycleTrust.Unavailable => "unavailable",
            ExtensionUpdateLifecycleTrust.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateLifecycleCoverage value)
        => value switch
        {
            ExtensionUpdateLifecycleCoverage.NotRequested => "not-requested",
            ExtensionUpdateLifecycleCoverage.Complete => "complete",
            ExtensionUpdateLifecycleCoverage.Incomplete => "incomplete",
            ExtensionUpdateLifecycleCoverage.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateLifecycleAction value)
        => value switch
        {
            ExtensionUpdateLifecycleAction.None => "none",
            ExtensionUpdateLifecycleAction.Preserve => "preserve",
            ExtensionUpdateLifecycleAction.Publish => "publish",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateLifecycleOutcome value)
        => value switch
        {
            ExtensionUpdateLifecycleOutcome.NotRequested => "not-requested",
            ExtensionUpdateLifecycleOutcome.Planned => "planned",
            ExtensionUpdateLifecycleOutcome.AlreadyCurrent => "already-current",
            ExtensionUpdateLifecycleOutcome.NotStarted => "not-started",
            ExtensionUpdateLifecycleOutcome.Verified => "verified",
            ExtensionUpdateLifecycleOutcome.VerificationFailed => "verification-failed",
            ExtensionUpdateLifecycleOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateRecoveryState value)
        => value switch
        {
            ExtensionUpdateRecoveryState.NotRequired => "not-required",
            ExtensionUpdateRecoveryState.NotCreated => "not-created",
            ExtensionUpdateRecoveryState.Removed => "removed",
            ExtensionUpdateRecoveryState.Retained => "retained",
            ExtensionUpdateRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReadMachineName(ExtensionUpdateVerificationState value)
        => value switch
        {
            ExtensionUpdateVerificationState.NotRequested => "not-requested",
            ExtensionUpdateVerificationState.Planned => "planned",
            ExtensionUpdateVerificationState.Verified => "verified",
            ExtensionUpdateVerificationState.Failed => "failed",
            ExtensionUpdateVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(value), value),
        };

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<ExtensionUpdateFinding> findings,
        bool force,
        bool prune,
        bool automatic,
        ExtensionUpdateMode mode)
    {
        ArgumentNullException.ThrowIfNull(findings);
        _ = force;
        _ = prune;
        _ = automatic;
        _ = mode;
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge extension update --help",
                "Correct the named Extension Update input, then rerun the request."),
            CliSemanticStatus.Attention when findings.Any(finding =>
                finding.Code == ExtensionUpdateFindingCode.RecoveryArtifactRetained) => new CliNextAction(
                "open-forge cleanup",
                "Review and remove the reported recovery artifact."),
            CliSemanticStatus.Blocked
                or CliSemanticStatus.Incomplete
                or CliSemanticStatus.Attention
                or CliSemanticStatus.Failed
                or CliSemanticStatus.Interrupted => null,
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Extension Update status is not defined."),
        };
    }

    private static string Undefined<T>(string parameterName, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(parameterName, value, "The Extension Update value is not defined.");
}
