using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Application;

// Restore control records before building any plan that depends on those records.
internal sealed class RepairToolBackupOperation(WorkspaceLockManager locks, RepairInteraction interaction)
{
    internal async ValueTask<RepairResult?> ExecuteAsync(RepairRequest request, CancellationToken token)
    {
        var candidates = ToolBackupReader.Read(request.Workspace, ToolBackupPaths.WorkspaceTargets, token);
        if (candidates.Length == 0) return null;
        var files = new List<ToolBackupRestoreFile>();
        try
        {
            foreach (var candidate in candidates) files.Add(await ToolBackupRestorer.ReadAsync(request.Workspace, candidate, token).ConfigureAwait(false));
            var controls = new List<FileStateSnapshot>();
            foreach (var path in ToolBackupPaths.WorkspaceTargets)
                controls.Add(await ToolFileReader.ReadAsync(request.Workspace, path, token).ConfigureAwait(false));
            Validate(request.Workspace, controls, files);
            var preview = Result(request, files, null, []);
            if (request.Relinks.Count != 0)
                return Result(request, files, null, [new(RepairFindingCode.ToolBackupConflict, "Restore control-file backups in a separate Repair request before applying explicit relinks.")]);
            if (!request.Automatic && request.SelectionMode != RepairSelectionMode.InteractivePrompt)
                return Result(request, files, null, [new(RepairFindingCode.SelectionRequired, "Restore control-file backups with --automatic or an interactive confirmation first.")]);
            if (request.Mode == RepairMode.DryRun) return preview;
            if (!request.Automatic)
            {
                var answer = await interaction.Confirm(preview, new(RepairConfirmationKind.Final, files.Count), new(request.AllowInteraction), token).ConfigureAwait(false);
                if (answer.State == CliPromptState.Unavailable)
                    return Result(request, files, null, [new(RepairFindingCode.ConfirmationRequired, RepairDefinitions.ConfirmationRequiredMessage)]);
                if (answer.State != CliPromptState.Answered || !answer.Value)
                    return Result(request, files, null, [new(RepairFindingCode.Interrupted, "Backup restoration was cancelled before writing.")]);
            }
            var acquired = await locks.AcquireAsync(new(request.Workspace, RepairDefinitions.CommandIdentity, Guid.NewGuid()), token).ConfigureAwait(false);
            if (acquired.Lease is not { } lease)
                return Result(request, files, null, [new(RepairFindingCode.WorkspaceLockUnavailable, acquired.Cause ?? "The workspace lease isn't available.")]);
            await using (lease.ConfigureAwait(false))
            {
                foreach (var before in controls)
                {
                    var current = await ToolFileReader.ReadAsync(request.Workspace, before.LogicalPath, token).ConfigureAwait(false);
                    if (current.Expectation.Exists != before.Expectation.Exists || current.ContentHash != before.ContentHash)
                        throw new IOException("A control file changed after the restoration preview.");
                }
                var restoration = await ToolBackupRestorer.ApplyAsync(lease, files, token).ConfigureAwait(false);
                var findings = new List<RepairFinding>();
                if (restoration.Interrupted) findings.Add(new(RepairFindingCode.Interrupted, restoration.Cause ?? "Restoration was interrupted."));
                else if (restoration.Cause is { } cause)
                    findings.Add(new(restoration.Files.Any(file => file.Written) ? RepairFindingCode.ToolBackupWriteFailed : RepairFindingCode.ToolBackupConflict, cause));
                foreach (var receipt in restoration.Files.Where(file => file.Cause is not null))
                    findings.Add(new(receipt.CleanupUnknown ? RepairFindingCode.ToolBackupWriteFailed : RepairFindingCode.ToolBackupRetained, receipt.Cause ?? "A backup is still present.", Relative(request.Workspace, receipt.Backup)));
                return Result(request, files, restoration, findings);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { return Result(request, files, null, [new(RepairFindingCode.Interrupted, "Restoration was cancelled. Saved backups are kept.")]); }
        catch (DecoderFallbackException)
        { return Result(request, files, null, [new(RepairFindingCode.ToolBackupConflict, "The saved control file isn't valid UTF-8.")]); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        { return Result(request, files, null, [new(RepairFindingCode.ToolBackupConflict, exception.Message)]); }
    }

    private static void Validate(CliWorkspace workspace, IReadOnlyList<FileStateSnapshot> controls, IReadOnlyList<ToolBackupRestoreFile> files)
    {
        FileStateSnapshot Intended(string path) => files.FirstOrDefault(file => Relative(workspace, file.Target.LogicalPath) == path)?.Backup
            ?? controls.Single(file => Relative(workspace, file.LogicalPath) == path);
        var currentSettings = controls.Single(file => Relative(workspace, file.LogicalPath) == WorkspaceSettingsDefinitions.RelativePath);
        var permission = currentSettings.HasBytes ? WorkspaceSettingsCodec.Read(currentSettings.Bytes.AsMemory()).Document : WorkspaceSettingsDocument.Empty;
        foreach (var file in files)
        {
            var relative = Relative(workspace, file.Target.LogicalPath);
            if (relative == ManagedGitIgnoreSection.Path)
            {
                _ = new UTF8Encoding(false, true).GetCharCount(file.Backup.Bytes.AsSpan());
                if (permission is null || !WorkspaceAllowList.Admits(permission.AllowInstallPaths, relative))
                    throw new IOException("The current settings don't permit restoring .gitignore.");
            }
            if (permission is not null && WorkspaceRemovals.IsPathRemoved(WorkspaceLayout.Create(workspace, permission.MainRoot), relative, permission))
                throw new IOException("A removal exclusion covers a restoration target.");
        }
        var settingFile = Intended(WorkspaceSettingsDefinitions.RelativePath);
        var settings = settingFile.HasBytes ? WorkspaceSettingsCodec.Read(settingFile.Bytes.AsMemory()).Document : WorkspaceSettingsDocument.Empty;
        var lockFile = Intended(WorkspaceOwnershipDefinitions.RelativePath);
        var ownership = lockFile.HasBytes ? WorkspaceOwnershipCodec.Read(lockFile.Bytes.AsMemory()).Document : WorkspaceOwnershipDocument.Empty;
        if (settings is null || ownership is null) throw new IOException("Restoration requires valid resulting settings and lock records.");
        if (settings.Tools.Count != ownership.Tools.Count || settings.Tools.Any(pair => !ownership.Tools.TryGetValue(pair.Key, out var receipt) || receipt.Version != pair.Value.Version))
            throw new IOException("The restored tool pins in settings and lock would disagree.");
    }

    private static RepairResult Result(RepairRequest request, IReadOnlyList<ToolBackupRestoreFile> files, ToolBackupRestoration? restoration, IReadOnlyList<RepairFinding> findings)
    {
        var data = files.Select((file, index) =>
        {
            var receipt = restoration?.Files[index];
            return new RepairToolBackupFile(Relative(request.Workspace, file.Target.LogicalPath), Relative(request.Workspace, file.Backup.LogicalPath),
                file.Target.ContentHash, file.Backup.ContentHash ?? throw new IOException("The backup hash is unavailable."), receipt?.Written == true, receipt?.Verified == true,
                receipt?.BackupRemoved == true, receipt?.Cause);
        }).ToArray();
        var verified = data.Count(file => file.Verified);
        var result = new RepairResult(new RepairResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Automatic = request.Automatic,
            Relinks = request.Relinks,
            SelectionMode = request.SelectionMode,
            Facts = new()
            {
                Diagnosis = RepairPostDiagnosis.NotRequested.Coverage,
                Selection = null,
                Plan = null,
                Findings = findings,
                AffectedPaths = data.Select(file => file.Target).ToArray(),
                Counts = new(selectedFindings: data.Length, unselectedFindings: 0, repaired: verified, remaining: data.Length - verified, newFindings: 0,
                    manual: 0, guided: 0, blocked: findings.Count(finding => finding.Status == OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Blocked), selectedEffects: data.Length, appliedEffects: data.Count(file => file.Written),
                    verifiedEffects: verified, noOps: 0, conflicts: findings.Count),
                Preflight = RepairPreflight.NotRequested,
                Application = RepairApplication.NotRequested,
                Verification = RepairVerification.NotRequested,
                Recovery = RepairRecovery.NotRequired,
                PostDiagnosis = RepairPostDiagnosis.NotRequested,
            },
        });
        var applied = restoration is not null;
        return result with { ToolBackups = new(data, applied, RepairToolBackupNextAction.Read(result, data, applied)) };
    }

    private static string Relative(CliWorkspace workspace, string path) => Path.GetRelativePath(workspace.LexicalRoot, path).Replace(Path.DirectorySeparatorChar, '/');
}
