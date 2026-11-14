using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

// The Tool record contract defines operation-scoped sibling backups without leases or rollback.
internal sealed class ToolBackupService(IToolProcessHost processes) : IToolBackupService
{
    private readonly ToolGitRecoveryReader _git = new(processes);
    private readonly Dictionary<string, ToolPreparedBackup> _prepared = new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    internal async Task CheckFeasibilityAsync(string targetPath, CancellationToken token)
    {
        var target = FileExpectation.NormalizeAbsolutePath(targetPath, nameof(targetPath));
        if (_prepared.TryGetValue(target, out var prepared))
        {
            await CheckSiblingAsync(prepared, token).ConfigureAwait(false);
            return;
        }
        var parent = Path.GetDirectoryName(target) ?? throw new IOException("The backup target has no parent.");
        while (!Directory.Exists(parent)) parent = Path.GetDirectoryName(parent) ?? throw new IOException("The backup target has no existing ancestor.");
        var scope = ToolFileReader.Scope(parent);
        _ = await ToolFileReader.ReadAsync(scope, target, token).ConfigureAwait(false);
        if ((await ToolFileReader.ReadAsync(scope, ToolBackupPaths.Beside(target), token).ConfigureAwait(false)).HasBytes)
            throw new IOException("The sibling backup destination is occupied.");
    }

    public async Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var target = FileExpectation.NormalizeAbsolutePath(request.TargetPath, nameof(request.TargetPath));
        string? retainedPath = null;
        try
        {
            if (_prepared.TryGetValue(target, out var existing))
            {
                retainedPath = existing.Result.BackupPath;
                await CheckSiblingAsync(existing, cancellationToken).ConfigureAwait(false);
                return existing.Result;
            }
            var parent = Path.GetDirectoryName(target) ?? throw new IOException("The backup target has no parent.");
            while (!Directory.Exists(parent)) parent = Path.GetDirectoryName(parent) ?? throw new IOException("The backup target has no existing ancestor.");
            var scope = ToolFileReader.Scope(parent);
            var original = await ToolFileReader.ReadAsync(scope, target, cancellationToken).ConfigureAwait(false);
            var siblingPath = ToolBackupPaths.Beside(target);
            var sibling = await ToolFileReader.ReadAsync(scope, siblingPath, cancellationToken).ConfigureAwait(false);
            if (sibling.HasBytes) throw new IOException($"The backup destination is occupied: {siblingPath}.");
            var gitObject = original.HasBytes ? await _git.ReadAsync(target, cancellationToken).ConfigureAwait(false) : null;
            var kind = ToolBackupKind.PriorAbsence;
            FileStateSnapshot? saved = null;
            if (original.HasBytes)
            {
                kind = gitObject is null ? ToolBackupKind.Sibling : ToolBackupKind.Git;
                if (kind == ToolBackupKind.Sibling)
                {
                    var current = await ToolFileReader.ReadAsync(scope, target, cancellationToken).ConfigureAwait(false);
                    if (current.Expectation != original.Expectation) throw new IOException("The backup target changed before it could be saved.");
                    await ToolFileReplacement.WriteAsync(scope, sibling.Expectation, original.Bytes.ToArray(), cancellationToken).ConfigureAwait(false);
                    retainedPath = siblingPath;
                    saved = await ToolFileReader.ReadAsync(scope, siblingPath, cancellationToken).ConfigureAwait(false);
                    if (saved.ContentHash != original.ContentHash) throw new IOException("The backup bytes did not verify.");
                    if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(siblingPath, File.GetUnixFileMode(target));
                }
            }
            if ((await ToolFileReader.ReadAsync(scope, target, cancellationToken).ConfigureAwait(false)).Expectation != original.Expectation)
                throw new IOException("The backup target changed during preparation. Keep the backup for manual checking.");
            var result = new ToolBackupResult { TargetPath = target, Kind = kind, BackupPath = saved?.LogicalPath, GitObjectId = gitObject };
            _prepared.Add(target, new(scope, result, original, saved));
            return result;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or OperationCanceledException)
        {
            return new()
            {
                TargetPath = target,
                Kind = retainedPath is null ? ToolBackupKind.PriorAbsence : ToolBackupKind.Sibling,
                BackupPath = retainedPath,
                Failure = retainedPath is null ? exception.Message : $"{exception.Message} Keep {retainedPath} and compare its contents with {target} before a manual restore.",
            };
        }
    }

    public async Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!_prepared.TryGetValue(request.Backup.TargetPath, out var prepared) || prepared.Result != request.Backup)
            return new(false, request.Backup.BackupPath, "The backup does not belong to this operation.");
        if (!request.Verified) return new(false, prepared.Result.BackupPath, ManualInstructions(prepared));
        try
        {
            var current = await ToolFileReader.ReadAsync(prepared.Scope, prepared.Result.TargetPath, cancellationToken).ConfigureAwait(false);
            if (current.ContentHash != request.ExpectedAppliedSha256) throw new IOException("The completed target differs from the verified result.");
            await CheckSiblingAsync(prepared, cancellationToken).ConfigureAwait(false);
            if (prepared.Sibling is { } sibling)
                await ToolFileReplacement.WriteAsync(prepared.Scope, sibling.Expectation, bytes: null, cancellationToken).ConfigureAwait(false);
            _prepared.Remove(prepared.Result.TargetPath);
            return new(prepared.Sibling is not null, null, null);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException)
        {
            return new(false, prepared.Result.BackupPath, exception.Message + " " + ManualInstructions(prepared));
        }
    }

    private static async Task CheckSiblingAsync(ToolPreparedBackup prepared, CancellationToken token)
    {
        var current = await ToolFileReader.ReadAsync(prepared.Scope, ToolBackupPaths.Beside(prepared.Result.TargetPath), token).ConfigureAwait(false);
        if (prepared.Sibling is { } sibling ? current.Expectation != sibling.Expectation : current.HasBytes)
            throw new IOException("The operation backup changed or its destination became occupied.");
    }

    private static string ManualInstructions(ToolPreparedBackup prepared)
    {
        if (prepared.Sibling is { } sibling)
            return $"Keep {sibling.LogicalPath}. Before a manual restore, check its SHA-256 is {sibling.ContentHash} and check the current target. Copy the saved bytes to {prepared.Result.TargetPath} only after reviewing both files.";
        if (prepared.Result.GitObjectId is { } gitObject)
            return $"Keep the current file for review. Git blob {gitObject} contains the original bytes for {prepared.Result.TargetPath}. Check both versions before manually restoring.";
        return $"The target {prepared.Result.TargetPath} was absent before this operation. Check its current contents before any manual deletion.";
    }
}
