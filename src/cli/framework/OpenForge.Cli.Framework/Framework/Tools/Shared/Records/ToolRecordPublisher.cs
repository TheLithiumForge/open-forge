using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

// The shared Tool operation contract requires matching settings and lock publication.
internal sealed class ToolRecordPublisher(Action<PlannedFileChange>? completed = null)
{
    internal async ValueTask<ToolRecordPublication> PublishAsync(WorkspaceLockLease lease, ToolRecordWrite write, CancellationToken token)
    {
        if (!lease.IsHeldFor(lease.Request.Workspace)) throw new InvalidOperationException("Tool record publication requires its live workspace lease.");
        var workspace = lease.Request.Workspace;
        var settingsPath = Path.GetFullPath(WorkspaceSettingsDefinitions.RelativePath, workspace.LexicalRoot);
        var lockPath = Path.GetFullPath(WorkspaceOwnershipDefinitions.RelativePath, workspace.LexicalRoot);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (!string.Equals(write.Settings.LogicalPath, settingsPath, comparison) || !string.Equals(write.Lock.LogicalPath, lockPath, comparison))
            throw new ArgumentException("Tool records must use the fixed workspace settings and lock paths.", nameof(write));
        Validate(write.SettingsBytes, write.LockBytes);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        foreach (var file in new[] { write.Settings, write.Lock })
        {
            var check = await validator.ValidateAsync(workspace, file.Expectation, token).ConfigureAwait(false);
            if (check.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
            if (check.State != FileExpectationValidationState.Matched) throw new IOException("A tool control record changed after the plan.");
            if (ToolFileReader.Observe(workspace, ToolBackupPaths.Beside(file.LogicalPath), token) != NoFollowLeafState.Missing)
                throw new IOException("A sibling backup blocks tool record publication.");
        }
        var changes = new List<PlannedFileChange>();
        Add(changes, write.Settings, write.SettingsBytes);
        Add(changes, write.Lock, write.LockBytes);
        if (changes.Count == 0) return new(write.Settings, write.Lock, ToolGitRecoveryReader.Read(workspace, token), [], []);
        var parent = Path.GetDirectoryName(settingsPath) ?? throw new IOException("The settings parent is unavailable.");
        var created = false;
        ToolBackupSession? session = null;
        try
        {
            var parentState = ToolFileReader.Observe(workspace, parent, token);
            if (parentState == NoFollowLeafState.Missing) { Directory.CreateDirectory(parent); created = true; }
            else if (parentState != NoFollowLeafState.Directory) throw new IOException("The tool record parent isn't an ordinary directory.");
            session = await ToolBackupSession.PrepareAsync(lease, changes, token).ConfigureAwait(false);
            foreach (var file in new[] { write.Settings, write.Lock })
            {
                var check = await validator.ValidateAsync(workspace, file.Expectation, token).ConfigureAwait(false);
                if (check.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
                if (check.State != FileExpectationValidationState.Matched) throw new IOException("A paired record changed before publication.");
            }
            await session.ApplyAsync(token, completed).ConfigureAwait(false);
            var settings = await ToolFileReader.ReadAsync(workspace, settingsPath, token).ConfigureAwait(false);
            var receipt = await ToolFileReader.ReadAsync(workspace, lockPath, token).ConfigureAwait(false);
            Validate(settings.Bytes.AsMemory(), receipt.Bytes.AsMemory());
            var residuals = await session.CompleteAsync(token).ConfigureAwait(false);
            return new(settings, receipt, session.Mode, residuals, created ? [parent] : []);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or OperationCanceledException or JsonException or InvalidOperationException)
        {
            var residuals = new List<ToolBackupResidual>();
            if (session is not null) residuals.AddRange(await session.RestoreAsync().ConfigureAwait(false));
            else
                foreach (var backup in ToolBackupReader.Read(workspace, [WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath], CancellationToken.None))
                    residuals.Add(new(backup.Target, backup.Backup, "A saved backup is still present after preparation failed."));
            if (created)
            {
                try
                {
                    if (ToolFileReader.Observe(workspace, parent, CancellationToken.None) != NoFollowLeafState.Directory)
                        throw new IOException("The created tool record directory changed.");
                    Directory.Delete(parent, recursive: false);
                }
                catch (Exception cleanup) when (cleanup is IOException or UnauthorizedAccessException)
                { residuals.Add(new(parent, null, cleanup.Message)); }
            }
            throw new ToolRecordWriteException("Tool records couldn't be published as a matching pair.", exception, session?.Mode, [.. residuals]);
        }
    }

    private static void Add(List<PlannedFileChange> changes, FileStateSnapshot current, byte[] bytes)
    {
        if (current.ContentHash == FileExpectation.Hash(bytes)) return;
        changes.Add(current.Expectation.Exists ? PlannedFileChange.Replace(current.Expectation, bytes) : PlannedFileChange.Create(current.Expectation, bytes));
    }

    private static void Validate(ReadOnlyMemory<byte> settingsBytes, ReadOnlyMemory<byte> lockBytes)
    {
        var settings = WorkspaceSettingsCodec.Read(settingsBytes).Document ?? throw new JsonException("The intended tool settings are invalid.");
        var ownership = WorkspaceOwnershipCodec.Read(lockBytes).Document ?? throw new JsonException("The intended tool lock is invalid.");
        ToolRecordConsistency.Validate(settings, ownership);
    }
}
