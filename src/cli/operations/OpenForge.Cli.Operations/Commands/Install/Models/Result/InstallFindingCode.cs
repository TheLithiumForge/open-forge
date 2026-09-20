namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal enum InstallFindingCode
{
    InvalidInput,
    ConfirmationRequired,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    ManagedDivergence,
    TargetOccupied,
    OwnershipConflict,
    TargetUnsafe,
    GeneratedRegionUnsafe,
    LifecycleBlocked,
    RecoveryConflict,
    PayloadUnavailable,
    PayloadInvalid,
    LifecycleUnavailable,
    ProjectionUnavailable,
    RecoveryUnavailable,
    RecoveryArtifactRetained,
    WriteFailed,
    VerificationFailed,
    LifecyclePublicationFailed,
    RecoveryFailed,
    OperationFailed,
    Interrupted,
}
