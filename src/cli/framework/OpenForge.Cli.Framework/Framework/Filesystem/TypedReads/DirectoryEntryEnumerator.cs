using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

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
