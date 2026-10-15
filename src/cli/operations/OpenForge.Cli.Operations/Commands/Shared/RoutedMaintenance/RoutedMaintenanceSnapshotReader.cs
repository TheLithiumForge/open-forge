using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

internal delegate ValueTask<FileStateSnapshot> RoutedMaintenanceSnapshotRead(
    CliWorkspace workspace, SourceLayerVerification verification, CancellationToken token);

internal sealed class RoutedMaintenanceSnapshotReader(RoutedMaintenanceSnapshotRead? read = null)
{
    internal const int MaximumConcurrentReads = 16;
    private readonly RoutedMaintenanceSnapshotRead _read = read ?? new SourceDocumentSnapshotReader().ReadAsync;

    internal async ValueTask<FileStateSnapshot?[]> ReadAsync(
        CliWorkspace workspace,
        IReadOnlyList<SourceLayerVerification> verifications,
        CancellationToken token)
    {
        var snapshots = new FileStateSnapshot?[verifications.Count];
        await Parallel.ForEachAsync(Enumerable.Range(0, verifications.Count), new ParallelOptions
        {
            MaxDegreeOfParallelism = MaximumConcurrentReads,
            CancellationToken = token,
        }, async (index, cancellation) =>
        {
            try
            {
                snapshots[index] = await _read(workspace, verifications[index], cancellation).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException)
            {
                // Unavailable sources retain the projection's existing acquisition boundary.
            }
        }).ConfigureAwait(false);
        return snapshots;
    }
}
