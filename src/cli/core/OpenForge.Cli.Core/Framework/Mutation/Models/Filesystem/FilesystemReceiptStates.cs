namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

// These states are neutral because file changes and directory creations use the
// same effect, verification, and pre-effect stop semantics. Target-specific
// identity and result invariants remain owned by their concrete receipt types.
internal enum FilesystemEffectState
{
    NotStarted,
    Applied,
    Unknown,
}

internal enum FilesystemVerificationState
{
    NotStarted,
    Verified,
    Failed,
}

internal enum FilesystemNotStartedReason
{
    Cancelled,
    TargetChanged,
    ApplicationFailed,
    ContractRejected,
}
