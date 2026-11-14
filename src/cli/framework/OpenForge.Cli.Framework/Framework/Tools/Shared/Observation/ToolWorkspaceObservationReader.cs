using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation;

internal sealed class ToolWorkspaceObservationReader(ToolCatalogue catalogue, IToolRecordService records, ToolObservationReader observations, string homePath)
{
    internal async Task<IReadOnlyList<ToolObservation>> ReadAsync(string workspacePath, string mainRoot, CancellationToken token)
    {
        var locations = new ToolRecordsRequest(workspacePath, homePath);
        var found = await records.ReadAsync(locations, token).ConfigureAwait(false);
        if (new[] { found.PortableState, found.MachineState, found.WorkspaceMachineState, found.RuntimeRecordsState, found.PathRecordState }
            .Any(state => state is ToolRecordReadState.Unavailable or ToolRecordReadState.Invalid))
            throw new IOException(string.Join(" ", found.Problems));
        var result = new List<ToolObservation>();
        foreach (var id in (found.Portable?.Settings.Keys ?? []).Order(StringComparer.Ordinal))
        {
            if (!catalogue.Entries.TryGetValue(id, out var entry)) throw new IOException($"The declared tool has no catalogue entry: {id}.");
            result.Add(await observations.ReadAsync(new(entry, locations, found, mainRoot), token).ConfigureAwait(false));
        }
        return result;
    }
}
