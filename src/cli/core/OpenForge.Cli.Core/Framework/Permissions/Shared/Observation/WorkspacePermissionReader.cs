using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Permissions.Shared.Observation;

internal static class WorkspacePermissionReader
{
    internal static async ValueTask<WorkspacePermissionRead> ReadAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var logicalPath = Path.Combine(workspace.LexicalRoot, WorkspacePermissionDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        var physicalPath = Path.Combine(workspace.PhysicalRoot, WorkspacePermissionDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        var parentPath = Path.GetDirectoryName(physicalPath)
            ?? throw new InvalidOperationException("The permission file requires its consumer .agents parent.");
        var parent = LinkTargetReader.Read(parentPath);
        if (parent.State == PathComponentState.Missing)
        {
            return Missing(logicalPath);
        }
        if (!IsOrdinaryDirectory(parent))
        {
            return Failure(
                parent.State is PathComponentState.Inaccessible or PathComponentState.InputOutputFailure
                    ? WorkspacePermissionReadState.Unavailable
                    : WorkspacePermissionReadState.Blocked,
                "The consumer permission parent is unavailable or is not an ordinary directory.");
        }

        var leaf = NoFollowLeafObserver.Observe(resolver, workspace, logicalPath, cancellationToken);
        if (leaf.State == NoFollowLeafState.Missing)
        {
            return Missing(logicalPath);
        }
        if (leaf.State != NoFollowLeafState.OrdinaryFile)
        {
            return Failure(
                leaf.State is NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown
                    ? WorkspacePermissionReadState.Unavailable
                    : WorkspacePermissionReadState.Blocked,
                "The consumer permission leaf is unavailable or is not an ordinary file.");
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            var confirmedParent = LinkTargetReader.Read(parentPath);
            var confirmedLeaf = NoFollowLeafObserver.Observe(resolver, workspace, logicalPath, cancellationToken);
            if (!IsOrdinaryDirectory(confirmedParent) || confirmedLeaf.State != NoFollowLeafState.OrdinaryFile)
            {
                return Failure(WorkspacePermissionReadState.Blocked, "The consumer permission boundary changed during reading.");
            }
            var snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes);
            var decoded = WorkspacePermissionCodec.Read(bytes);
            return new(
                decoded.Document is null ? WorkspacePermissionReadState.Invalid : WorkspacePermissionReadState.Complete,
                decoded.Document, snapshot, decoded.Cause);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return Failure(WorkspacePermissionReadState.Unavailable, "The consumer permission file could not be read.");
        }
    }

    private static bool IsOrdinaryDirectory(PathComponent component)
        => component.State == PathComponentState.Ordinary
            && component.Attributes is { } attributes
            && (attributes & FileAttributes.Directory) != 0
            && (attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) == 0;

    private static WorkspacePermissionRead Missing(string logicalPath)
        => new(WorkspacePermissionReadState.Missing, Document: null, FileStateSnapshot.Missing(logicalPath), Cause: null);

    private static WorkspacePermissionRead Failure(WorkspacePermissionReadState state, string cause)
        => new(state, Document: null, Snapshot: null, cause);
}
