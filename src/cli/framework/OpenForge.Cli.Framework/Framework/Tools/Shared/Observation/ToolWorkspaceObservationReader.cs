using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation;

internal sealed class ToolWorkspaceObservationReader
{
    private readonly ToolPortableCodec _codec;
    private readonly Func<string, string, CancellationToken, Task<IReadOnlyList<ToolObservation>>> _readDeclared;

    internal ToolWorkspaceObservationReader(ToolCatalogue catalogue, IToolRecordService records, ToolObservationReader observations, string homePath)
        : this(new ToolVersionService(), async (workspacePath, mainRoot, token) =>
        {
            var locations = new ToolRecordsRequest(workspacePath, homePath);
            var found = await records.ReadAsync(locations, token).ConfigureAwait(false);
            if (new[] { found.PortableState, found.MachineState, found.WorkspaceMachineState, found.RuntimeRecordsState, found.PathRecordState }
                .Any(state => state is ToolRecordReadState.Unavailable or ToolRecordReadState.Invalid))
                throw new IOException(string.Join(" ", found.Problems));
            var result = new List<ToolObservation>();
            foreach (var id in ToolSelection.DeclaredOrLocked(found.Portable))
            {
                if (!catalogue.Entries.TryGetValue(id, out var entry)) throw new IOException($"The declared or locked tool has no catalogue entry: {id}.");
                result.Add(await observations.ReadAsync(new(entry, locations, found, mainRoot), token).ConfigureAwait(false));
            }
            return result;
        })
    { }

    internal ToolWorkspaceObservationReader(IToolVersionService versions, Func<ToolWorkspaceObservationReader> createReader)
        : this(versions, (workspacePath, mainRoot, token) => createReader()._readDeclared(workspacePath, mainRoot, token))
    { }

    private ToolWorkspaceObservationReader(IToolVersionService versions,
        Func<string, string, CancellationToken, Task<IReadOnlyList<ToolObservation>>> readDeclared)
    {
        _codec = new(versions);
        _readDeclared = readDeclared;
    }

    internal async Task<IReadOnlyList<ToolObservation>> ReadAsync(string workspacePath, string mainRoot, CancellationToken token)
    {
        // Only project settings and the committed lock can make workspace Tool observation eligible. No home or combined record read precedes this check.
        var root = ToolRecordPaths.ResolveRoot(workspacePath);
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, token).ConfigureAwait(false);
        if (settings.State == WorkspaceSettingsReadState.Absent) return [];
        if (settings.State != WorkspaceSettingsReadState.Complete) throw new IOException(settings.Cause);
        var snapshot = settings.Snapshot ?? throw new IOException("The settings observation has no file snapshot.");
        if (_codec.ReadSettings(ToolRecordJson.Read(snapshot.Bytes.ToArray())).Count == 0 && !await LockNamesToolsAsync(root, token).ConfigureAwait(false)) return [];
        return await _readDeclared(workspacePath, mainRoot, token).ConfigureAwait(false);
    }

    // An unreadable lock names no tools here. The full record read reports its problem when tools are declared.
    private async Task<bool> LockNamesToolsAsync(string root, CancellationToken token)
    {
        var path = Path.Combine(root, WorkspaceOwnershipDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path)) return false;
        try { return _codec.ReadLock(ToolRecordJson.Read(await File.ReadAllBytesAsync(path, token).ConfigureAwait(false))).Count != 0; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException or InvalidDataException) { return false; }
    }
}
