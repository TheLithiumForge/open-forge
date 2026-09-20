using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Route.Update.Shared.Selection;

internal static class RouteUpdateWireVocabulary
{
    internal const string TemplateReferenceValueName = "template-reference";
    internal const string RouteUpdateHelpCommand = "open-forge route update --help";
    internal const string VerboseRouteUpdateCommand = "open-forge route update --detail debug";

    internal static string Name(RouteUpdateMode mode)
        => mode switch
        {
            RouteUpdateMode.Apply => "apply",
            RouteUpdateMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string Name(RouteUpdateTargetSelection selection)
        => selection switch
        {
            RouteUpdateTargetSelection.SourceId => "source-id",
            RouteUpdateTargetSelection.BasePath => "base-path",
            RouteUpdateTargetSelection.OverwritePath => "overwrite-path",
            _ => Undefined(nameof(selection), selection),
        };

    internal static string Name(RouteUpdateTargetForm form)
        => form switch
        {
            RouteUpdateTargetForm.OrdinaryMarkdown => "ordinary-markdown",
            RouteUpdateTargetForm.CanonicalEntrypoint => "canonical-entrypoint",
            RouteUpdateTargetForm.CompatibilityEntrypoint => "compatibility-entrypoint",
            _ => Undefined(nameof(form), form),
        };

    internal static string Name(RouteUpdatePatchState state)
        => state switch
        {
            RouteUpdatePatchState.NotRequested => "not-requested",
            RouteUpdatePatchState.Unresolved => "unresolved",
            RouteUpdatePatchState.Unchanged => "unchanged",
            RouteUpdatePatchState.Changed => "changed",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteUpdateResponsibilityOperation operation)
        => operation switch
        {
            RouteUpdateResponsibilityOperation.NotRequested => "not-requested",
            RouteUpdateResponsibilityOperation.Set => "set",
            RouteUpdateResponsibilityOperation.Remove => "remove",
            _ => Undefined(nameof(operation), operation),
        };

    internal static string Name(RouteUpdateTemplateDecision decision)
        => decision switch
        {
            RouteUpdateTemplateDecision.Unresolved => "unresolved",
            RouteUpdateTemplateDecision.Copied => "copied",
            RouteUpdateTemplateDecision.AuthoredBodyProtected => "authored-body-protected",
            _ => Undefined(nameof(decision), decision),
        };

    internal static string Name(RouteUpdatePlanCompleteness completeness)
        => completeness switch
        {
            RouteUpdatePlanCompleteness.NotEstablished => "not-established",
            RouteUpdatePlanCompleteness.Incomplete => "incomplete",
            RouteUpdatePlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(completeness), completeness),
        };

