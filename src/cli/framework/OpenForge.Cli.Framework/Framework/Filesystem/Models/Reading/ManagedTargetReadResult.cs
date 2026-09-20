using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;

internal enum ManagedTargetReadState
{
    Available,
    Missing,
    Unavailable,
    Blocked,
    Cancelled,
}

internal sealed record ManagedTargetReadResult
{
    private ManagedTargetReadResult(
        ManagedTargetReadState state,
        ReadOnlyMemory<byte> bytes,
        FilesystemFailure? failure)
    {
        var coherent = state switch
        {
            ManagedTargetReadState.Available => failure is null,
            ManagedTargetReadState.Missing
                or ManagedTargetReadState.Cancelled => bytes.IsEmpty && failure is null,
            ManagedTargetReadState.Unavailable => bytes.IsEmpty && failure is not null,
            ManagedTargetReadState.Blocked => bytes.IsEmpty,
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

    internal ManagedTargetReadState State { get; }

    internal ReadOnlyMemory<byte> Bytes { get; }

    internal FilesystemFailure? Failure { get; }

    internal static ManagedTargetReadResult Available(byte[] bytes)
        => new(ManagedTargetReadState.Available, bytes, failure: null);

    internal static ManagedTargetReadResult Missing()
        => new(ManagedTargetReadState.Missing, ReadOnlyMemory<byte>.Empty, failure: null);

    internal static ManagedTargetReadResult Unavailable(FilesystemFailure failure)
        => new(ManagedTargetReadState.Unavailable, ReadOnlyMemory<byte>.Empty, failure);

    internal static ManagedTargetReadResult Blocked(FilesystemFailure? failure)
        => new(ManagedTargetReadState.Blocked, ReadOnlyMemory<byte>.Empty, failure);

    internal static ManagedTargetReadResult Cancelled()
        => new(ManagedTargetReadState.Cancelled, ReadOnlyMemory<byte>.Empty, failure: null);
}
