using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdateComparisonReader(PhysicalPathResolver physicalPathResolver)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<UpdateTargetRead> ReadAsync(
        CliWorkspace workspace,
        string relativePath,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
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
            if ((attributes & (FileAttributes.Directory
                    | FileAttributes.Device
                    | FileAttributes.ReparsePoint)) != 0)
            {
                return Blocked(relativePath, "The Update target is not an ordinary file.");
            }

            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            var confirmed = Resolve(workspace, logicalPath);
            if (confirmed.State != PhysicalPathState.Contained
                || !PhysicalIdentityTracker.PathComparer.Equals(
                    physicalPath,
                    confirmed.GetContainedPhysicalPath()))
            {
                return Blocked(
                    relativePath,
                    "The Update target changed physical identity during inspection.");
            }

            return new UpdateTargetRead(
                relativePath,
                UpdateTargetReadState.Available,
                FileStateSnapshot.File(logicalPath, physicalPath, bytes),
                Cause: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(relativePath);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return Missing(relativePath, logicalPath);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return Unavailable(relativePath, exception.Message);
        }
    }

    private PhysicalPathResolution Resolve(CliWorkspace workspace, string logicalPath)
        => _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalPath);

    private static UpdateTargetRead FromResolution(
        string relativePath,
        PhysicalPathResolution resolution)
    {
        var cause = resolution.Failure?.DirectCause
            ?? $"The Update target physical state is {resolution.State}.";
        return resolution.State is PhysicalPathState.Inaccessible
            or PhysicalPathState.InputOutputFailure
            ? Unavailable(relativePath, cause)
            : Blocked(relativePath, cause);
    }

    private static UpdateTargetRead Missing(string relativePath, string logicalPath)
        => new(
            relativePath,
            UpdateTargetReadState.Missing,
            FileStateSnapshot.Missing(logicalPath),
            Cause: null);

    private static UpdateTargetRead Blocked(string relativePath, string cause)
        => new(relativePath, UpdateTargetReadState.Blocked, Snapshot: null, cause);

    private static UpdateTargetRead Unavailable(string relativePath, string cause)
        => new(relativePath, UpdateTargetReadState.Unavailable, Snapshot: null, cause);

    private static UpdateTargetRead Cancelled(string relativePath)
        => new(relativePath, UpdateTargetReadState.Cancelled, Snapshot: null, Cause: null);
}
