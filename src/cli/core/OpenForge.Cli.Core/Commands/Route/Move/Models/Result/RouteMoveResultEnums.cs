namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

internal enum RouteMoveSourceSelection
{
    SourceId,
    BasePath,
    OverwritePath,
}

internal enum RouteMoveSourceForm
{
    OrdinaryMarkdown,
    CanonicalEntrypoint,
    CompatibilityEntrypoint,
}

internal enum RouteMoveSubjectKind
{
    Leaf,
    Category,
}

internal enum RouteMoveLayerKind
{
    Base,
    Overwrite,
}

internal enum RouteMoveItemKind
{
    Directory,
    Entrypoint,
    RoutedMarkdown,
    UnroutedMarkdown,
    NativeSource,
    Resource,
}

internal enum RouteMoveOwnershipState
{
    NotEstablished,
    Unmanaged,
    Claimed,
    Blocked,
    Interrupted,
}

internal enum RouteMoveOwnershipTrust
{
    NotEstablished,
    Trusted,
    Blocked,
    Interrupted,
}

internal enum RouteMoveOwnershipManager
{
    Framework,
    Extension,
}

internal enum RouteMoveCoverage
{
    NotEstablished,
    Incomplete,
    Complete,
    Blocked,
    Interrupted,
}

internal enum RouteMoveGeneratedReason
{
    OldParent,
    NewParent,
    Loader,
    MovedEntrypoint,
}

internal enum RouteMoveGeneratedState
{
    Changed,
    Unchanged,
}

internal enum RouteMovePlanCompleteness
{
    NotEstablished,
    Incomplete,
    Complete,
}

internal enum RouteMovePlanSafety
{
    NotEstablished,
    Safe,
    Blocked,
}

internal enum RouteMoveEffectKind
{
    Directory,
    MovedFile,
    ReferenceSource,
    GeneratedRegion,
}

internal enum RouteMoveEffectAction
{
    Create,
    Replace,
    Delete,
}

internal enum RouteMovePathStateKind
{
    Missing,
    File,
    Directory,
}

internal enum RouteMoveEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteMoveEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum RouteMoveRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum RouteMoveVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal enum RouteMoveFindingCode
{
    InvalidInput,
    InvalidSource,
    SourceNotFound,
    InvalidSubject,
    InvalidDestination,
    WorkspaceUnsafe,
    SourceUnsafe,
    RouteAmbiguous,
    IdentityCollision,
    OverwriteAmbiguous,
    CategoryUnsafe,
    OwnershipUnavailable,
    OwnershipClaimed,
    DestinationUnsafe,
    DestinationParentMissing,
    DestinationOccupied,
    SelfMove,
    DestinationInsideSource,
    ReferenceUnsafe,
    GeneratedRegionUnsafe,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    WorkspaceUnavailable,
    CategoryInventoryIncomplete,
    ReferenceCoverageIncomplete,
    ProjectionIncomplete,
    RecoveryUnavailable,
    InspectionIncomplete,
    RecoveryArtifactRetained,
    TargetChangedDuringApply,
    WriteFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}
