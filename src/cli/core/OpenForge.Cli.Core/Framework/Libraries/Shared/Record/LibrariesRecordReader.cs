using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Record;

internal static class LibrariesRecordReader
{
    internal static async ValueTask<LibrariesRecordRead> ReadAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(workspace);
        cancellationToken.ThrowIfCancellationRequested();
        var logicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            LibraryPathIdentity.RecordRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        var agents = Path.GetDirectoryName(logicalPath)
            ?? throw new InvalidOperationException("The Library record requires its .agents parent.");
        var parent = LibraryDirectoryBoundaryObserver.Observe(
            workspace,
            agents,
            cancellationToken);
        if (parent.State == NoFollowLeafState.Missing)
        {
            return new LibrariesRecordRead
            {
                State = LibrariesRecordReadState.Missing,
                Record = null,
                Snapshot = FileStateSnapshot.Missing(logicalPath),
                Cause = null,
            };
        }

        if (parent.State != NoFollowLeafState.Directory)
        {
            return Classified(
                parent.State is NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown
                    ? LibrariesRecordReadState.Unavailable
                    : LibrariesRecordReadState.Blocked,
                parent.Failure?.DirectCause
                    ?? "The consumer .agents boundary is not a real ordinary directory.");
        }

        var leaf = NoFollowLeafObserver.Observe(
            resolver,
            workspace,
            logicalPath,
            cancellationToken);
        if (leaf.State == NoFollowLeafState.Missing)
        {
            return new LibrariesRecordRead
            {
                State = LibrariesRecordReadState.Missing,
                Record = null,
                Snapshot = FileStateSnapshot.Missing(logicalPath),
                Cause = null,
            };
        }

        if (leaf.State != NoFollowLeafState.OrdinaryFile)
        {
            return Classified(
                leaf.State is NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown
                    ? LibrariesRecordReadState.Unavailable
                    : LibrariesRecordReadState.Blocked,
                leaf.Failure?.DirectCause
                    ?? "The Library record leaf is not a real ordinary file.");
        }

        var physicalPath = LibraryDirectoryBoundaryObserver.PhysicalPath(
            workspace,
            logicalPath);
        try
        {
            var bytes = await File.ReadAllBytesAsync(
                physicalPath,
                cancellationToken).ConfigureAwait(false);
            var confirmedParent = LibraryDirectoryBoundaryObserver.Observe(
                workspace,
                agents,
                cancellationToken);
            var confirmedLeaf = NoFollowLeafObserver.Observe(
                resolver,
                workspace,
                logicalPath,
                cancellationToken);
            if (confirmedParent.State != NoFollowLeafState.Directory
                || confirmedLeaf.State != NoFollowLeafState.OrdinaryFile)
            {
                return Classified(
                    LibrariesRecordReadState.Blocked,
                    "The Library record boundary changed during the read.");
            }

            var snapshot = FileStateSnapshot.File(logicalPath, physicalPath, bytes);
            var decoded = LibrariesRecordCodec.Read(bytes);
            return new LibrariesRecordRead
            {
                State = decoded.Issue == LibrariesRecordDecodeIssue.AmbiguousOwnership
                    ? LibrariesRecordReadState.Blocked
                    : decoded.State,
                Record = decoded.Record,
                Snapshot = snapshot,
                Cause = decoded.Cause,
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            return Classified(
                LibrariesRecordReadState.Unavailable,
                "The Library record is inaccessible.");
        }
        catch (IOException)
        {
            return Classified(
                LibrariesRecordReadState.Unavailable,
                "The Library record could not be read.");
        }
    }

    private static LibrariesRecordRead Classified(
        LibrariesRecordReadState state,
        string cause)
        => new()
        {
            State = state,
            Record = null,
            Snapshot = null,
            Cause = cause,
        };
}
