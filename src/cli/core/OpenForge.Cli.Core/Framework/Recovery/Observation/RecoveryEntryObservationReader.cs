using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

namespace OpenForge.Cli.Core.Framework.Recovery.Observation;

internal static class RecoveryEntryObservationReader
{
    // The production reader must guard the parent chain and final leaf, and
    // independently obtain ordinary content identity only after no-follow
    // ordinary admission. Revalidate the same target around that read. Never
    // resolve or open a link target; retain raw relative link identity instead.
    internal static async ValueTask<RecoveryEntryComparisonInput> ReadAsync(
        PhysicalPathResolver physicalPathResolver,
        RecoveryEntryComparisonContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        var leaf = NoFollowLeafObserver.Observe(
            physicalPathResolver,
            context.Workspace,
            context.LogicalPath,
            cancellationToken);
        if (leaf.State != NoFollowLeafState.OrdinaryFile)
        {
            return new RecoveryEntryComparisonInput(context, leaf, ordinaryContent: null);
        }

        RecoveryContentIdentity? identity = null;
        FilesystemFailure? failure = null;
        try
        {
            var admitted = NoFollowLeafObserver.Observe(
                physicalPathResolver,
                context.Workspace,
                context.LogicalPath,
                cancellationToken);
            if (admitted.State != NoFollowLeafState.OrdinaryFile)
            {
                return new RecoveryEntryComparisonInput(
                    context,
                    admitted,
                    ordinaryContent: null);
            }

            var logicalParent = Path.GetDirectoryName(context.LogicalPath)
                ?? throw new InvalidOperationException(
                    "An ordinary recovery target requires a parent directory.");
            var resolution = physicalPathResolver.ResolveCandidate(
                context.Workspace.LexicalRoot,
                context.Workspace.PhysicalRoot,
                logicalParent);
            if (resolution.State != PhysicalPathState.Contained)
            {
                var changed = NoFollowLeafObserver.Observe(
                    physicalPathResolver,
                    context.Workspace,
                    context.LogicalPath,
                    cancellationToken);
                return changed.State == NoFollowLeafState.OrdinaryFile
                    ? new RecoveryEntryComparisonInput(
                        context,
                        changed,
                        new RecoveryOrdinaryContentObservation(
                            context.LogicalPath,
                            identity: null,
                            resolution.Failure ?? new FilesystemFailure(
                                FilesystemFailureKind.InputOutput,
                                "The ordinary recovery target could not be resolved for content observation.")))
                    : new RecoveryEntryComparisonInput(context, changed, ordinaryContent: null);
            }

            var physicalPath = Path.Combine(
                resolution.GetContainedPhysicalPath(),
                Path.GetFileName(context.LogicalPath));
            if (!PhysicalContainment.Contains(
                    context.Workspace.PhysicalRoot,
                    physicalPath))
            {
                return new RecoveryEntryComparisonInput(
                    context,
                    admitted,
                    new RecoveryOrdinaryContentObservation(
                        context.LogicalPath,
                        identity: null,
                        new FilesystemFailure(
                            FilesystemFailureKind.InvalidPath,
                            "The ordinary recovery target has no contained no-follow physical leaf.")));
            }

            var bytes = await File.ReadAllBytesAsync(
                physicalPath,
                cancellationToken).ConfigureAwait(false);
            identity = RecoveryContentIdentity.FromBytes(bytes);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            failure = FilesystemFailure.FromException(
                FilesystemFailureKind.AccessDenied,
                exception);
        }
        catch (Exception exception) when (exception is FileNotFoundException
            or DirectoryNotFoundException
            or IOException)
        {
            failure = FilesystemFailure.FromException(
                FilesystemFailureKind.InputOutput,
                exception);
        }

        var confirmed = NoFollowLeafObserver.Observe(
            physicalPathResolver,
            context.Workspace,
            context.LogicalPath,
            cancellationToken);
        if (confirmed.State != NoFollowLeafState.OrdinaryFile)
        {
            return new RecoveryEntryComparisonInput(
                context,
                confirmed,
                ordinaryContent: null);
        }

        return new RecoveryEntryComparisonInput(
            context,
            confirmed,
            new RecoveryOrdinaryContentObservation(
                context.LogicalPath,
                identity,
                failure));
    }
}
