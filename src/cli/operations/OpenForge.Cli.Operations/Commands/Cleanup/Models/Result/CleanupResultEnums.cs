namespace OpenForge.Cli.Core.Commands.Cleanup.Models.Result;

internal enum CleanupPreflightState
{
    NotRequested,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal enum CleanupLeaseState
{
    NotRequested,
    Acquired,
    Failed,
    Cancelled,
}

internal enum CleanupCatalogueComparisonState
{
    NotRequested,
    Matched,
    Changed,
    Incomplete,
    Blocked,
    Cancelled,
}

internal enum CleanupArtifactFileKind
{
    NotEstablished,
    Ordinary,
    NonOrdinary,
}

internal enum CleanupWorkspaceAssociationState
{
    NotEstablished,
    CurrentWorkspace,
    Mismatched,
    Unavailable,
}

internal enum CleanupLeaseBoundaryState
{
    NotEstablished,
    NotRequested,
    Required,
    Held,
    Mismatched,
}

internal enum CleanupVerificationConditionState
{
    NotEstablished,
    NotRequested,
    ExactPathAndKind,
    SemanticFinal,
    Absence,
}

internal enum CleanupEffectOutcome
{
    Planned,
    NotStarted,
    Verified,
    VerificationFailed,
    CompletionUnknown,
}

internal enum CleanupEffectResidual
{
    None,
    Retained,
    Unknown,
}

internal enum CleanupVerificationState
{
    NotRequested,
    Verified,
    Failed,
    Unknown,
}

internal enum CleanupFindingCode
{
    InvalidInput,
    WorkspaceUnavailable,
    WorkspaceNotDirectory,
    WorkspaceUnsafe,
    CatalogueIncomplete,
    RecoveryFinalMalformed,
    RecoveryFinalUnsupported,
    RecoveryFinalUnavailable,
    RecoveryDraftUnsafe,
    WorkspaceLockUnavailable,
    CatalogueChangedDuringApply,
    CandidateChangedDuringApply,
    DeletionFailed,
    VerificationFailed,
    OperationFailed,
    Interrupted,
}
