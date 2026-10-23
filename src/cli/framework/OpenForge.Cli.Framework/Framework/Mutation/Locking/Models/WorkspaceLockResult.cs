using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal enum WorkspaceLockState
{
    Acquired,
    Failed,
    Cancelled,
}

internal sealed record WorkspaceLockResult
{
    private WorkspaceLockResult(
        WorkspaceLockState state,
        WorkspaceLockLease? lease,
        WorkspaceLockFailure? lockFailure)
    {
        State = state;
        Lease = lease;
        LockFailure = lockFailure;
    }

    internal WorkspaceLockState State { get; }
    internal WorkspaceLockLease? Lease { get; }
    internal WorkspaceLockFailure? LockFailure { get; }

    internal FilesystemFailure? Failure => LockFailure is null
        ? null
        : LockFailure.Detail ?? new FilesystemFailure(
            LockFailure.Kind switch
            {
                WorkspaceLockFailureKind.AccessDenied => FilesystemFailureKind.AccessDenied,
                WorkspaceLockFailureKind.Unsupported => FilesystemFailureKind.Unsupported,
                WorkspaceLockFailureKind.InvalidTarget => FilesystemFailureKind.InvalidPath,
                WorkspaceLockFailureKind.Busy or WorkspaceLockFailureKind.IoError
                    or WorkspaceLockFailureKind.Cancelled => FilesystemFailureKind.InputOutput,
                _ => throw new ArgumentOutOfRangeException(nameof(LockFailure)),
            },
            $"Workspace lock acquisition failed: {LockFailure.Kind}.");

    internal string? Cause => Failure?.DirectCause;

    internal static WorkspaceLockResult Acquired(WorkspaceLockLease lease)
    {
        if (!lease.IsHeld)
        {
            throw new ArgumentException("An acquired lock result requires a held lease.", nameof(lease));
        }

        return new(WorkspaceLockState.Acquired, lease, null);
    }

    internal static WorkspaceLockResult Failed(WorkspaceLockFailure failure)
        => new(
            failure.Kind == WorkspaceLockFailureKind.Cancelled ? WorkspaceLockState.Cancelled : WorkspaceLockState.Failed,
            lease: null,
            lockFailure: failure);

    internal static WorkspaceLockResult Cancelled(string path)
        => Failed(new WorkspaceLockFailure(WorkspaceLockFailureKind.Cancelled, path));
}
