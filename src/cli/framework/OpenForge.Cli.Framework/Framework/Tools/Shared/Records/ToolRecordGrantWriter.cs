using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal sealed class ToolRecordGrantWriter(IToolBackupService backups)
{
    internal async Task<ToolRecordWriteResult> WriteAsync(ToolRecordGrantRequest request, CancellationToken token)
    {
        var written = new List<string>();
        var pending = new List<ToolBackupCompletionRequest>();
        string? target = null;
        var verified = false;
        try
        {
            token.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(request.Locations);
            ArgumentNullException.ThrowIfNull(request.Expected);
            ArgumentNullException.ThrowIfNull(request.AllowInstallPaths);
            foreach (var value in request.AllowInstallPaths)
                if (!PortableWorkspacePath.TryNormalize(value, out _))
                    throw new ArgumentException("An explicit grant requires a portable workspace-relative path.");
            if (request.AllowInstallPaths.Count == 0) return Result(written, pending);

            // Grant publication depends only on the workspace settings, never on a Tool home or lock.
            var root = ToolRecordPaths.ResolveRoot(request.Locations.WorkspacePath);
            target = Path.Combine(root, WorkspaceSettingsDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
            ToolRecordPaths.Check(root, target);
            var observation = await ReadAsync(workspace, token).ConfigureAwait(false);
            var before = await ToolRecordFileWriter.CheckExpectedAsync(root, target, request.Expected, token).ConfigureAwait(false);
            var snapshot = observation.Snapshot ?? throw new IOException("The settings observation has no file expectation.");
            if ((before is null) != (observation.State == WorkspaceSettingsReadState.Absent)
                || !snapshot.Bytes.AsSpan().SequenceEqual(before ?? []))
                throw new IOException("Settings changed while the grant was being prepared.");

            var intended = before ?? [];
            foreach (var path in request.AllowInstallPaths.Distinct(StringComparer.Ordinal))
                intended = WorkspaceSettingsCodec.AddAllowInstallPath(intended, path) ?? intended;
            if (before is not null && before.AsSpan().SequenceEqual(intended)) return Result(written, pending);
            var decoded = WorkspaceSettingsCodec.Read(intended);
            if (decoded.Document is null) throw new ArgumentException(decoded.Cause);

            var backup = await backups.PrepareAsync(new(target), token).ConfigureAwait(false);
            pending.Add(new(backup, ToolRecordFileWriter.Hash(intended), Verified: false));
            if (backup.Failure is not null) throw new IOException(backup.Failure);
            await ToolRecordFileWriter.ReplaceAsync(new(root, target, intended), request.Expected, token).ConfigureAwait(false);
            written.Add(target);
            var confirmed = await ReadAsync(workspace, token).ConfigureAwait(false);
            if (confirmed.Snapshot is not { } saved || !saved.Bytes.AsSpan().SequenceEqual(intended))
                throw new IOException("The settings grant couldn't be verified after publication.");
            pending[^1] = pending[^1] with { Verified = true };
            verified = true;
            return Result(written, pending);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or JsonException or OperationCanceledException)
        {
            var unfinished = pending.Select(item => item.Backup.BackupPath).OfType<string>().ToList();
            if (target is not null && !verified) unfinished.Insert(0, target);
            return new(written.ToArray(), unfinished.ToArray(), exception.Message) { PendingBackups = pending.ToArray() };
        }
    }

    private static async Task<WorkspaceSettingsRead> ReadAsync(CliWorkspace workspace, CancellationToken token)
    {
        var observation = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, token).ConfigureAwait(false);
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
            throw new IOException(observation.Cause ?? "Authored settings aren't readable.");
        return observation;
    }

    private static ToolRecordWriteResult Result(List<string> written, List<ToolBackupCompletionRequest> pending)
        => new(written.ToArray(), [], null) { PendingBackups = pending.ToArray() };
}
