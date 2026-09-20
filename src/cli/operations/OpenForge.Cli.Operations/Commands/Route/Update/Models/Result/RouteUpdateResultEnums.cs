namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

internal enum RouteUpdateTargetSelection
{
    SourceId,
    BasePath,
    OverwritePath,
}

internal enum RouteUpdateTargetForm
{
    OrdinaryMarkdown,
    CanonicalEntrypoint,
    CompatibilityEntrypoint,
}

internal enum RouteUpdatePatchState
{
    NotRequested,
    Unresolved,
    Unchanged,
    Changed,
}

internal enum RouteUpdateResponsibilityOperation
{
    NotRequested,
    Set,
    Remove,
}

internal enum RouteUpdateTemplateClassification
{
    Template,
}

internal enum RouteUpdateTemplateDecision
{
    Unresolved,
    Copied,
    AuthoredBodyProtected,
}

internal enum RouteUpdatePlanCompleteness
{
    NotEstablished,
    Incomplete,
    Complete,
}

internal enum RouteUpdatePlanSafety
{
    NotEstablished,
    Safe,
    Blocked,
}

internal enum RouteUpdateBodyState
{
    NotEstablished,
    Preserved,
    TemplateCopied,
    AuthoredBodyProtected,
}

internal enum RouteUpdateEffectKind
{
    RoutedFile,
    GeneratedRegion,
}

internal enum RouteUpdateEffectAction
{
    Replace,
}

internal enum RouteUpdatePreviewKind
{
    MetadataField,
    TemplateBody,
    GeneratedRegion,
}

internal enum RouteUpdateEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteUpdateEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum RouteUpdateRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum RouteUpdateVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal enum RouteUpdateFindingCode
{
    InvalidInput,
    InvalidTarget,
    InvalidPatch,
    InvalidTemplate,
    WorkspaceUnsafe,
    TargetUnsafe,
    RouteAmbiguous,
    IdentityCollision,
    FrontmatterUnsafe,
    MetadataPreservationUnsafe,
    TemplateUnsafe,
    GeneratedRegionUnsafe,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    WorkspaceUnavailable,
    InspectionIncomplete,
    TemplateUnavailable,
    ProjectionIncomplete,
    RecoveryUnavailable,
    TemplateBodyProtected,
    RecoveryArtifactRetained,
    TargetChangedDuringApply,
    WriteFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}
