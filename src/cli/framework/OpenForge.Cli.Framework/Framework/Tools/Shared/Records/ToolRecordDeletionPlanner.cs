using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal sealed class ToolRecordDeletionPlanner(ToolPortableCodec codec, ToolRecordPaths paths, ToolRecordsResult expected)
{
    internal async Task<IReadOnlyList<ToolRecordWrite>> PlanAsync(ToolRecordWriteRequest request, CancellationToken token)
    {
        var deletions = new List<ToolRecordWrite>();
        var selected = new HashSet<string>(StringComparer.Ordinal);
        foreach (var id in request.RemovedToolIds.Order(StringComparer.Ordinal))
        {
            var path = paths.Tool(id);
            if (!selected.Add(id) || request.Machine.Any(machine => machine.ToolId == id))
                throw new ArgumentException("A tool record cannot be selected for both writing and deletion, or deleted twice.");
            if (!expected.Machine.TryGetValue(id, out var observed) || observed.ToolId != id)
                throw new IOException($"The tool record deletion has no matching observed identity: {path}.");
            codec.ValidateVersion(observed.Version);
            await CheckAsync(path, observed, ToolMachineJsonContext.Default.ToolMachineState, token).ConfigureAwait(false);
            deletions.Add(new(paths.HomePath, path, Bytes: null));
        }
        selected.Clear();
        foreach (var version in request.RemovedRuntimeVersions.Order(StringComparer.Ordinal))
        {
            codec.ValidateVersion(version);
            var path = paths.Runtime(version);
            if (!selected.Add(version) || request.Runtimes.Any(runtime => runtime.Version == version))
                throw new ArgumentException("A runtime record cannot be selected for both writing and deletion, or deleted twice.");
            if (!expected.Runtimes.TryGetValue(version, out var observed) || observed.Version != version)
                throw new IOException($"The runtime record deletion has no matching observed identity: {path}.");
            await CheckAsync(path, observed, ToolMachineJsonContext.Default.ToolNodeRuntimeRecord, token).ConfigureAwait(false);
            deletions.Add(new(paths.HomePath, path, Bytes: null));
        }
        return deletions;
    }

    private async Task CheckAsync<T>(string path, T observed, JsonTypeInfo<T> type, CancellationToken token) where T : class
    {
        var bytes = await ToolRecordFileWriter.CheckExpectedAsync(paths.HomePath, path, expected, token).ConfigureAwait(false)
            ?? throw new IOException($"A record deletion requires an observed existing file: {path}.");
        var current = ToolRecordJson.Read(bytes).Deserialize(type) ?? throw new ArgumentException("The machine record requires an object.");
        ToolMachineValidation.Validate(observed);
        ToolMachineValidation.Validate(current);
        var observedIdentity = JsonSerializer.SerializeToNode(observed, type);
        var currentIdentity = JsonSerializer.SerializeToNode(current, type);
        if (!JsonNode.DeepEquals(observedIdentity, currentIdentity))
            throw new IOException($"The record deletion identity differs from the observed record: {path}.");
    }
}
