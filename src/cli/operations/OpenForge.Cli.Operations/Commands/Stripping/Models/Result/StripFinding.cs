using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Result;

internal enum StripFindingCode
{
    InvalidInput,
    WorkspaceUnavailable,
    DocumentUnsupported,
    FlattenCollision,
    LinkedFileSkipped,
    TargetChanged,
    TargetChangedDuringApply,
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

    // A Library ID when the finding names one instead of a path.
    public string? Identifier { get; init; }

    public WorkspaceLockFailureInfo? LockFailure { get; init; }
}

internal enum StripKeptReason
{
    Edited,
    RecoveryData,
    Link,
    Unreadable,
}

internal sealed record StripKeptFile(string Path, StripKeptReason Reason);

// KeepsFile marks a destination that a file which stays in place already occupies.
internal sealed record StripCollision(string Path, ImmutableArray<string> Sources, bool KeepsFile = false)
{
    internal int Count => Sources.Length + (KeepsFile ? 1 : 0);
}

internal enum StripRecoveryState
{
    NotRequired,
    Removed,
    Retained,
    Unknown,
}
