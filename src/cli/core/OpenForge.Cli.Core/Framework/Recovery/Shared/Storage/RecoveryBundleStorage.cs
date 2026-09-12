using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;

internal static class RecoveryBundleStorage
{
    internal static bool TryEnsureWriterDirectory(
        string storeRoot,
        string workspacePhysicalPath,
        out string workspaceDirectory,
        out FilesystemFailure? failure)
    {
        workspaceDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            workspacePhysicalPath);
        failure = null;
        try
        {
            _ = Directory.CreateDirectory(workspaceDirectory);
            return true;
        }
        catch (UnauthorizedAccessException exception)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception);
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.InvalidPath, exception);
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.Unsupported, exception);
        }
        catch (IOException exception)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception);
        }

        return false;
    }

    internal static bool TryGetObserverDirectory(
        string storeRoot,
        string workspacePhysicalPath,
        out string workspaceDirectory,
        out bool exists,
        out FilesystemFailure? failure)
    {
        workspaceDirectory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            workspacePhysicalPath);
        exists = false;
        if (!TryObservePath(workspaceDirectory, out var attributes, out failure))
        {
            return false;
        }

        if (attributes is null)
        {
            return true;
        }

        if ((attributes.Value & FileAttributes.Directory) == 0)
        {
            failure = new FilesystemFailure(
                FilesystemFailureKind.InvalidPath,
                "The recovery workspace storage path is not a directory.");
            return false;
        }

        exists = true;
        return true;
    }

    internal static bool TryObservePath(
        string path,
        out FileAttributes? attributes,
        out FilesystemFailure? failure)
    {
        attributes = null;
        failure = null;
        try
        {
            attributes = File.GetAttributes(path);
            return true;
        }
        catch (FileNotFoundException)
        {
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }
        catch (UnauthorizedAccessException exception)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception);
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.InvalidPath, exception);
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.Unsupported, exception);
        }
        catch (IOException exception)
        {
            failure = FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception);
        }

        return false;
    }

    internal static void ValidateOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
        {
            throw new IOException("The recovery candidate is not an ordinary file.");
        }
    }

    internal static FileStream OpenCreateNewFile(string path)
        => new(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = RecoveryBundleFormatV1.StreamBufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });

    internal static FileStream OpenReadFile(string path)
        => new(
            path,
            new FileStreamOptions
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read,
                BufferSize = RecoveryBundleFormatV1.StreamBufferSize,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });
}
