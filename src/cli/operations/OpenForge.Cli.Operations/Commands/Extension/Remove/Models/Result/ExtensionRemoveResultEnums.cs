namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

internal enum ExtensionRemoveFindingCode
{
    InvalidInput,
    SelectionRequired,
    InteractionEnded,
    ConfirmationRequired,
    FrameworkUnavailable,
    FrameworkUnsafe,
    LifecycleUnavailable,
    LifecycleBlocked,
    DependencyBlocked,
    OwnershipConflict,
    PermissionRequired,
    PermissionDeclined,
    PermissionsInvalid,
    PermissionsUnavailable,
    PermissionsChanged,
    PermissionWriteFailed,
    TargetUnsafe,
    ProjectionUnavailable,
    GeneratedRegionUnsafe,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryConflict,
    RecoveryUnavailable,
    LifecycleObservation,
    OwnershipObservation,
    RecoveryArtifactRetained,
    WriteFailed,
    TopologyVerificationFailed,
    LifecyclePublicationFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}

internal enum ExtensionRemovePathClassification
{
    Shared,
    FinalOwner,
    Missing,
}

internal enum ExtensionRemovePathAction
{
    RetainShared,
    Delete,
    ReleaseOwnership,
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
