using OpenForge.Cli.Core.Framework.Filesystem;

namespace OpenForge.Cli.Core.Framework.Lifecycle.Models;

internal enum LifecycleManagedTargetReadState
{
    Available,
    Missing,
    Unavailable,
    Blocked,
    Cancelled,
}

internal sealed record LifecycleManagedTargetReadResult
{
    private LifecycleManagedTargetReadResult(
        LifecycleManagedTargetReadState state,
        ReadOnlyMemory<byte> bytes,
        FilesystemFailure? failure)
    {
        var coherent = state switch
        {
            LifecycleManagedTargetReadState.Available => failure is null,
            LifecycleManagedTargetReadState.Missing
                or LifecycleManagedTargetReadState.Cancelled => bytes.IsEmpty && failure is null,
            LifecycleManagedTargetReadState.Unavailable => bytes.IsEmpty && failure is not null,
            LifecycleManagedTargetReadState.Blocked => bytes.IsEmpty,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The lifecycle managed-target read state is not defined."),
        };
        if (!coherent)
        {
            throw new ArgumentException(
                "Lifecycle managed-target read details do not match their state.");
        }

        State = state;
        Bytes = bytes;
        Failure = failure;
    }

    internal LifecycleManagedTargetReadState State { get; }

    internal ReadOnlyMemory<byte> Bytes { get; }

    internal FilesystemFailure? Failure { get; }

    internal static LifecycleManagedTargetReadResult Available(byte[] bytes)
        => new(LifecycleManagedTargetReadState.Available, bytes, failure: null);

    internal static LifecycleManagedTargetReadResult Missing()
        => new(LifecycleManagedTargetReadState.Missing, ReadOnlyMemory<byte>.Empty, failure: null);

    internal static LifecycleManagedTargetReadResult Unavailable(FilesystemFailure failure)
        => new(LifecycleManagedTargetReadState.Unavailable, ReadOnlyMemory<byte>.Empty, failure);

    internal static LifecycleManagedTargetReadResult Blocked(FilesystemFailure? failure)
        => new(LifecycleManagedTargetReadState.Blocked, ReadOnlyMemory<byte>.Empty, failure);

    internal static LifecycleManagedTargetReadResult Cancelled()
        => new(LifecycleManagedTargetReadState.Cancelled, ReadOnlyMemory<byte>.Empty, failure: null);
}
