using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;

/// <summary>
/// Reads <c>.agents/open-forge.lock.json</c> under the same no-follow leaf
/// boundary every other workspace file read uses, so a symlinked lock cannot
/// redirect the read outside the workspace.
///
/// No failure here is fatal. Every state other than <see
/// cref="WorkspaceOwnershipReadState.Complete"/> still returns an empty document,
/// which records nothing as owned, so a caller proceeds and reports rather than
/// refusing.
/// </summary>
internal static class WorkspaceOwnershipReader
{
    internal static async ValueTask<WorkspaceOwnershipRead> ReadAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(workspace);
        cancellationToken.ThrowIfCancellationRequested();

        var relative = WorkspaceOwnershipDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar);
        var logicalPath = Path.Combine(workspace.LexicalRoot, relative);
        var physicalPath = Path.Combine(workspace.PhysicalRoot, relative);

        var parentPath = Path.GetDirectoryName(logicalPath)!;
        var parent = NoFollowLeafObserver.Observe(resolver, workspace, parentPath, cancellationToken);
        if (parent.State == NoFollowLeafState.Missing)
        {
            return WorkspaceOwnershipRead.Absent(logicalPath);
        }
        if (parent.State != NoFollowLeafState.Directory)
        {
            return Failure(logicalPath, "The ownership lock parent is not an ordinary directory.");
        }

        var leaf = NoFollowLeafObserver.Observe(resolver, workspace, logicalPath, cancellationToken);
        if (leaf.State == NoFollowLeafState.Missing)
        {
            return WorkspaceOwnershipRead.Absent(logicalPath);
        }

        if (leaf.State != NoFollowLeafState.OrdinaryFile)
        {
            return Failure(
                logicalPath,
                $"{WorkspaceOwnershipDefinitions.RelativePath} is not an ordinary file.");
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            var confirmed = NoFollowLeafObserver.Observe(resolver, workspace, logicalPath, cancellationToken);
            var confirmedParent = NoFollowLeafObserver.Observe(resolver, workspace, parentPath, cancellationToken);
            if (confirmed.State != NoFollowLeafState.OrdinaryFile || confirmedParent.State != NoFollowLeafState.Directory)
            {
                return Failure(
                    logicalPath,
                    $"{WorkspaceOwnershipDefinitions.RelativePath} changed while it was being read.");
            }

            // The snapshot describes the bytes actually on disk, so an
            // unintelligible file can still be replaced safely.
            var snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes);
            var decoded = WorkspaceOwnershipCodec.Read(bytes);
            return decoded.Document is { } document
                ? new WorkspaceOwnershipRead(
                    WorkspaceOwnershipReadState.Complete, document, logicalPath, snapshot, Cause: null)
                : new WorkspaceOwnershipRead(
                    WorkspaceOwnershipReadState.Invalid,
                    WorkspaceOwnershipDocument.Empty,
                    logicalPath,
                    snapshot,
                    decoded.Cause);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return Failure(logicalPath, $"{WorkspaceOwnershipDefinitions.RelativePath} could not be read.");
        }
    }

    /// <summary>
    /// The bytes could not be read, so there is no trustworthy description of
    /// what is on disk and no safe basis for replacing it. The snapshot is null,
    /// and a writer skips the lock rather than overwriting content it cannot
    /// describe. The command decides whether its other effects can proceed.
    /// </summary>
    private static WorkspaceOwnershipRead Failure(string logicalPath, string cause)
        => new(
            WorkspaceOwnershipReadState.Unavailable,
            WorkspaceOwnershipDocument.Empty,
            logicalPath,
            Snapshot: null,
            cause);
}
