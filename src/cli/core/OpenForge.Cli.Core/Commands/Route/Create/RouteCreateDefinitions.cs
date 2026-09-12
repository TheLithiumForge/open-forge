using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Create;

internal static class RouteCreateDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "route create";
    internal const string RouteCreateCommandName = "create";
    internal const string FileTargetValueName = "file-target";
    internal const string TextValueName = "text";
    internal const string TagValueName = "tag";
    internal const string TemplateReferenceValueName = "template-reference";

    internal const string RouteCreateHelpCommand = "open-forge route create --help";
    internal const string RouteCreateCommand = "open-forge route create";
    internal const string RouteInitCommand = "open-forge route init";
    internal const string RouteUpdateCommand = "open-forge route update";
    internal const string CleanupCommand = "open-forge cleanup";
    internal const string DoctorCommand = "open-forge doctor";
    internal const string VerboseRouteCreateCommand = "open-forge route create --verbose";

    internal static readonly CliSyntaxDefinition CreateCommand = new(
        RouteCreateCommandName,
        "Create one ordinary routed Markdown file below an existing parent.");

    internal static readonly CliSyntaxDefinition FileTarget = new(
        FileTargetValueName,
        "Select one route ID or exact ordinary Markdown path.");

    internal static readonly CliOptionDefinition<string?> Description = new(
        "--description",
        "Set the destination description.",
        CliOptionArity.ExactlyOne,
        null,
        TextValueName);

    internal static readonly CliOptionDefinition<string[]> Tag = new(
        "--tag",
        "Add one ordered destination tag; repeat for additional tags.",
        CliOptionArity.ExactlyOne,
        [],
        TagValueName);

    internal static readonly CliOptionDefinition<string?> Responsibility = new(
        "--responsibility",
        "Set the optional destination responsibility.",
        CliOptionArity.ExactlyOne,
        null,
        TextValueName);

    internal static readonly CliOptionDefinition<string?> Template = new(
        "--template",
        "Copy the body of one existing routed Template.",
        CliOptionArity.ExactlyOne,
        null,
        TemplateReferenceValueName);

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview the complete creation plan without writing.",
        CliOptionArity.None,
        false);

    internal static readonly IReadOnlyList<RouteCreateFindingCode> FindingCodes =
        Array.AsReadOnly(Enum.GetValues<RouteCreateFindingCode>());

    internal static string ReadMachineName(RouteCreateMode mode)
        => mode switch
        {
            RouteCreateMode.Apply => "apply",
            RouteCreateMode.DryRun => "dry-run",
            _ => Undefined(nameof(mode), mode),
        };

    internal static string ReadMachineName(RouteCreateParentForm form)
        => form switch
        {
            RouteCreateParentForm.Canonical => "canonical",
            RouteCreateParentForm.Compatibility => "compatibility",
            _ => Undefined(nameof(form), form),
        };

    internal static string ReadMachineName(RouteCreateTemplateClassification classification)
        => classification switch
        {
            RouteCreateTemplateClassification.Template => "template",
            _ => Undefined(nameof(classification), classification),
        };

    internal static string ReadMachineName(RouteCreatePlanCompleteness completeness)
        => completeness switch
        {
            RouteCreatePlanCompleteness.NotEstablished => "not-established",
            RouteCreatePlanCompleteness.Incomplete => "incomplete",
            RouteCreatePlanCompleteness.Complete => "complete",
            _ => Undefined(nameof(completeness), completeness),
        };

    internal static string ReadMachineName(RouteCreatePlanSafety safety)
        => safety switch
        {
            RouteCreatePlanSafety.NotEstablished => "not-established",
            RouteCreatePlanSafety.Safe => "safe",
            RouteCreatePlanSafety.Blocked => "blocked",
            _ => Undefined(nameof(safety), safety),
        };

    internal static string ReadMachineName(RouteCreateEffectKind kind)
        => kind switch
        {
            RouteCreateEffectKind.RoutedFile => "routed-file",
            RouteCreateEffectKind.GeneratedRegion => "generated-region",
            _ => Undefined(nameof(kind), kind),
        };

    internal static string ReadMachineName(RouteCreateEffectAction action)
        => action switch
        {
            RouteCreateEffectAction.Create => "create",
            RouteCreateEffectAction.Replace => "replace",
            _ => Undefined(nameof(action), action),
        };

    internal static string ReadMachineName(RouteCreateEffectOutcome outcome)
        => outcome switch
        {
            RouteCreateEffectOutcome.Planned => "planned",
            RouteCreateEffectOutcome.NotStarted => "not-started",
            RouteCreateEffectOutcome.Verified => "verified",
            RouteCreateEffectOutcome.VerificationFailed => "verification-failed",
            RouteCreateEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => Undefined(nameof(outcome), outcome),
        };

    internal static string ReadMachineName(RouteCreateEffectResidual residual)
        => residual switch
        {
            RouteCreateEffectResidual.None => "none",
            RouteCreateEffectResidual.Retained => "retained",
            RouteCreateEffectResidual.Unknown => "unknown",
            _ => Undefined(nameof(residual), residual),
        };

    internal static string ReadMachineName(RouteCreateRecoveryState state)
        => state switch
        {
            RouteCreateRecoveryState.NotRequired => "not-required",
            RouteCreateRecoveryState.NotCreated => "not-created",
            RouteCreateRecoveryState.Removed => "removed",
            RouteCreateRecoveryState.Retained => "retained",
            RouteCreateRecoveryState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteCreateVerificationState state)
        => state switch
        {
            RouteCreateVerificationState.NotRequested => "not-requested",
            RouteCreateVerificationState.Verified => "verified",
            RouteCreateVerificationState.Failed => "failed",
            RouteCreateVerificationState.Unknown => "unknown",
            _ => Undefined(nameof(state), state),
        };

    internal static string ReadMachineName(RouteCreateFindingCode code)
        => code switch
        {
            RouteCreateFindingCode.InvalidInput => "route-create.invalid-input",
            RouteCreateFindingCode.InvalidTarget => "route-create.invalid-target",
            RouteCreateFindingCode.InvalidMetadata => "route-create.invalid-metadata",
            RouteCreateFindingCode.InvalidTemplate => "route-create.invalid-template",
            RouteCreateFindingCode.WorkspaceUnavailable => "route-create.workspace-unavailable",
            RouteCreateFindingCode.WorkspaceUnsafe => "route-create.workspace-unsafe",
            RouteCreateFindingCode.TargetUnsafe => "route-create.target-unsafe",
            RouteCreateFindingCode.TargetContentDiffers => "route-create.target-content-differs",
            RouteCreateFindingCode.ParentMissing => "route-create.parent-missing",
            RouteCreateFindingCode.RouteAmbiguous => "route-create.route-ambiguous",
            RouteCreateFindingCode.IdentityCollision => "route-create.identity-collision",
            RouteCreateFindingCode.TemplateUnsafe => "route-create.template-unsafe",
            RouteCreateFindingCode.MetadataUnsafe => "route-create.metadata-unsafe",
            RouteCreateFindingCode.GeneratedRegionUnsafe => "route-create.generated-region-unsafe",
            RouteCreateFindingCode.WorkspaceLockUnavailable => "route-create.workspace-lock-unavailable",
            RouteCreateFindingCode.TargetChanged => "route-create.target-changed",
            RouteCreateFindingCode.RecoveryConflict => "route-create.recovery-conflict",
            RouteCreateFindingCode.InspectionIncomplete => "route-create.inspection-incomplete",
            RouteCreateFindingCode.TemplateUnavailable => "route-create.template-unavailable",
            RouteCreateFindingCode.MetadataIncomplete => "route-create.metadata-incomplete",
            RouteCreateFindingCode.ProjectionIncomplete => "route-create.projection-incomplete",
            RouteCreateFindingCode.RecoveryUnavailable => "route-create.recovery-unavailable",
            RouteCreateFindingCode.RecoveryArtifactRetained => "route-create.recovery-artifact-retained",
            RouteCreateFindingCode.TargetChangedDuringApply => "route-create.target-changed-during-apply",
            RouteCreateFindingCode.WriteFailed => "route-create.write-failed",
            RouteCreateFindingCode.VerificationFailed => "route-create.verification-failed",
            RouteCreateFindingCode.RecoveryFailed => "route-create.recovery-failed",
            RouteCreateFindingCode.OperationFailed => "route-create.operation-failed",
            RouteCreateFindingCode.Interrupted => "route-create.interrupted",
            _ => Undefined(nameof(code), code),
        };

    internal static CliSemanticStatus ReadStatus(RouteCreateFindingCode code)
        => code switch
        {
            RouteCreateFindingCode.InvalidInput
                or RouteCreateFindingCode.InvalidTarget
                or RouteCreateFindingCode.InvalidMetadata
                or RouteCreateFindingCode.InvalidTemplate => CliSemanticStatus.Invalid,
            RouteCreateFindingCode.WorkspaceUnavailable
                or RouteCreateFindingCode.WorkspaceUnsafe
                or RouteCreateFindingCode.TargetUnsafe
                or RouteCreateFindingCode.TargetContentDiffers
                or RouteCreateFindingCode.ParentMissing
                or RouteCreateFindingCode.RouteAmbiguous
                or RouteCreateFindingCode.IdentityCollision
                or RouteCreateFindingCode.TemplateUnsafe
                or RouteCreateFindingCode.MetadataUnsafe
                or RouteCreateFindingCode.GeneratedRegionUnsafe
                or RouteCreateFindingCode.WorkspaceLockUnavailable
                or RouteCreateFindingCode.TargetChanged
                or RouteCreateFindingCode.RecoveryConflict => CliSemanticStatus.Blocked,
            RouteCreateFindingCode.InspectionIncomplete
                or RouteCreateFindingCode.TemplateUnavailable
                or RouteCreateFindingCode.MetadataIncomplete
                or RouteCreateFindingCode.ProjectionIncomplete
                or RouteCreateFindingCode.RecoveryUnavailable => CliSemanticStatus.Incomplete,
            RouteCreateFindingCode.RecoveryArtifactRetained => CliSemanticStatus.Attention,
            RouteCreateFindingCode.TargetChangedDuringApply
                or RouteCreateFindingCode.WriteFailed
                or RouteCreateFindingCode.VerificationFailed
                or RouteCreateFindingCode.RecoveryFailed
                or RouteCreateFindingCode.OperationFailed => CliSemanticStatus.Failed,
            RouteCreateFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Route Create finding code is not defined."),
        };

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        IReadOnlyList<RouteCreateFinding> findings)
    {
        if (Contains(findings, RouteCreateFindingCode.ParentMissing))
        {
            return new CliNextAction(
                RouteInitCommand,
                "Initialize the missing parent route, then rerun Route Create.");
        }

        if (Contains(findings, RouteCreateFindingCode.TargetContentDiffers))
        {
            return new CliNextAction(
                RouteUpdateCommand,
                "Update the existing route explicitly, then rerun Route Create if needed.");
        }

        if (Contains(findings, RouteCreateFindingCode.RecoveryArtifactRetained))
        {
            return new CliNextAction(
                CleanupCommand,
                "Review and remove the reported recovery artifact after confirming the verified Route Create result.");
        }

        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                RouteCreateHelpCommand,
                "Correct the named Route Create input, then rerun the request."),
            CliSemanticStatus.Blocked when Contains(findings, RouteCreateFindingCode.WorkspaceLockUnavailable)
                || Contains(findings, RouteCreateFindingCode.TargetChanged) => new CliNextAction(
                    RouteCreateCommand,
                    "Wait for the blocking condition or inspect the changed target, then rerun Route Create from a fresh plan."),
            CliSemanticStatus.Blocked => new CliNextAction(
                DoctorCommand,
                "Inspect the blocked workspace, route, identity, metadata, generated-region, Template, or recovery boundary before rerunning Route Create."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                DoctorCommand,
                "Inspect the unavailable route, Template, metadata, projection, or recovery facts before relying on this Route Create result."),
            CliSemanticStatus.Attention => new CliNextAction(
                CleanupCommand,
                "Review and remove the reported recovery artifact after confirming the verified Route Create result."),
            CliSemanticStatus.Failed => new CliNextAction(
                VerboseRouteCreateCommand,
                "Report the failure and retry the same Route Create request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                RouteCreateCommand,
                "Rerun the same Route Create request."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Route Create status is not defined."),
        };
    }

    private static bool Contains(
        IReadOnlyList<RouteCreateFinding> findings,
        RouteCreateFindingCode code)
        => findings.Any(finding => finding.Code == code);

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            $"The Route Create {name} value is not defined.");
}
