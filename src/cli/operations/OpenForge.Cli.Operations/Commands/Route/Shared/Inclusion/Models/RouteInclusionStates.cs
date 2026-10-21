namespace OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;

internal enum RouteInclusionFindingCode
{
    InvalidInput,
    WorkspaceUnsafe,
    Interrupted,
    WorkspaceLockUnavailable,
    TargetChanged,
    RecoveryUnavailable,
    RecoveryConflict,
    WriteFailed,
    VerificationFailed,
    RecoveryFailed,
    RecoveryArtifactRetained,
}

internal enum RouteInclusionRecoveryState
{
    NotRequired,
    NotCreated,
    Removed,
    Retained,
    Unknown,
}
