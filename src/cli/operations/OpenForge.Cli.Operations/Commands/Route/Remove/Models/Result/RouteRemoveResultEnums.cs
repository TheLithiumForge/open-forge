namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

internal enum RouteRemoveSourceSelection
{
    SourceId,
    BasePath,
    OverwritePath,
}

internal enum RouteRemoveSourceForm
{
    OrdinaryMarkdown,
    CanonicalEntrypoint,
    CompatibilityEntrypoint,
}

internal enum RouteRemoveSubjectKind
{
    Leaf,
    Category,
}

internal enum RouteRemoveLayerKind
{
    Base,
    Overwrite,
}

internal enum RouteRemoveItemKind
{
    Directory,
    Entrypoint,
    RoutedMarkdown,
    UnroutedMarkdown,
    NativeSource,
    Resource,
}

internal enum RouteRemoveOwnershipState
{
    NotEstablished,
    Unmanaged,
    Claimed,
    Blocked,
    Interrupted,
}

internal enum RouteRemoveOwnershipTrust
{
    NotEstablished,
    Trusted,
    Blocked,
    Interrupted,
}

internal enum RouteRemoveOwnershipManager
{
    Framework,
    Extension,
}

internal enum RouteRemoveCoverage
{
    NotEstablished,
    Incomplete,
    Complete,
    Blocked,
    Interrupted,
}

internal enum RouteRemoveGeneratedReason
{
    OldParent,
    Loader,
}

internal enum RouteRemoveGeneratedState
{
    Changed,
    Unchanged,
}

internal enum RouteRemovePlanCompleteness
{
    NotEstablished,
    Incomplete,
    Complete,
}

internal enum RouteRemovePlanSafety
{
    NotEstablished,
    Safe,
    Blocked,
}

internal enum RouteRemoveEffectKind
{
    Directory,
    RemovedFile,
    ReferenceSource,
    GeneratedRegion,
}

internal enum RouteRemoveEffectAction
{
    Replace,
    Delete,
}

internal enum RouteRemovePathStateKind
{
    Missing,
    File,
    Directory,
}

internal enum RouteRemoveEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum RouteRemoveEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum RouteRemoveRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum RouteRemovePersistenceOutcome
{
    NotEstablished,
    Planned,
    Applied,
    Unchanged,
    NotStarted,
    Failed,
    Unknown,
}

internal enum RouteRemoveVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal enum RouteRemoveFindingCode
{
    InvalidInput,
    ConfirmationRequired,
    InvalidSource,
    SourceNotFound,
    InvalidSubject,
    WorkspaceUnsafe,
    SourceUnsafe,
    RouteAmbiguous,
    IdentityCollision,
    OverwriteAmbiguous,
    CategoryUnsafe,
    OwnershipUnavailable,
    OwnershipClaimed,
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
    SettingsUnavailable,
    ProtectedTarget,
}
