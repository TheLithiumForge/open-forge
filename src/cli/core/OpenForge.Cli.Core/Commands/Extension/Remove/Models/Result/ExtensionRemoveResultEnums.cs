namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

internal enum ExtensionRemoveFindingCode
{
    InvalidInput,
    SelectionRequired,
    InteractionEnded,
    FrameworkUnavailable,
    FrameworkUnsafe,
    LifecycleUnavailable,
    LifecycleBlocked,
    DependencyBlocked,
    OwnershipConflict,
    ManagedDivergence,
    TargetOutsideAgents,
    TargetUnsafe,
    ProjectionUnavailable,
    GeneratedRegionUnsafe,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    RecoveryUnavailable,
    LifecycleObservation,
    RecoveryArtifactRetained,
    WriteFailed,
    TopologyVerificationFailed,
    LifecyclePublicationFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}

internal enum ExtensionRemoveEffectKind
{
    PackageFile,
    GeneratedRegion,
    Lifecycle,
}

internal enum ExtensionRemoveEffectAction
{
    ReleaseOwnership,
    Delete,
    Retain,
}

internal enum ExtensionRemoveEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionRemoveEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum ExtensionRemovePathClassification
{
    Shared,
    UnchangedFinalOwner,
    ChangedFinalOwner,
    Missing,
}

internal enum ExtensionRemovePathAction
{
    RetainShared,
    Delete,
    KeepAsUnmanaged,
    ReleaseOwnership,
}

internal enum ExtensionRemoveChangedContentPolicy
{
    KeepAsUnmanaged,
    Delete,
}

internal enum ExtensionRemoveGeneratedRegionState
{
    Unchanged,
    Changed,
    Unavailable,
    Blocked,
}

internal enum ExtensionRemoveLifecycleTrust
{
    NotRequested,
    Trusted,
    Unavailable,
    Blocked,
}

internal enum ExtensionRemoveLifecycleCoverage
{
    NotRequested,
    Complete,
    Incomplete,
    Blocked,
}

internal enum ExtensionRemoveLifecycleAction
{
    None,
    Preserve,
    Publish,
}

internal enum ExtensionRemoveLifecycleOutcome
{
    NotRequested,
    Planned,
    AlreadyCurrent,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionRemoveRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum ExtensionRemoveVerificationState
{
    NotRequested,
    Planned,
    Verified,
    Failed,
    Unknown,
}