    internal static string Name(RouteUpdatePlanSafety safety)
        => safety switch
        {
            RouteUpdatePlanSafety.NotEstablished => "not-established",
            RouteUpdatePlanSafety.Safe => "safe",
            RouteUpdatePlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(safety), safety),
        };

    internal static string Name(RouteUpdateBodyState state)
        => state switch
        {
            RouteUpdateBodyState.NotEstablished => "not-established",
            RouteUpdateBodyState.Preserved => "preserved",
            RouteUpdateBodyState.TemplateCopied => "template-copied",
            RouteUpdateBodyState.AuthoredBodyProtected => "authored-body-protected",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteUpdateEffectKind kind)
        => kind switch
        {
            RouteUpdateEffectKind.RoutedFile => "routed-file",
            RouteUpdateEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteUpdateEffectAction action)
        => action switch
        {
            RouteUpdateEffectAction.Replace => "replace",
            _ => Undefined(nameof(action), action),
        };

    internal static string Name(RouteUpdatePreviewKind kind)
        => kind switch
        {
            RouteUpdatePreviewKind.MetadataField => "metadata-field",
            RouteUpdatePreviewKind.TemplateBody => "template-body",
            RouteUpdatePreviewKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteUpdateEffectOutcome outcome)
        => outcome switch
        {
            RouteUpdateEffectOutcome.Planned => "planned",
            RouteUpdateEffectOutcome.NotStarted => "not-started",
            RouteUpdateEffectOutcome.Verified => "verified",
            RouteUpdateEffectOutcome.VerificationFailed => "verification-failed",
            RouteUpdateEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string Name(RouteUpdateEffectResidual residual)
        => residual switch
        {
            RouteUpdateEffectResidual.None => "none",
            RouteUpdateEffectResidual.Retained => "retained",
            RouteUpdateEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string Name(RouteUpdateRecoveryState state)
        => state switch
        {
            RouteUpdateRecoveryState.NotRequired => "not-required",
            RouteUpdateRecoveryState.NotCreated => "not-created",
            RouteUpdateRecoveryState.Removed => "removed",
            RouteUpdateRecoveryState.Retained => "retained",
            RouteUpdateRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteUpdateVerificationState state)
        => state switch
        {
            RouteUpdateVerificationState.NotRequested => "not-requested",
            RouteUpdateVerificationState.Verified => "verified",
            RouteUpdateVerificationState.Failed => "failed",
            RouteUpdateVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string Name(RouteUpdateFindingCode code)
        => code switch
        {
            RouteUpdateFindingCode.InvalidInput => "route-update.invalid-input",
            RouteUpdateFindingCode.InvalidTarget => "route-update.invalid-target",
            RouteUpdateFindingCode.InvalidPatch => "route-update.invalid-patch",
            RouteUpdateFindingCode.InvalidTemplate => "route-update.invalid-template",
            RouteUpdateFindingCode.WorkspaceUnsafe => "route-update.workspace-unsafe",
            RouteUpdateFindingCode.TargetUnsafe => "route-update.target-unsafe",
            RouteUpdateFindingCode.RouteAmbiguous => "route-update.route-ambiguous",
            RouteUpdateFindingCode.IdentityCollision => "route-update.identity-collision",
            RouteUpdateFindingCode.FrontmatterUnsafe => "route-update.frontmatter-unsafe",
            RouteUpdateFindingCode.MetadataPreservationUnsafe => "route-update.metadata-preservation-unsafe",
            RouteUpdateFindingCode.TemplateUnsafe => "route-update.template-unsafe",
            RouteUpdateFindingCode.GeneratedRegionUnsafe => "route-update.generated-region-unsafe",
            RouteUpdateFindingCode.WorkspaceLockUnavailable => "route-update.workspace-lock-unavailable",
            RouteUpdateFindingCode.TargetChanged => "route-update.target-changed",
            RouteUpdateFindingCode.RecoveryConflict => "route-update.recovery-conflict",
            RouteUpdateFindingCode.WorkspaceUnavailable => "route-update.workspace-unavailable",
            RouteUpdateFindingCode.InspectionIncomplete => "route-update.inspection-incomplete",
            RouteUpdateFindingCode.TemplateUnavailable => "route-update.template-unavailable",
            RouteUpdateFindingCode.ProjectionIncomplete => "route-update.projection-incomplete",
            RouteUpdateFindingCode.RecoveryUnavailable => "route-update.recovery-unavailable",
            RouteUpdateFindingCode.TemplateBodyProtected => "route-update.template-body-protected",
            RouteUpdateFindingCode.RecoveryArtifactRetained => "route-update.recovery-artifact-retained",
            RouteUpdateFindingCode.TargetChangedDuringApply => "route-update.target-changed-during-apply",
            RouteUpdateFindingCode.WriteFailed => "route-update.write-failed",
            RouteUpdateFindingCode.VerificationFailed => "route-update.verification-failed",
            RouteUpdateFindingCode.RecoveryFailed => "route-update.recovery-failed",
            RouteUpdateFindingCode.OperationFailed => "route-update.operation-failed",
            RouteUpdateFindingCode.Interrupted => "route-update.interrupted",
            _ => Undefined(nameof(code), code),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Update {name} value is not defined.");
}
