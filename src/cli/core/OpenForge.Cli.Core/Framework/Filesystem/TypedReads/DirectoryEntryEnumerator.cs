using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

internal enum DirectoryEnumerationState
{
    Complete,
    Missing,
    AccessDenied,
    InputOutputFailure,
    Cancelled,
}

internal sealed record DirectoryEnumerationResult(
    DirectoryEnumerationState State,
    string LogicalPath,
    IReadOnlyList<string>? Entries,
    FilesystemFailure? Failure)
{
    internal static DirectoryEnumerationResult Complete(string logicalPath, IEnumerable<string> entries)
    {
        ValidatePath(logicalPath);
        ArgumentNullException.ThrowIfNull(entries);
        return new DirectoryEnumerationResult(
            DirectoryEnumerationState.Complete,
            logicalPath,
            new ReadOnlyCollection<string>(entries.ToArray()),
            null);
    }

    internal static DirectoryEnumerationResult Missing(string logicalPath)
    {
        ValidatePath(logicalPath);
        return new DirectoryEnumerationResult(DirectoryEnumerationState.Missing, logicalPath, null, null);
    }

    internal static DirectoryEnumerationResult Cancelled(string logicalPath)
    {
        ValidatePath(logicalPath);
        return new DirectoryEnumerationResult(DirectoryEnumerationState.Cancelled, logicalPath, null, null);
    }

    internal static DirectoryEnumerationResult Failed(
        DirectoryEnumerationState state,
        string logicalPath,
        FilesystemFailure failure)
    {
        ValidatePath(logicalPath);
        ArgumentNullException.ThrowIfNull(failure);
        if (state is not (DirectoryEnumerationState.AccessDenied or DirectoryEnumerationState.InputOutputFailure))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The state is not a failure state.");
        }

        var expectedFailureKind = state == DirectoryEnumerationState.AccessDenied
            ? FilesystemFailureKind.AccessDenied
            : FilesystemFailureKind.InputOutput;
        if (failure.Kind != expectedFailureKind)
        {
            throw new ArgumentException(
                "The filesystem failure kind does not match the directory enumeration state.",
                nameof(failure));
        }

        return new DirectoryEnumerationResult(state, logicalPath, null, failure);
    }

    private static void ValidatePath(string logicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
    }
}

internal static class DirectoryEntryEnumerator
{
    internal static DirectoryEnumerationResult Enumerate(
        string physicalPath,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        if (cancellationToken.IsCancellationRequested)
        {
            return DirectoryEnumerationResult.Cancelled(logicalPath);
        }

        try
        {
            return DirectoryEnumerationResult.Complete(
                logicalPath,
                Directory.GetFileSystemEntries(physicalPath));
        }
        catch (Exception exception) when (exception is DirectoryNotFoundException or FileNotFoundException)
        {
            return DirectoryEnumerationResult.Missing(logicalPath);
        }
        catch (UnauthorizedAccessException exception)
        {
            return DirectoryEnumerationResult.Failed(
                DirectoryEnumerationState.AccessDenied,
                logicalPath,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (IOException exception)
        {
            return DirectoryEnumerationResult.Failed(
                DirectoryEnumerationState.InputOutputFailure,
                logicalPath,
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
        }
    }
}
