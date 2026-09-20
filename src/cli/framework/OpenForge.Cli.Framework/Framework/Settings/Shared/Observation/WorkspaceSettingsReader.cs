using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Observation;

/// <summary>
/// Reads <c>.agents/open-forge.json</c> under the same no-follow leaf boundary
/// every other workspace file read uses, so a symlinked settings file cannot
/// redirect the read outside the workspace.
/// </summary>
internal static class WorkspaceSettingsReader
{
    internal static async ValueTask<WorkspaceSettingsRead> ReadAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(workspace);
        cancellationToken.ThrowIfCancellationRequested();

        var relative = WorkspaceSettingsDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar);
        var logicalPath = Path.Combine(workspace.LexicalRoot, relative);
        var physicalPath = Path.Combine(workspace.PhysicalRoot, relative);

        var parentPath = Path.GetDirectoryName(physicalPath)
            ?? throw new InvalidOperationException("The settings file requires its .agents parent.");
        var parent = LinkTargetReader.Read(parentPath);
        if (parent.State != PathComponentState.Missing && !IsOrdinaryDirectory(parent))
        {
            return Failure(logicalPath, "The settings parent is unavailable or is not an ordinary directory.");
        }
        var leaf = NoFollowLeafObserver.Observe(resolver, workspace, logicalPath, cancellationToken);
        if (leaf.State == NoFollowLeafState.Missing)
        {
            return WorkspaceSettingsRead.Absent(logicalPath);
        }

        if (leaf.State != NoFollowLeafState.OrdinaryFile)
        {
            return Failure(
                logicalPath,
                $"{WorkspaceSettingsDefinitions.RelativePath} is not an ordinary file.");
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            var confirmed = NoFollowLeafObserver.Observe(resolver, workspace, logicalPath, cancellationToken);
            if (confirmed.State != NoFollowLeafState.OrdinaryFile || !IsOrdinaryDirectory(LinkTargetReader.Read(parentPath)))
            {
                return Failure(
                    logicalPath,
                    $"{WorkspaceSettingsDefinitions.RelativePath} changed while it was being read.");
            }

            var snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes);
            var decoded = WorkspaceSettingsCodec.Read(bytes);
            return decoded.Document is { } document
                ? new WorkspaceSettingsRead(
                    WorkspaceSettingsReadState.Complete, document, logicalPath, Cause: null)
                { Snapshot = snapshot }
                : new WorkspaceSettingsRead(
                    WorkspaceSettingsReadState.Invalid,
                    WorkspaceSettingsDocument.Empty,
                    logicalPath,
                    decoded.Cause)
                { Snapshot = snapshot };
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return Failure(logicalPath, $"{WorkspaceSettingsDefinitions.RelativePath} could not be read.");
        }
    }

    private static bool IsOrdinaryDirectory(PathComponent component)
        => component.State == PathComponentState.Ordinary
            && component.Attributes is { } attributes
            && (attributes & FileAttributes.Directory) != 0
            && (attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) == 0;

    private static WorkspaceSettingsRead Failure(string logicalPath, string cause)
        => new(
            WorkspaceSettingsReadState.Unavailable,
            WorkspaceSettingsDocument.Empty,
            logicalPath,
            cause);
}
