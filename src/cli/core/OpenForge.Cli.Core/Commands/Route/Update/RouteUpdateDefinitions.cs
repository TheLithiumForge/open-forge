using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Update;

internal static class RouteUpdateDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route update";
    internal const string RouteUpdateCommandName = "update";
    internal const string SourceReferenceValueName = "source-reference";
    internal const string TextValueName = "text";
    internal const string TagValueName = "tag";
    internal const string TemplateReferenceValueName = "template-reference";

    internal const string RouteUpdateHelpCommand = "open-forge route update --help";
    internal const string RouteUpdateCommand = "open-forge route update";
    internal const string CleanupCommand = "open-forge cleanup";
    internal const string DoctorCommand = "open-forge doctor";
    internal const string VerboseRouteUpdateCommand = "open-forge route update --verbose";

    internal static readonly CliSyntaxDefinition UpdateCommand = new(
        RouteUpdateCommandName,
        "Update selected metadata or eligible Template body content on one routed source.");

    internal static readonly CliSyntaxDefinition SourceReference = new(
        SourceReferenceValueName,
        "Select one existing routed source by ID or exact path.");

    internal static readonly CliOptionDefinition<string?> Description = new(
        "--description",
        "Replace the routed source description.",
        CliOptionArity.ExactlyOne,
        null,
        TextValueName);

    internal static readonly CliOptionDefinition<string[]> Tag = new(
        "--tag",
        "Replace the ordered tag list; repeat for additional tags.",
        CliOptionArity.ExactlyOne,
        [],
        TagValueName);

    internal static readonly CliOptionDefinition<string?> Responsibility = new(
        "--responsibility",
        "Set the responsibility, or remove it with an exact empty value.",
        CliOptionArity.ExactlyOne,
        null,
        TextValueName);

    internal static readonly CliOptionDefinition<string?> Template = new(
        "--template",
        "Copy one routed Template body when the target body is eligible.",
        CliOptionArity.ExactlyOne,
        null,
        TemplateReferenceValueName);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete update plan without writing.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<RouteUpdateFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<RouteUpdateFindingCode>());

    internal static string ReadMachineName(RouteUpdateMode mode)
        => mode switch
        {
            RouteUpdateMode.Apply => "apply",
            RouteUpdateMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(RouteUpdateTargetSelection selection)
        => selection switch
        {
            RouteUpdateTargetSelection.SourceId => "source-id",
            RouteUpdateTargetSelection.BasePath => "base-path",
            RouteUpdateTargetSelection.OverwritePath => "overwrite-path",
            _ => Undefined(nameof(selection), selection),
        };

    internal static string ReadMachineName(RouteUpdateTargetForm form)
        => form switch
        {
            RouteUpdateTargetForm.OrdinaryMarkdown => "ordinary-markdown",
            RouteUpdateTargetForm.CanonicalEntrypoint => "canonical-entrypoint",
            RouteUpdateTargetForm.CompatibilityEntrypoint => "compatibility-entrypoint",
            _ => Undefined(nameof(form), form),
        };

    internal static string ReadMachineName(RouteUpdatePatchState state)
        => state switch
        {
            RouteUpdatePatchState.NotRequested => "not-requested",
            RouteUpdatePatchState.Unresolved => "unresolved",
            RouteUpdatePatchState.Unchanged => "unchanged",
            RouteUpdatePatchState.Changed => "changed",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteUpdateResponsibilityOperation operation)
        => operation switch
        {
            RouteUpdateResponsibilityOperation.NotRequested => "not-requested",
            RouteUpdateResponsibilityOperation.Set => "set",
            RouteUpdateResponsibilityOperation.Remove => "remove",
            _ => Undefined(nameof(operation), operation),
        };

    internal static string ReadMachineName(RouteUpdateTemplateClassification classification)
        => classification switch
        {
            RouteUpdateTemplateClassification.Template => "template",
            _ => Undefined(nameof(classification), classification),
        };

    internal static string ReadMachineName(RouteUpdateTemplateDecision decision)
        => decision switch
        {
            RouteUpdateTemplateDecision.Unresolved => "unresolved",
            RouteUpdateTemplateDecision.Copied => "copied",
            RouteUpdateTemplateDecision.AuthoredBodyProtected => "authored-body-protected",
            _ => Undefined(nameof(decision), decision),
        };

    internal static string ReadMachineName(RouteUpdatePlanCompleteness completeness)
        => completeness switch
        {
            RouteUpdatePlanCompleteness.NotEstablished => "not-established",
            RouteUpdatePlanCompleteness.Incomplete => "incomplete",
            RouteUpdatePlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(completeness), completeness),
        };

    internal static string ReadMachineName(RouteUpdatePlanSafety safety)
        => safety switch
        {
            RouteUpdatePlanSafety.NotEstablished => "not-established",
            RouteUpdatePlanSafety.Safe => "safe",
            RouteUpdatePlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(safety), safety),
        };

    internal static string ReadMachineName(RouteUpdateBodyState state)
        => state switch
        {
            RouteUpdateBodyState.NotEstablished => "not-established",
            RouteUpdateBodyState.Preserved => "preserved",
            RouteUpdateBodyState.TemplateCopied => "template-copied",
            RouteUpdateBodyState.AuthoredBodyProtected => "authored-body-protected",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteUpdateEffectKind kind)
        => kind switch
        {
            RouteUpdateEffectKind.RoutedFile => "routed-file",
            RouteUpdateEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteUpdateEffectAction action)
        => action switch
        {
            RouteUpdateEffectAction.Replace => "replace",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(RouteUpdatePreviewKind kind)
        => kind switch
        {
            RouteUpdatePreviewKind.MetadataField => "metadata-field",
            RouteUpdatePreviewKind.TemplateBody => "template-body",
            RouteUpdatePreviewKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteUpdateEffectOutcome outcome)
        => outcome switch
        {
            RouteUpdateEffectOutcome.Planned => "planned",
            RouteUpdateEffectOutcome.NotStarted => "not-started",
            RouteUpdateEffectOutcome.Verified => "verified",
            RouteUpdateEffectOutcome.VerificationFailed => "verification-failed",
            RouteUpdateEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(RouteUpdateEffectResidual residual)
        => residual switch
        {
            RouteUpdateEffectResidual.None => "none",
            RouteUpdateEffectResidual.Retained => "retained",
            RouteUpdateEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string ReadMachineName(RouteUpdateRecoveryState state)
        => state switch
        {
            RouteUpdateRecoveryState.NotRequired => "not-required",
            RouteUpdateRecoveryState.NotCreated => "not-created",
            RouteUpdateRecoveryState.Removed => "removed",
            RouteUpdateRecoveryState.Retained => "retained",
            RouteUpdateRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteUpdateVerificationState state)
        => state switch
        {
            RouteUpdateVerificationState.NotRequested => "not-requested",
            RouteUpdateVerificationState.Verified => "verified",
            RouteUpdateVerificationState.Failed => "failed",
            RouteUpdateVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteUpdateFindingCode code)
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

    internal static CliSemanticStatus ReadStatus(RouteUpdateFindingCode code)
        => code switch
        {
            RouteUpdateFindingCode.InvalidInput
                or RouteUpdateFindingCode.InvalidTarget
                or RouteUpdateFindingCode.InvalidPatch
                or RouteUpdateFindingCode.InvalidTemplate => CliSemanticStatus.Invalid,
            RouteUpdateFindingCode.WorkspaceUnsafe
                or RouteUpdateFindingCode.TargetUnsafe
                or RouteUpdateFindingCode.RouteAmbiguous
                or RouteUpdateFindingCode.IdentityCollision
                or RouteUpdateFindingCode.FrontmatterUnsafe
                or RouteUpdateFindingCode.MetadataPreservationUnsafe
                or RouteUpdateFindingCode.TemplateUnsafe
                or RouteUpdateFindingCode.GeneratedRegionUnsafe
                or RouteUpdateFindingCode.WorkspaceLockUnavailable
                or RouteUpdateFindingCode.TargetChanged
                or RouteUpdateFindingCode.RecoveryConflict => CliSemanticStatus.Blocked,
            RouteUpdateFindingCode.WorkspaceUnavailable
                or RouteUpdateFindingCode.InspectionIncomplete
                or RouteUpdateFindingCode.TemplateUnavailable
                or RouteUpdateFindingCode.ProjectionIncomplete
                or RouteUpdateFindingCode.RecoveryUnavailable => CliSemanticStatus.Incomplete,
            RouteUpdateFindingCode.TemplateBodyProtected
                or RouteUpdateFindingCode.RecoveryArtifactRetained => CliSemanticStatus.Attention,
            RouteUpdateFindingCode.TargetChangedDuringApply
                or RouteUpdateFindingCode.WriteFailed
                or RouteUpdateFindingCode.VerificationFailed
                or RouteUpdateFindingCode.RecoveryFailed
                or RouteUpdateFindingCode.OperationFailed => CliSemanticStatus.Failed,
            RouteUpdateFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Update finding code is not defined."),
        };

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<RouteUpdateFinding> findings)
    {
        if (Contains(findings, RouteUpdateFindingCode.RecoveryArtifactRetained))
        {
            return new CliNextAction(
                CleanupCommand,
                "Review and remove the reported recovery artifact after confirming the verified Route Update result.");
        }

        if (status == CliSemanticStatus.Attention
            && Contains(findings, RouteUpdateFindingCode.TemplateBodyProtected))
        {
            return new CliNextAction(
                RouteUpdateCommand,
                "review the authored body; the Template body was not applied.");
        }

        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                RouteUpdateHelpCommand,
                "Correct the named Route Update input, then rerun the request."),
            CliSemanticStatus.Blocked when Contains(findings, RouteUpdateFindingCode.WorkspaceLockUnavailable)
                || Contains(findings, RouteUpdateFindingCode.TargetChanged) => new CliNextAction(
                    RouteUpdateCommand,
                    "Wait for the blocking condition or inspect the changed target, then rerun Route Update from a fresh plan."),
            CliSemanticStatus.Blocked => new CliNextAction(
                DoctorCommand,
                "Inspect the blocked workspace, route, identity, metadata, generated-region, Template, or recovery boundary before rerunning Route Update."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                DoctorCommand,
                "Inspect the unavailable route, Template, projection, or recovery facts before relying on this Route Update result."),
            CliSemanticStatus.Attention => throw new ArgumentException(
                "Route Update attention requires a protected Template body or retained recovery artifact.",
                nameof(findings)),
            CliSemanticStatus.Failed => new CliNextAction(
                VerboseRouteUpdateCommand,
                "Report the failure and retry the same Route Update request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                RouteUpdateCommand,
                "Rerun the same Route Update request."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Route Update status is not defined."),
        };
    }

    private static bool Contains(
        IReadOnlyList<RouteUpdateFinding> findings,
        RouteUpdateFindingCode code)
        => findings.Any(finding => finding.Code == code);

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Update {name} value is not defined.");
}
