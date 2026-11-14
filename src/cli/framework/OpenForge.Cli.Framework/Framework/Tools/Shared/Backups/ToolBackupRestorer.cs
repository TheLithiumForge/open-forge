using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

internal static class ToolBackupRestorer
{
    internal static async ValueTask<ToolBackupRestoreFile> ReadAsync(CliWorkspace workspace, ToolBackupCandidate candidate, CancellationToken token)
    {
        if (!candidate.SafePaths || candidate.Backup != ToolBackupPaths.Beside(candidate.Target))
            throw new IOException("Restoration requires an ordinary sibling backup at its exact target.");
        var target = await ToolFileReader.ReadAsync(workspace, candidate.Target, token).ConfigureAwait(false);
        var backup = await ToolFileReader.ReadAsync(workspace, candidate.Backup, token).ConfigureAwait(false);
        if (!backup.HasBytes) throw new IOException("The sibling backup is missing.");
        return new(target, backup);
    }

    internal static async ValueTask<ToolBackupRestoration> ApplyAsync(WorkspaceLockLease lease, IReadOnlyList<ToolBackupRestoreFile> files, CancellationToken token)
    {
        var receipts = files.Select(file => new ToolBackupRestoreReceipt(file.Target.LogicalPath, file.Backup.LogicalPath, false, false, false, null)).ToArray();
        try
        {
            var targets = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
            foreach (var file in files)
            {
                if (!file.Backup.HasBytes || file.Backup.LogicalPath != ToolBackupPaths.Beside(file.Target.LogicalPath) || !targets.Add(file.Target.LogicalPath))
                    throw new IOException("Restoration targets must be distinct files with ordinary sibling backups.");
                await CheckAsync(lease, file.Target.Expectation, token).ConfigureAwait(false);
                await CheckAsync(lease, file.Backup.Expectation, token).ConfigureAwait(false);
            }
            if (files.Any(file => targets.Contains(file.Backup.LogicalPath))) throw new IOException("A restoration target cannot also be a backup.");
            for (var index = 0; index < files.Count; index++)
            {
                var file = files[index];
                await CheckAsync(lease, file.Backup.Expectation, token).ConfigureAwait(false);
                if (file.Target.ContentHash != file.Backup.ContentHash)
                {
                    UnixFileMode? mode = null;
                    if (!OperatingSystem.IsWindows()) mode = File.GetUnixFileMode(file.Backup.PhysicalPath ?? throw new IOException("The backup has no physical path."));
                    await ToolFileReplacement.WriteAsync(lease.Request.Workspace, new(file.Target.Expectation, file.Backup.Bytes.AsMemory(), mode),
                        () => RequireLease(lease), token).ConfigureAwait(false);
                    receipts[index] = receipts[index] with { Written = true };
                }
                var current = await ToolFileReader.ReadAsync(lease.Request.Workspace, file.Target.LogicalPath, token).ConfigureAwait(false);
                if (current.ContentHash != file.Backup.ContentHash) throw new IOException("The restored bytes didn't verify.");
                receipts[index] = receipts[index] with { Verified = true };
            }
            foreach (var file in files)
            {
                var current = await ToolFileReader.ReadAsync(lease.Request.Workspace, file.Target.LogicalPath, token).ConfigureAwait(false);
                if (current.ContentHash != file.Backup.ContentHash) throw new IOException("A restored target changed before backup removal.");
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { return new(receipts, true, "Restoration was cancelled. Saved backups are kept."); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        { return new(receipts, false, exception.Message); }
        for (var index = 0; index < files.Count; index++)
        {
            try
            {
                await CheckAsync(lease, files[index].Backup.Expectation, CancellationToken.None).ConfigureAwait(false);
                File.Delete(files[index].Backup.PhysicalPath ?? throw new IOException("The backup has no physical path."));
                receipts[index] = receipts[index] with { BackupRemoved = true };
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                var state = ToolFileReader.Observe(lease.Request.Workspace, files[index].Backup.LogicalPath, CancellationToken.None);
                receipts[index] = receipts[index] with
                {
                    BackupRemoved = state == NoFollowLeafState.Missing,
                    Cause = state == NoFollowLeafState.Missing ? null : exception.Message,
                    CleanupUnknown = state is not NoFollowLeafState.Missing and not NoFollowLeafState.OrdinaryFile,
                };
            }
        }
        return new(receipts, false, null);
    }

    private static async ValueTask CheckAsync(WorkspaceLockLease lease, FileExpectation expectation, CancellationToken token)
    {
        RequireLease(lease);
        var check = await new FileExpectationValidator(new PhysicalPathResolver()).ValidateAsync(lease.Request.Workspace, expectation, token).ConfigureAwait(false);
        if (check.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
        if (check.State != FileExpectationValidationState.Matched) throw new IOException(check.Cause ?? "The backup or target changed.");
    }

    private static void RequireLease(WorkspaceLockLease lease)
    {
        if (!lease.IsHeldFor(lease.Request.Workspace)) throw new InvalidOperationException("Restoration requires its live workspace lease.");
    }
}
