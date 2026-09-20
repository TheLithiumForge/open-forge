namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;

internal enum ExtensionUpdateFindingCode
{
    InvalidInput,
    SelectionRequired,
    InteractionEnded,
    ConfirmationRequired,
    SourceUnavailable,
    SourceInvalid,
    SourceOverlap,
    SourceIdentityConflict,
    FrameworkUnavailable,
    FrameworkUnsafe,
    LifecycleUnavailable,
    LifecycleBlocked,
    ManagedDivergence,
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
    RecoveryArtifactRetained,
    WriteFailed,
    TopologyVerificationFailed,
    LifecyclePublicationFailed,
    VerificationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}

internal enum ExtensionUpdateSourceKind
{
    Embedded,
    Package,
    Catalogue,
}

internal enum ExtensionUpdateComparisonTargetKind
{
    PackageFile,
    GeneratedRegion,
}

internal enum ExtensionUpdateComparisonFingerprintKind
{
    OpenForgeMarkdownV1,
    ExactBytes,
}

internal enum ExtensionUpdateComparisonCurrentState
{
    Missing,
    Same,
    FormatOnly,
    Changed,
    Unavailable,
    Blocked,
}

internal enum ExtensionUpdateComparisonIntendedState
{
    Same,
    Changed,
    New,
    Retired,
    Unavailable,
    Blocked,
}

internal enum ExtensionUpdateRetirementEligibility
{
    NotApplicable,
    Eligible,
    Ineligible,
    Unavailable,
    Blocked,
}

internal enum ExtensionUpdateEffectKind
{
    PackageFile,
    GeneratedRegion,
}

internal enum ExtensionUpdateEffectAction
{
    Create,
    Replace,
    Delete,
}

internal enum ExtensionUpdateChangeAction
{
    Create,
    Replace,
    Restore,
    Delete,
    Preserve,
}

internal enum ExtensionUpdateEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionUpdateEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum ExtensionUpdateGeneratedRegionState
{
    Unchanged,
    Changed,
    New,
    Retired,
    Unavailable,
    Blocked,
}

internal enum ExtensionUpdateLifecycleTrust
{
    NotRequested,
    Trusted,
    Unavailable,
    Blocked,
}

internal enum ExtensionUpdateLifecycleCoverage
{
    NotRequested,
    Complete,
    Incomplete,
    Blocked,
}

internal enum ExtensionUpdateLifecycleAction
{
    None,
    Preserve,
    Publish,
}

internal enum ExtensionUpdateLifecycleOutcome
{
    NotRequested,
    Planned,
    AlreadyCurrent,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum ExtensionUpdateRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}

internal enum ExtensionUpdateVerificationState
{
    NotRequested,
    Planned,
    Verified,
    Failed,
    Unknown,
}
