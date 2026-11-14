using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal sealed class ToolRecordWriter(ToolPortableCodec codec, IToolBackupService backups)
{
    internal async Task<ToolRecordWriteResult> WriteAsync(ToolRecordWriteRequest request, CancellationToken token)
    {
        var written = new List<string>();
        var finished = new HashSet<string>(StringComparer.Ordinal);
        var pending = new List<ToolBackupCompletionRequest>();
        var writes = new List<ToolRecordWrite>();
        try
        {
            var paths = new ToolRecordPaths(request.Locations.WorkspacePath, request.Locations.HomePath);
            if (request.Expected.PortableState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable)
                throw new IOException("Settings and lock facts must be readable before publication.");
            var settingsPath = Path.Combine(paths.WorkspacePath, WorkspaceSettingsDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            var lockPath = Path.Combine(paths.WorkspacePath, WorkspaceOwnershipDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            var settings = await ExistingAsync(paths.WorkspacePath, settingsPath, request.Expected, token).ConfigureAwait(false);
            var locked = await ExistingAsync(paths.WorkspacePath, lockPath, request.Expected, token).ConfigureAwait(false);
            var settingsRoot = codec.WriteSettings(settings, request.Portable.Settings);
            var lockRoot = codec.WriteLock(locked, request.Portable.Lock);
            Stamp(settingsRoot, WorkspaceSettingsDefinitions.SchemaVersion, WorkspaceSettingsDefinitions.SchemaUrl);
            Stamp(lockRoot, WorkspaceOwnershipDefinitions.SchemaVersion, WorkspaceOwnershipDefinitions.SchemaUrl);
            writes.Add(new(paths.WorkspacePath, settingsPath, ToolRecordJson.Write(settingsRoot)));
            writes.Add(new(paths.WorkspacePath, lockPath, ToolRecordJson.Write(lockRoot)));
            foreach (var machine in request.Machine.OrderBy(value => value.ToolId, StringComparer.Ordinal))
            {
                codec.ValidateVersion(machine.Version);
                writes.Add(await MachineAsync(paths, paths.Tool(machine.ToolId), machine, ToolMachineJsonContext.Default.ToolMachineState, request.Expected, token).ConfigureAwait(false));
            }
            foreach (var runtime in request.Runtimes.OrderBy(value => value.Version, StringComparer.Ordinal))
            {
                codec.ValidateVersion(runtime.Version);
                writes.Add(await MachineAsync(paths, paths.Runtime(runtime.Version), runtime, ToolMachineJsonContext.Default.ToolNodeRuntimeRecord, request.Expected, token).ConfigureAwait(false));
            }
            if (request.WorkspaceMachine is { } workspace)
            {
                if (workspace.WorkspaceKey != paths.WorkspaceKey || !Path.IsPathFullyQualified(workspace.WorkspacePath)
                    || WorkspaceIdentity.Key(workspace.WorkspacePath) != paths.WorkspaceKey)
                    throw new IOException("The machine record doesn't match the physical workspace.");
                writes.Add(await MachineAsync(paths, paths.WorkspaceRecordPath, workspace, ToolMachineJsonContext.Default.ToolWorkspaceMachineState, request.Expected, token).ConfigureAwait(false));
            }
            if (request.Path is { } path)
                writes.Add(await MachineAsync(paths, paths.PathRecordPath, path, ToolMachineJsonContext.Default.ToolPathState, request.Expected, token).ConfigureAwait(false));
            if (writes.Select(write => write.Path).Distinct(StringComparer.Ordinal).Count() != writes.Count)
                throw new IOException("A record publication cannot write the same path twice.");
            foreach (var write in writes) await ToolRecordFileWriter.CheckExpectedAsync(write.Root, write.Path, request.Expected, token).ConfigureAwait(false);
            foreach (var write in writes)
            {
                token.ThrowIfCancellationRequested();
                var before = await ToolRecordFileWriter.CheckExpectedAsync(write.Root, write.Path, request.Expected, token).ConfigureAwait(false);
                if (before is not null && before.AsSpan().SequenceEqual(write.Bytes)) { finished.Add(write.Path); continue; }
                var backup = await backups.PrepareAsync(new(write.Path), token).ConfigureAwait(false);
                pending.Add(new(backup, ToolRecordFileWriter.Hash(write.Bytes), Verified: false));
                if (backup.Failure is not null) throw new IOException(backup.Failure);
                await ToolRecordFileWriter.ReplaceAsync(write, request.Expected, token).ConfigureAwait(false);
                written.Add(write.Path);
                var bytes = await File.ReadAllBytesAsync(write.Path, token).ConfigureAwait(false);
                if (!bytes.AsSpan().SequenceEqual(write.Bytes)) throw new IOException($"The written record changed during verification: {write.Path}.");
                pending[^1] = pending[^1] with { Verified = true };
                finished.Add(write.Path);
            }
            return new(written.ToArray(), [], null) { PendingBackups = pending.ToArray() };
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or JsonException or OperationCanceledException)
        {
            var unfinished = writes.Where(write => !finished.Contains(write.Path)).Select(write => write.Path)
                .Concat(pending.Select(item => item.Backup).Where(backup => backup.BackupPath is not null).Select(backup => backup.BackupPath ?? backup.TargetPath)).ToArray();
            return new(written.ToArray(), unfinished, exception.Message) { PendingBackups = pending.ToArray() };
        }
    }

    private static async Task<ToolRecordWrite> MachineAsync<T>(ToolRecordPaths paths, string path, T value, JsonTypeInfo<T> type, ToolRecordsResult expected, CancellationToken token)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));
        ToolMachineValidation.Validate(value);
        var before = await ExistingAsync(paths.HomePath, path, expected, token).ConfigureAwait(false);
        var replacement = ToolRecordJson.Read(JsonSerializer.SerializeToUtf8Bytes(value, type));
        var merged = ToolRecordJson.Merge(before, replacement);
        Stamp(merged, ToolRecordDefinitions.LocalSchemaVersion, schemaUrl: null);
        return new(paths.HomePath, path, ToolRecordJson.Write(merged));
    }

    private static async Task<JsonObject> ExistingAsync(string root, string path, ToolRecordsResult expected, CancellationToken token)
    {
        var bytes = await ToolRecordFileWriter.CheckExpectedAsync(root, path, expected, token).ConfigureAwait(false);
        return bytes is null ? new JsonObject() : ToolRecordJson.Read(bytes);
    }

    private static void Stamp(JsonObject root, int version, string? schemaUrl)
    {
        var current = root[WorkspaceSettingsDefinitions.SchemaVersionProperty]?.GetValue<int>() ?? 0;
        root[WorkspaceSettingsDefinitions.SchemaVersionProperty] = Math.Max(current, version);
        if (schemaUrl is not null && !root.ContainsKey(WorkspaceSettingsDefinitions.SchemaProperty)) root[WorkspaceSettingsDefinitions.SchemaProperty] = schemaUrl;
    }

}
