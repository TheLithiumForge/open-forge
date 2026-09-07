using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Deletion;

internal static class RecoveryDeletionStorageBoundary
{
    internal static FilesystemFailure? ReadWorkspace(CliWorkspace workspace)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return null;
        }

        return ReadDirectoryChain(RecoveryBundlePathIdentity.WorkspaceDirectory(storeRoot, workspace.PhysicalRoot));
    }

    internal static FilesystemFailure? ReadDirectoryChain(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        try
        {
            return ReadComponents(Path.GetFullPath(directory));
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException)
        {
            return FilesystemFailure.FromException(FilesystemFailureKind.InvalidPath, exception);
        }
        catch (Exception exception) when (exception is NotSupportedException or PlatformNotSupportedException)
        {
            return FilesystemFailure.FromException(FilesystemFailureKind.Unsupported, exception);
        }
    }

    private static FilesystemFailure? ReadComponents(string directory)
    {
        var chain = new Stack<string>();
        string? current = directory;
        while (!string.IsNullOrEmpty(current))
        {
            chain.Push(current);
            current = Path.GetDirectoryName(current);
        }

        foreach (var path in chain)
        {
            var component = LinkTargetReader.Read(path);
            switch (component.State)
            {
                case PathComponentState.Ordinary:
                    if (component.Attributes is not { } attributes
                        || (attributes & FileAttributes.Directory) == 0)
                    {
                        return new FilesystemFailure(FilesystemFailureKind.InvalidPath,
                            $"The recovery storage directory chain contains a non-directory: {path}");
                    }

                    break;
                case PathComponentState.Missing:
                    return null;
                case PathComponentState.Link:
                    return new FilesystemFailure(FilesystemFailureKind.InvalidPath,
                        $"The recovery storage directory chain contains a symbolic link: {path}");
                case PathComponentState.Inaccessible:
                case PathComponentState.Unsupported:
                case PathComponentState.InputOutputFailure:
                    return component.Failure
                        ?? throw new InvalidOperationException("An unavailable directory component requires its typed failure.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(directory), component.State, "The path component state is not defined.");
            }
        }

        return null;
    }
}
