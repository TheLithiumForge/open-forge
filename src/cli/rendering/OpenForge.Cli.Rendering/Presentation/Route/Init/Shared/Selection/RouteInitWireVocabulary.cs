using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Route.Init.Shared.Selection;

internal static class RouteInitWireVocabulary
{
    internal static string Name(RouteInitMode mode)
        => mode switch
        {
            RouteInitMode.Apply => "apply",
            RouteInitMode.DryRun => "dry-run",
            _ => throw Undefined(nameof(mode), mode),
        };

    internal static string Name(RouteInitScaffold scaffold)
        => scaffold switch
        {
            RouteInitScaffold.Generic => "generic",
            RouteInitScaffold.Framework => "framework",
            _ => throw Undefined(nameof(scaffold), scaffold),
        };

    internal static string Name(RouteInitPlanCompleteness completeness)
        => completeness switch
        {
            RouteInitPlanCompleteness.NotEstablished => "not-established",
            RouteInitPlanCompleteness.Incomplete => "incomplete",
            RouteInitPlanCompleteness.Complete => "complete",
            _ => throw Undefined(nameof(completeness), completeness),
        };

    internal static string Name(RouteInitPlanSafety safety)
        => safety switch
        {
            RouteInitPlanSafety.NotEstablished => "not-established",
            RouteInitPlanSafety.Safe => "safe",
            RouteInitPlanSafety.Blocked => "blocked",
            _ => throw Undefined(nameof(safety), safety),
        };

