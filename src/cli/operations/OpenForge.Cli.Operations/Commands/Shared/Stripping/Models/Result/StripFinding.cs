using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;

internal enum StripFindingCode
{
    InvalidInput,
    WorkspaceUnavailable,
    DocumentUnsupported,
    FlattenCollision,
    LinkedFileSkipped,
    TargetChanged,
    WorkspaceLockUnavailable,
    ConfirmationRequired,
    PermissionDeclined,
    RecoveryUnavailable,
    WriteFailed,
    RecoveryArtifactRetained,
    Interrupted,
    OwnershipUnavailable,
    LibraryAttached,
    TargetUnavailable,
    EditedKept,
    RuntimeRetained,
}

internal enum StripFindingSeverity
{
    Error,
    Warning,
    Info,
}

internal sealed record StripFinding
{
    public required StripFindingCode Code { get; init; }

    public required StripFindingSeverity Severity { get; init; }

    public required string Message { get; init; }

    public string? Path { get; init; }

    public WorkspaceLockFailureInfo? LockFailure { get; init; }
}

internal enum StripKeptReason
{
    Edited,
}

internal sealed record StripKeptFile(string Path, StripKeptReason Reason);

internal sealed record StripCollision(string Path, ImmutableArray<string> Sources);

internal enum StripRecoveryState
{
    NotRequired,
    Removed,
    Retained,
    Unknown,
}
