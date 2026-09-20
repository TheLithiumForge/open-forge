using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;

namespace OpenForge.Cli.Core.Presentation.Route.Create.Shared.Selection;

internal static class RouteCreateWireVocabulary
{
    internal const string TemplateReferenceValueName = "template-reference";
    internal const string RouteCreateHelpCommand = "open-forge route create --help";
    internal const string VerboseRouteCreateCommand = "open-forge route create --detail debug";

    internal static string Name(RouteCreateMode mode)
        => mode switch
        {
            RouteCreateMode.Apply => "apply",
            RouteCreateMode.DryRun => "dry-run",
            _ => throw Undefined(nameof(mode), mode),
        };

    internal static string Name(RouteCreatePlanCompleteness completeness)
        => completeness switch
        {
            RouteCreatePlanCompleteness.NotEstablished => "not-established",
            RouteCreatePlanCompleteness.Incomplete => "incomplete",
            RouteCreatePlanCompleteness.Complete => "complete",
            _ => throw Undefined(nameof(completeness), completeness),
        };

    internal static string Name(RouteCreatePlanSafety safety)
        => safety switch
        {
            RouteCreatePlanSafety.NotEstablished => "not-established",
            RouteCreatePlanSafety.Safe => "safe",
            RouteCreatePlanSafety.Blocked => "blocked",
            _ => throw Undefined(nameof(safety), safety),
        };

    internal static string Name(RouteCreateEffectKind kind)
        => kind switch
        {
            RouteCreateEffectKind.Directory => "directory",
            RouteCreateEffectKind.Entrypoint => "entrypoint",
            RouteCreateEffectKind.RoutedFile => "routed-file",
            RouteCreateEffectKind.GeneratedRegion => "generated-region",
            _ => throw Undefined(nameof(kind), kind),
        };

    internal static string Name(RouteCreateEffectAction action)
        => action switch
        {
            RouteCreateEffectAction.Create => "create",
            RouteCreateEffectAction.Replace => "replace",
            _ => throw Undefined(nameof(action), action),
        };

    internal static string Name(RouteCreateEffectOutcome outcome)
        => outcome switch
        {
            RouteCreateEffectOutcome.Planned => "planned",
            RouteCreateEffectOutcome.NotStarted => "not-started",
            RouteCreateEffectOutcome.Verified => "verified",
            RouteCreateEffectOutcome.VerificationFailed => "verification-failed",
            RouteCreateEffectOutcome.CompletionUnknown => "completion-unknown",
            _ => throw Undefined(nameof(outcome), outcome),
        };

    internal static string Name(RouteCreateRecoveryState state)
        => state switch
        {
            RouteCreateRecoveryState.NotRequired => "not-required",
            RouteCreateRecoveryState.NotCreated => "not-created",
            RouteCreateRecoveryState.Removed => "removed",
            RouteCreateRecoveryState.Retained => "retained",
            RouteCreateRecoveryState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static string Name(RouteCreateVerificationState state)
        => state switch
        {
            RouteCreateVerificationState.NotRequested => "not-requested",
            RouteCreateVerificationState.Verified => "verified",
            RouteCreateVerificationState.Failed => "failed",
            RouteCreateVerificationState.Unknown => "unknown",
            _ => throw Undefined(nameof(state), state),
        };

    internal static string Name(RouteCreateFindingCode code)
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
            RouteCreateFindingCode.OptionalMetadata => "route-create.optional-metadata",
            RouteCreateFindingCode.TargetChangedDuringApply => "route-create.target-changed-during-apply",
            RouteCreateFindingCode.WriteFailed => "route-create.write-failed",
            RouteCreateFindingCode.VerificationFailed => "route-create.verification-failed",
            RouteCreateFindingCode.RecoveryFailed => "route-create.recovery-failed",
            RouteCreateFindingCode.OperationFailed => "route-create.operation-failed",
            RouteCreateFindingCode.Interrupted => "route-create.interrupted",
            _ => throw Undefined(nameof(code), code),
        };

    private static ArgumentOutOfRangeException Undefined<T>(string name, T value)
        where T : struct, Enum
        => new(name, value, $"The Route Create {name} value is not defined.");
}