    internal static string Name(RouteInitEntrypointOutcome outcome)
        => outcome switch
        {
            RouteInitEntrypointOutcome.Unchanged => "unchanged",
            RouteInitEntrypointOutcome.Planned => "planned",
            RouteInitEntrypointOutcome.NotStarted => "not-started",
            RouteInitEntrypointOutcome.Created => "created",
            RouteInitEntrypointOutcome.VerificationFailed => "verification-failed",
            RouteInitEntrypointOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string Name(RouteInitDescriptionSource source)
        => source switch
        {
            RouteInitDescriptionSource.Draft => "draft",
            RouteInitDescriptionSource.Explicit => "explicit",
            RouteInitDescriptionSource.Embedded => "embedded",
            _ => throw Undefined(nameof(source), source),
        };

    internal static string Name(RouteInitResponsibilitySource source)
        => source switch
        {
            RouteInitResponsibilitySource.DefaultOmitted => "default-omitted",
            RouteInitResponsibilitySource.ExplicitOmitted => "explicit-omitted",
            RouteInitResponsibilitySource.Explicit => "explicit",
            RouteInitResponsibilitySource.Embedded => "embedded",
            _ => throw Undefined(nameof(source), source),
        };

    internal static string Name(RouteInitTagsSource source)
        => source switch
        {
            RouteInitTagsSource.Draft => "draft",
            RouteInitTagsSource.Explicit => "explicit",
            RouteInitTagsSource.Mixed => "mixed",
            RouteInitTagsSource.Embedded => "embedded",
            _ => throw Undefined(nameof(source), source),
        };

    internal static string Name(RouteInitEffectKind kind)
        => kind switch
        {
            RouteInitEffectKind.Directory => "directory",
            RouteInitEffectKind.Entrypoint => "entrypoint",
            RouteInitEffectKind.GeneratedRegion => "generated-region",
            _ => throw Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteInitEffectAction action)
        => action switch
        {
            RouteInitEffectAction.Create => "create",
            RouteInitEffectAction.Replace => "replace",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string Name(RouteInitEffectOutcome outcome)
        => outcome switch
        {
            RouteInitEffectOutcome.Planned => "planned",
            RouteInitEffectOutcome.NotStarted => "not-started",
            RouteInitEffectOutcome.Verified => "verified",
            RouteInitEffectOutcome.VerificationFailed => "verification-failed",
            RouteInitEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string Name(RouteInitRecoveryState state)
        => state switch
        {
            RouteInitRecoveryState.NotRequired => "not-required",
            RouteInitRecoveryState.NotCreated => "not-created",
            RouteInitRecoveryState.Removed => "removed",
            RouteInitRecoveryState.Retained => "retained",
            RouteInitRecoveryState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static string Name(RouteInitVerificationState state)
        => state switch
        {
            RouteInitVerificationState.NotRequested => "not-requested",
            RouteInitVerificationState.Verified => "verified",
            RouteInitVerificationState.Failed => "failed",
            RouteInitVerificationState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static string Name(RouteInitFindingCode code)
        => code switch
        {
            RouteInitFindingCode.InvalidInput => "route-init.invalid-input",
            RouteInitFindingCode.InvalidTarget => "route-init.invalid-target",
            RouteInitFindingCode.InvalidMetadata => "route-init.invalid-metadata",
            RouteInitFindingCode.WorkspaceUnavailable => "route-init.workspace-unavailable",
            RouteInitFindingCode.WorkspaceUnsafe => "route-init.workspace-unsafe",
            RouteInitFindingCode.TargetUnsafe => "route-init.target-unsafe",
            RouteInitFindingCode.RouteAmbiguous => "route-init.route-ambiguous",
            RouteInitFindingCode.IdentityCollision => "route-init.identity-collision",
            RouteInitFindingCode.LoaderUnsafe => "route-init.loader-unsafe",
            RouteInitFindingCode.FrameworkPayloadInvalid => "route-init.framework-payload-invalid",
            RouteInitFindingCode.FrameworkInstallRequired => "route-init.framework-install-required",
            RouteInitFindingCode.FrameworkUpdateRequired => "route-init.framework-update-required",
            RouteInitFindingCode.FrameworkAlignmentBlocked => "route-init.framework-alignment-blocked",
            RouteInitFindingCode.MetadataUnsafe => "route-init.metadata-unsafe",
            RouteInitFindingCode.GeneratedRegionUnsafe => "route-init.generated-region-unsafe",
            RouteInitFindingCode.LifecycleBlocked => "route-init.lifecycle-blocked",
            RouteInitFindingCode.WorkspaceLockUnavailable => "route-init.workspace-lock-unavailable",
            RouteInitFindingCode.TargetChanged => "route-init.target-changed",
            RouteInitFindingCode.RecoveryConflict => "route-init.recovery-conflict",
            RouteInitFindingCode.FrameworkPayloadUnavailable => "route-init.framework-payload-unavailable",
            RouteInitFindingCode.InspectionIncomplete => "route-init.inspection-incomplete",
            RouteInitFindingCode.MetadataIncomplete => "route-init.metadata-incomplete",
            RouteInitFindingCode.ProjectionIncomplete => "route-init.projection-incomplete",
            RouteInitFindingCode.LifecycleUnavailable => "route-init.lifecycle-unavailable",
            RouteInitFindingCode.RecoveryUnavailable => "route-init.recovery-unavailable",
            RouteInitFindingCode.NeedsAuthoring => "route-init.needs-authoring",
            RouteInitFindingCode.RecoveryArtifactRetained => "route-init.recovery-artifact-retained",
            RouteInitFindingCode.TargetChangedDuringApply => "route-init.target-changed-during-apply",
            RouteInitFindingCode.WriteFailed => "route-init.write-failed",
            RouteInitFindingCode.VerificationFailed => "route-init.verification-failed",
            RouteInitFindingCode.LifecyclePublicationFailed => "route-init.lifecycle-publication-failed",
            RouteInitFindingCode.RecoveryFailed => "route-init.recovery-failed",
            RouteInitFindingCode.OperationFailed => "route-init.operation-failed",
            RouteInitFindingCode.Interrupted => "route-init.interrupted",
            _ => throw Undefined(nameof(code), code),
        };

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Route Init {name} value is not defined.");
}
