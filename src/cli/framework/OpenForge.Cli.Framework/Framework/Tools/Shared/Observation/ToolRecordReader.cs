using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Observation;

internal sealed class ToolRecordReader(ToolPortableCodec codec, ToolCatalogue catalogue, ToolRecordPaths paths)
{
    private readonly List<string> _problems = [];
    private readonly Dictionary<string, string?> _expected = new(StringComparer.Ordinal);

    internal async Task<ToolRecordsResult> ReadAsync(CancellationToken cancellationToken)
    {
        var settingsPath = Path.Combine(paths.WorkspacePath, WorkspaceSettingsDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        var lockPath = Path.Combine(paths.WorkspacePath, WorkspaceOwnershipDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
        var (settings, settingsState) = await ReadAsync(settingsPath, paths.WorkspacePath, root =>
        {
            var decoded = WorkspaceSettingsCodec.Read(ToolRecordJson.Write(root));
            if (decoded.Document is null) throw new ArgumentException(decoded.Cause);
            Version(root, WorkspaceSettingsDefinitions.SchemaVersion);
            return codec.ReadSettings(root);
        }, cancellationToken).ConfigureAwait(false);
        var (locked, lockState) = await ReadAsync(lockPath, paths.WorkspacePath, root =>
        {
            var decoded = WorkspaceOwnershipCodec.Read(ToolRecordJson.Write(root));
            if (decoded.Document is null) throw new ArgumentException(decoded.Cause);
            Version(root, WorkspaceOwnershipDefinitions.SchemaVersion);
            return codec.ReadLock(root);
        }, cancellationToken).ConfigureAwait(false);
        var machines = new SortedDictionary<string, ToolMachineState>(StringComparer.Ordinal);
        var machineState = ToolRecordReadState.Missing;
        foreach (var id in Inventory(paths.ToolsDirectory, catalogue.Entries.Keys, ref machineState))
        {
            var (machine, state) = await ReadMachineAsync(paths.Tool(id), ToolMachineJsonContext.Default.ToolMachineState, cancellationToken).ConfigureAwait(false);
            machineState = Combine(machineState, state);
            if (machine is null) continue;
            try
            {
                if (machine.ToolId != id) throw new ArgumentException("The tool ID doesn't match its machine-state filename.");
                codec.ValidateVersion(machine.Version);
                ArgumentException.ThrowIfNullOrWhiteSpace(machine.ChannelId);
                ArgumentNullException.ThrowIfNull(machine.Invocation);
                ArgumentNullException.ThrowIfNull(machine.Launchers);
                ArgumentNullException.ThrowIfNull(machine.Integrity);
                if (!Path.IsPathFullyQualified(machine.CommandPath) || !Path.IsPathFullyQualified(machine.Invocation.ExecutablePath))
                    throw new ArgumentException("Machine command paths must be absolute.");
                machines.Add(id, machine);
            }
            catch (ArgumentException exception) { _problems.Add($"{paths.Tool(id)}: {exception.Message}"); machineState = ToolRecordReadState.Invalid; }
        }
        var runtimes = new SortedDictionary<string, ToolNodeRuntimeRecord>(StringComparer.Ordinal);
        var runtimeState = ToolRecordReadState.Missing;
        foreach (var version in Inventory(paths.RuntimesDirectory, catalogue.NodeRuntimes.Select(runtime => runtime.Version), ref runtimeState))
        {
            var (runtime, state) = await ReadMachineAsync(paths.Runtime(version), ToolMachineJsonContext.Default.ToolNodeRuntimeRecord, cancellationToken).ConfigureAwait(false);
            runtimeState = Combine(runtimeState, state);
            if (runtime is null) continue;
            if (runtime.Version != version || runtime.Integrity is null || !Path.IsPathFullyQualified(runtime.RuntimePath))
            { _problems.Add($"{paths.Runtime(version)}: The runtime identity, integrity or path is invalid."); runtimeState = ToolRecordReadState.Invalid; }
            else runtimes.Add(version, runtime);
        }
        var (workspace, workspaceState) = await ReadMachineAsync(paths.WorkspaceRecordPath, ToolMachineJsonContext.Default.ToolWorkspaceMachineState, cancellationToken).ConfigureAwait(false);
        if (workspace is not null && (workspace.WorkspaceKey != paths.WorkspaceKey || !Path.IsPathFullyQualified(workspace.WorkspacePath)
            || WorkspaceIdentity.Key(workspace.WorkspacePath) != paths.WorkspaceKey || workspace.NativeSettings is null))
        {
            _problems.Add($"{paths.WorkspaceRecordPath}: The physical workspace binding is invalid.");
            workspace = null;
            workspaceState = ToolRecordReadState.Invalid;
        }
        var (path, pathState) = await ReadMachineAsync(paths.PathRecordPath, ToolMachineJsonContext.Default.ToolPathState, cancellationToken).ConfigureAwait(false);
        if (path is not null && (path.Changes is null || !Path.IsPathFullyQualified(path.BinPath)))
        { _problems.Add($"{paths.PathRecordPath}: The PATH record requires an absolute bin path and changes."); path = null; pathState = ToolRecordReadState.Invalid; }
        return new ToolRecordsResult
        {
            PortableState = Combine(settingsState, lockState),
            Portable = settings is not null || locked is not null || settingsState == ToolRecordReadState.Missing && lockState == ToolRecordReadState.Missing
                ? new ToolPortableRecords { Settings = settings ?? new Dictionary<string, ToolSetting>(), Lock = locked ?? new Dictionary<string, ToolLockRecord>() } : null,
            MachineState = machineState,
            Machine = machines,
            WorkspaceMachineState = workspaceState,
            WorkspaceMachine = workspace,
            RuntimeRecordsState = runtimeState,
            Runtimes = runtimes,
            PathRecordState = pathState,
            Path = path,
            Problems = _problems.ToArray(),
            ExpectedFileSha256 = _expected,
        };
    }

    private Task<(T?, ToolRecordReadState)> ReadMachineAsync<T>(string path, JsonTypeInfo<T> type, CancellationToken token) where T : class
        => ReadAsync(path, paths.HomePath, root =>
        {
            Version(root, ToolRecordDefinitions.LocalSchemaVersion);
            if (!root.ContainsKey(WorkspaceSettingsDefinitions.SchemaVersionProperty)) root[WorkspaceSettingsDefinitions.SchemaVersionProperty] = 0;
            var value = root.Deserialize(type) ?? throw new ArgumentException("The machine record requires an object.");
            ToolMachineValidation.Validate(value);
            if (value is ToolNodeRuntimeRecord runtime) codec.ValidateVersion(runtime.Version);
            return value;
        }, token);

    private async Task<(T?, ToolRecordReadState)> ReadAsync<T>(string path, string root, Func<JsonObject, T> decode, CancellationToken token) where T : class
    {
        try
        {
            ToolRecordPaths.Check(root, path);
            if (!File.Exists(path))
            {
                if (Directory.Exists(path)) throw new IOException("A record destination is a directory.");
                _expected.Add(path, null);
                return (null, ToolRecordReadState.Missing);
            }
            var bytes = await File.ReadAllBytesAsync(path, token).ConfigureAwait(false);
            ToolRecordPaths.Check(root, path);
            _expected.Add(path, Convert.ToHexStringLower(SHA256.HashData(bytes)));
            return (decode(ToolRecordJson.Read(bytes)), ToolRecordReadState.Available);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException or DecoderFallbackException)
        { _problems.Add($"{path}: {exception.Message}"); return (null, ToolRecordReadState.Invalid); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { _problems.Add($"{path}: {exception.Message}"); return (null, ToolRecordReadState.Unavailable); }
    }

    private string[] Inventory(string directory, IEnumerable<string> known, ref ToolRecordReadState state)
    {
        try
        {
            ToolRecordPaths.Check(paths.HomePath, directory);
            var found = Directory.Exists(directory) ? Directory.EnumerateFiles(directory, "*" + ToolRecordPaths.Extension).Select(Path.GetFileNameWithoutExtension).OfType<string>() : [];
            return known.Concat(found).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { _problems.Add($"{directory}: {exception.Message}"); state = ToolRecordReadState.Unavailable; return known.Order(StringComparer.Ordinal).ToArray(); }
    }

    private void Version(JsonObject root, int current)
    {
        var node = root[WorkspaceSettingsDefinitions.SchemaVersionProperty];
        if (node is not null && (node is not JsonValue value || !value.TryGetValue<int>(out _)))
            throw new ArgumentException($"\"{WorkspaceSettingsDefinitions.SchemaVersionProperty}\" requires an integer.");
        var version = node?.GetValue<int>() ?? 0;
        if (version != current) _problems.Add($"The record declares schemaVersion {version}. This reader writes {current}.");
    }

    private static ToolRecordReadState Combine(ToolRecordReadState first, ToolRecordReadState second)
    {
        if (first == ToolRecordReadState.Invalid || second == ToolRecordReadState.Invalid) return ToolRecordReadState.Invalid;
        if (first == ToolRecordReadState.Unavailable || second == ToolRecordReadState.Unavailable) return ToolRecordReadState.Unavailable;
        if (first == ToolRecordReadState.Available || second == ToolRecordReadState.Available) return ToolRecordReadState.Available;
        return ToolRecordReadState.Missing;
    }
}
