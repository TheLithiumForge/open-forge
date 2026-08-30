using OpenForge.Cli.Core.Framework.Filesystem;

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
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Lease = lease;
        Failure = failure;
        Cause = cause;
    }

    internal WorkspaceLockState State { get; }

    internal WorkspaceLockLease? Lease { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static WorkspaceLockResult Acquired(WorkspaceLockLease lease)
    {
        if (!lease.IsHeld)
        {
            throw new ArgumentException("An acquired lock result requires a held lease.", nameof(lease));
        }

        return new WorkspaceLockResult(
            state: WorkspaceLockState.Acquired,
            lease: lease,
            failure: null,
            cause: null);
    }

    internal static WorkspaceLockResult Failed(FilesystemFailure failure)
    {
        return new WorkspaceLockResult(
            state: WorkspaceLockState.Failed,
            lease: null,
            failure: failure,
            cause: failure.DirectCause);
    }

    internal static WorkspaceLockResult Cancelled()
        => new(
            state: WorkspaceLockState.Cancelled,
            lease: null,
            failure: null,
            cause: null);
}
