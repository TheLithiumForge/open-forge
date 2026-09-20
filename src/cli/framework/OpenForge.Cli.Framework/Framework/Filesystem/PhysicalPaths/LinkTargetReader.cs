using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal static class LinkTargetReader
{
    internal static PathComponent Read(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        try
        {
            var target = ReadImmediateTarget(path);
            if (target is not null)
            {
                return new PathComponent(PathComponentState.Link, path, target, null, null);
            }

            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                return new PathComponent(
                    PathComponentState.Unsupported,
                    path,
                    null,
                    attributes,
                    new FilesystemFailure(
                        FilesystemFailureKind.Unsupported,
                        "The reparse point does not expose an immediate managed link target."));
            }

            return new PathComponent(PathComponentState.Ordinary, path, null, attributes, null);
        }
        catch (FileNotFoundException)
        {
            return new PathComponent(PathComponentState.Missing, path, null, null, null);
        }
        catch (DirectoryNotFoundException)
        {
            return new PathComponent(PathComponentState.Missing, path, null, null, null);
        }
        catch (UnauthorizedAccessException exception)
        {
            return new PathComponent(
                PathComponentState.Inaccessible,
                path,
                null,
                null,
                FilesystemFailure.FromException(FilesystemFailureKind.AccessDenied, exception));
        }
        catch (IOException exception)
        {
            return new PathComponent(
                PathComponentState.InputOutputFailure,
                path,
                null,
                null,
                FilesystemFailure.FromException(FilesystemFailureKind.InputOutput, exception));
        }
    }

    private static string? ReadImmediateTarget(string path)
    {
        var directoryTarget = new DirectoryInfo(path).LinkTarget;
        if (directoryTarget is not null)
        {
            return directoryTarget;
        }

        return new FileInfo(path).LinkTarget;
    }
}
