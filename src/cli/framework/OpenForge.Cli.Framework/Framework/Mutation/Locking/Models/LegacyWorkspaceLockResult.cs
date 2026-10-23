namespace OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

internal sealed record LegacyWorkspaceLockResult
{
    internal LegacyWorkspaceLockResult(
        LegacyWorkspaceLockState state,
        WorkspaceLockStoreRoot? storeRoot = null,
        string? lockPath = null,
        FileStream? handle = null,
        WorkspaceLockFailure? failure = null)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state));
        }

        var held = state is LegacyWorkspaceLockState.HeldReadWrite or LegacyWorkspaceLockState.HeldReadOnly;
        if (held && (handle is null || !handle.CanRead || handle.SafeFileHandle.IsClosed
                || handle.SafeFileHandle.IsInvalid || storeRoot is null || lockPath is null)
            || !held && handle is not null
            || (state == LegacyWorkspaceLockState.Failed) != (failure is not null))
        {
            throw new ArgumentException("The legacy lock result must carry the authority required by its state.");
        }

        State = state;
        StoreRoot = storeRoot;
        LockPath = lockPath;
        Handle = handle;
        Failure = failure;
    }

    internal LegacyWorkspaceLockState State { get; }
    internal WorkspaceLockStoreRoot? StoreRoot { get; }
    internal string? LockPath { get; }
    internal FileStream? Handle { get; }
    internal WorkspaceLockFailure? Failure { get; }
}
