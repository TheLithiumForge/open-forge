using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

internal static class StrictUtf8FileReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static async ValueTask<FileReadResult<string>> ReadAsync(
        string physicalPath,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            return FileReadResult<string>.Complete(logicalPath, StrictUtf8.GetString(bytes));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FileReadResult<string>.Cancelled(logicalPath);
        }
        catch (FileNotFoundException)
        {
            return FileReadResult<string>.Missing(logicalPath);
        }
        catch (DirectoryNotFoundException)
        {
            return FileReadResult<string>.Missing(logicalPath);
        }
        catch (DecoderFallbackException exception)
        {
            return FileReadResult<string>.Failed(
                FileReadState.InvalidEncoding,
                logicalPath,
                FilesystemFailure.FromException(FilesystemFailureKind.InvalidEncoding, exception));
        }
        catch (UnauthorizedAccessException exception)
        {
            return FileReadResult<string>.Failed(
                FileReadState.AccessDenied,
                logicalPath,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (IOException exception)
        {
            return FileReadResult<string>.Failed(
                FileReadState.InputOutputFailure,
                logicalPath,
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
        }
    }
}
