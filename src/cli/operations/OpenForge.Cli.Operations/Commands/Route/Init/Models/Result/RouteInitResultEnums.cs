namespace OpenForge.Cli.Core.Commands.Route.Init.Models.Result;

internal enum RouteInitPlanCompleteness
{
    NotEstablished,
    Incomplete,
    Complete,
}

internal enum RouteInitPlanSafety
{
    NotEstablished,
    Safe,
    Blocked,
}

internal enum RouteInitFrameworkSegmentRole
{
    InstalledRoot,
    Managed,
    Scope,
}

internal enum RouteInitEntrypointForm
{
    Canonical,
    Compatibility,
}

internal enum RouteInitEntrypointCurrent
{
    Existing,
    Missing,
}

internal enum RouteInitEntrypointOwnership
{
    User,
    Framework,
}

internal enum RouteInitDescriptionSource
{
    Draft,
    Explicit,
    Embedded,
}

internal enum RouteInitResponsibilitySource
{
    DefaultOmitted,
    ExplicitOmitted,
    Explicit,
    Embedded,
}

internal enum RouteInitTagsSource
{
    Draft,
    Explicit,
    Mixed,
    Embedded,
}

internal enum RouteInitEntrypointOutcome
{
    Unchanged,
    Planned,
    NotStarted,
    Created,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteInitEffectKind
{
    Directory,
    Entrypoint,
    GeneratedRegion,
}

internal enum RouteInitEffectAction
{
    Create,
    Replace,
}

internal enum RouteInitEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteInitEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum RouteInitLifecycleAction
{
    None,
    Preserve,
    Publish,
}

internal enum RouteInitLifecycleOutcome
{
    NotRequested,
    Planned,
    AlreadyCurrent,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteInitRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum RouteInitVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal enum RouteInitFindingCode
{
    InvalidInput,
    InvalidTarget,
    InvalidMetadata,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    TargetUnsafe,
    RouteAmbiguous,
    IdentityCollision,
    LoaderUnsafe,
    FrameworkPayloadInvalid,
    FrameworkInstallRequired,
    FrameworkUpdateRequired,
    FrameworkAlignmentBlocked,
    MetadataUnsafe,
    GeneratedRegionUnsafe,
    LifecycleBlocked,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    FrameworkPayloadUnavailable,
    InspectionIncomplete,
    MetadataIncomplete,
    ProjectionIncomplete,
    LifecycleUnavailable,
    RecoveryUnavailable,
    NeedsAuthoring,
    RecoveryArtifactRetained,
    TargetChangedDuringApply,
    WriteFailed,
    VerificationFailed,
    LifecyclePublicationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}
