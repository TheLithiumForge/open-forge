using OpenForge.Cli.Core.Framework.Filesystem.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

internal enum FileReadState
{
    Complete,
    Missing,
    InvalidEncoding,
    InvalidSyntax,
    AccessDenied,
    InputOutputFailure,
    Cancelled,
}

internal sealed class FileReadResult<T>
{
    private FileReadResult(
        FileReadState state,
        string logicalPath,
        T? value,
        FilesystemFailure? failure)
    {
        State = state;
        LogicalPath = logicalPath;
        Value = value;
        Failure = failure;
    }

    internal FileReadState State { get; }

    internal string LogicalPath { get; }

    internal T? Value { get; }

    internal FilesystemFailure? Failure { get; }

    internal static FileReadResult<T> Complete(string logicalPath, T value)
    {
        ValidatePath(logicalPath);
        ArgumentNullException.ThrowIfNull(value);
        return new FileReadResult<T>(FileReadState.Complete, logicalPath, value, null);
    }

    internal static FileReadResult<T> Missing(string logicalPath)
    {
        ValidatePath(logicalPath);
        return new FileReadResult<T>(FileReadState.Missing, logicalPath, default, null);
    }

    internal static FileReadResult<T> Failed(
        FileReadState state,
        string logicalPath,
        FilesystemFailure failure)
    {
        ValidatePath(logicalPath);
        ArgumentNullException.ThrowIfNull(failure);
        if (state is not (FileReadState.InvalidEncoding
            or FileReadState.InvalidSyntax
            or FileReadState.AccessDenied
            or FileReadState.InputOutputFailure))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state.");
        }

        var expectedFailureKind = state switch
        {
            FileReadState.InvalidEncoding => FilesystemFailureKind.InvalidEncoding,
            FileReadState.InvalidSyntax => FilesystemFailureKind.InvalidSyntax,
            FileReadState.AccessDenied => FilesystemFailureKind.AccessDenied,
            FileReadState.InputOutputFailure => FilesystemFailureKind.InputOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state."),
        };
        if (failure.Kind != expectedFailureKind)
        {
            throw new ArgumentException("The filesystem failure kind does not match the file read state.", nameof(failure));
        }

        return new FileReadResult<T>(state, logicalPath, default, failure);
    }

    internal static FileReadResult<T> Cancelled(string logicalPath)
    {
        ValidatePath(logicalPath);
        return new FileReadResult<T>(FileReadState.Cancelled, logicalPath, default, null);
    }

    private static void ValidatePath(string logicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
    }
}
