using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallTargetReader(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<InstallTargetRead> ReadAsync(
        CliWorkspace workspace,
        string relativePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        var logicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(relativePath);
        }

        var resolution = Resolve(workspace, logicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return Missing(relativePath, logicalPath);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return FromResolution(relativePath, resolution);
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var attributes = File.GetAttributes(physicalPath);
            if ((attributes & FileAttributes.Directory) != 0
                || (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return Blocked(relativePath, "The Install target is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(
                    physicalPath,
                    cancellationToken)
                .ConfigureAwait(false);
            var confirmed = Resolve(workspace, logicalPath);
            if (confirmed.State != PhysicalPathState.Contained
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    physicalPath,
                    confirmed.GetContainedPhysicalPath()))
            {
                return Blocked(
                    relativePath,
                    "The Install target changed its resolved physical identity during inspection.");
            }

            return new InstallTargetRead
            {
                RelativePath = relativePath,
                State = InstallTargetReadState.File,
                Snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes),
                Cause = null,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(relativePath);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Missing(relativePath, logicalPath);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unavailable(relativePath, exception.Message);
        }
        catch (IOException exception)
        {
            return Unavailable(relativePath, exception.Message);
        }
    }

    private PhysicalPathResolution Resolve(CliWorkspace workspace, string logicalPath)
        => _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalPath);

    private static InstallTargetRead FromResolution(
        string relativePath,
        PhysicalPathResolution resolution)
    {
        var cause = resolution.Failure?.DirectCause
            ?? $"The Install target physical state is {resolution.State}.";
        return resolution.State is PhysicalPathState.Inaccessible
            or PhysicalPathState.InputOutputFailure
            ? Unavailable(relativePath, cause)
            : Blocked(relativePath, cause);
    }

    private static InstallTargetRead Missing(string relativePath, string logicalPath)
        => new()
        {
            RelativePath = relativePath,
            State = InstallTargetReadState.Missing,
            Snapshot = FileStateSnapshot.Missing(logicalPath),
            Cause = null,
        };

    private static InstallTargetRead Blocked(string relativePath, string cause)
        => new()
        {
            RelativePath = relativePath,
            State = InstallTargetReadState.Blocked,
            Snapshot = null,
            Cause = cause,
        };

    private static InstallTargetRead Unavailable(string relativePath, string cause)
        => new()
        {
            RelativePath = relativePath,
            State = InstallTargetReadState.Unavailable,
            Snapshot = null,
            Cause = cause,
        };

    private static InstallTargetRead Cancelled(string relativePath)
        => new()
        {
            RelativePath = relativePath,
            State = InstallTargetReadState.Cancelled,
            Snapshot = null,
            Cause = null,
        };
}
