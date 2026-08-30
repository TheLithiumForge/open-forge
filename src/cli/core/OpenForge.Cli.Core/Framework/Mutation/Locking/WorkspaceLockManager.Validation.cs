using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Locking;

internal sealed partial class WorkspaceLockManager
{
    private PhysicalPathResolution Resolve(
        WorkspaceLockRequest request,
        string path)
        => _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            path);

    private static WorkspaceLockResult? ValidateDirectory(
        string logicalPath,
        PhysicalPathResolution resolution)
    {
        if (resolution.State != PhysicalPathState.Contained)
        {
            return FromResolution(
                resolution,
                "The workspace lock directory is unsafe or unavailable.");
        }

        try
        {
            var logicalAttributes = File.GetAttributes(logicalPath);
            var physicalAttributes = File.GetAttributes(resolution.GetContainedPhysicalPath());
            var logicalIsReparsePoint = (logicalAttributes & FileAttributes.ReparsePoint) != 0;
            var isInternalDirectoryAlias = logicalIsReparsePoint
                && string.Equals(
                    Path.GetFileName(Path.TrimEndingDirectorySeparator(logicalPath)),
                    WorkspaceLockRequest.DirectoryName,
                    StringComparison.Ordinal);
            return (logicalAttributes & FileAttributes.Directory) != 0
                && (logicalAttributes & FileAttributes.Device) == 0
                && (!logicalIsReparsePoint || isInternalDirectoryAlias)
                && (physicalAttributes & FileAttributes.Directory) != 0
                && (physicalAttributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0
                ? null
                : WorkspaceLockResult.Failed(
                    new FilesystemFailure(
                        FilesystemFailureKind.InvalidPath,
                        "The workspace lock directory path is not an ordinary directory."));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Failed(FilesystemFailureKind.AccessDenied, exception);
        }
        catch (IOException exception)
        {
            return Failed(FilesystemFailureKind.InputOutput, exception);
        }
    }

    private static WorkspaceLockResult? ValidateLockTarget(
        string logicalPath,
        string physicalPath,
        WorkspaceLockBootstrapOutcome bootstrapOutcome)
    {
        try
        {
            var logicalAttributes = File.GetAttributes(logicalPath);
            var physicalAttributes = File.GetAttributes(physicalPath);
            return (logicalAttributes
                    & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0
                && (physicalAttributes
                    & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0
                ? null
                : WorkspaceLockResult.Failed(
                    new FilesystemFailure(
                        FilesystemFailureKind.InvalidPath,
                        "The workspace lock path is not an ordinary file."),
                    bootstrapOutcome);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Failed(
                FilesystemFailureKind.AccessDenied,
                exception,
                bootstrapOutcome);
        }
        catch (IOException exception)
        {
            return Failed(
                FilesystemFailureKind.InputOutput,
                exception,
                bootstrapOutcome);
        }
    }

    private static WorkspaceLockResult FromResolution(
        PhysicalPathResolution resolution,
        string cause,
        WorkspaceLockBootstrapOutcome? bootstrapOutcome = null)
    {
        if (resolution.Failure is not null)
        {
            return WorkspaceLockResult.Failed(
                resolution.Failure,
                bootstrapOutcome);
        }

        return WorkspaceLockResult.Failed(
            new FilesystemFailure(FilesystemFailureKind.InvalidPath, cause),
            bootstrapOutcome);
    }

    private static WorkspaceLockResult Failed(
        FilesystemFailureKind kind,
        Exception exception,
        WorkspaceLockBootstrapOutcome? bootstrapOutcome = null)
        => WorkspaceLockResult.Failed(
            FilesystemFailure.FromException(kind, exception),
            bootstrapOutcome);
}
