namespace OpenForge.Cli.Core.Commands.Route.Create.Models.Result;

internal enum RouteCreateParentForm
{
    Canonical,
    Compatibility,
}

internal enum RouteCreateTemplateClassification
{
    Template,
}

internal enum RouteCreatePlanCompleteness
{
    NotEstablished,
    Incomplete,
    Complete,
}

internal enum RouteCreatePlanSafety
{
    NotEstablished,
    Safe,
    Blocked,
}

internal enum RouteCreateEffectKind
{
    Directory,
    Entrypoint,
    RoutedFile,
    GeneratedRegion,
}

internal enum RouteCreateEffectAction
{
    Create,
    Replace,
}

internal enum RouteCreateEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteCreateEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum RouteCreateRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum RouteCreateVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal enum RouteCreateFindingCode
{
    InvalidInput,
    InvalidTarget,
    InvalidMetadata,
    InvalidTemplate,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    TargetUnsafe,
    TargetContentDiffers,
    ParentMissing,
    RouteAmbiguous,
    IdentityCollision,
    TemplateUnsafe,
    MetadataUnsafe,
    GeneratedRegionUnsafe,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    InspectionIncomplete,
    TemplateUnavailable,
    MetadataIncomplete,
    ProjectionIncomplete,
    RecoveryUnavailable,
    RecoveryArtifactRetained,
    OptionalMetadata,
    TargetChangedDuringApply,
    WriteFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}
