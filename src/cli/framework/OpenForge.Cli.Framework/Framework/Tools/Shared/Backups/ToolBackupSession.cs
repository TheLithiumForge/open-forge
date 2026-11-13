using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

// Tool backups technical design defines this file-only lifecycle separately from recovery v1.
internal sealed partial class ToolBackupSession
{
    private readonly WorkspaceLockLease _lease;
    private readonly PlannedFileChange[] _changes;
    private readonly FileExpectationValidator _validator = new(new PhysicalPathResolver());
    private readonly Dictionary<string, FileStateSnapshot> _backups = new(StringComparer.Ordinal);
    private readonly Dictionary<string, FileStateSnapshot> _priors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, UnixFileMode> _priorModes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _attempted = new(StringComparer.Ordinal);
    private bool _closed;

    private ToolBackupSession(WorkspaceLockLease lease, PlannedFileChange[] changes)
    {
        _lease = lease;
        _changes = changes;
    }

    internal ToolBackupMode Mode { get; private set; }

    internal static async ValueTask<ToolBackupSession> PrepareAsync(
        WorkspaceLockLease lease, IReadOnlyList<PlannedFileChange> changes, CancellationToken token)
    {
        var session = new ToolBackupSession(lease, [.. changes]);
        session.RequireLease();
        session.Mode = ToolGitRecoveryReader.Read(lease.Request.Workspace, token);
        var targets = new HashSet<string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var change in session._changes)
        {
            if (!targets.Add(change.LogicalPath)) throw new ArgumentException("A backup group cannot change one path twice.", nameof(changes));
            await session.CheckAsync(change.Expectation, token).ConfigureAwait(false);
            await session.CheckAsync(FileExpectation.Missing(ToolBackupPaths.Beside(change.LogicalPath)), token).ConfigureAwait(false);
        }
        if (session._changes.Any(change => targets.Contains(ToolBackupPaths.Beside(change.LogicalPath))))
            throw new ArgumentException("A backup cannot also be a changed target.", nameof(changes));
        var leaves = targets.Concat(targets.Select(ToolBackupPaths.Beside)).ToHashSet(targets.Comparer);
        foreach (var path in leaves)
            for (var parent = Path.GetDirectoryName(path); parent is not null; parent = Path.GetDirectoryName(parent))
                if (leaves.Contains(parent)) throw new ArgumentException("A changed file or backup cannot be another target's ancestor.", nameof(changes));
        try
        {
            foreach (var change in session._changes.Where(change => change.Expectation.Exists))
            {
                var prior = await ToolFileReader.ReadAsync(lease.Request.Workspace, change.LogicalPath, token).ConfigureAwait(false);
                await session.CheckAsync(change.Expectation, token).ConfigureAwait(false);
                var target = prior.PhysicalPath ?? throw new IOException("The prior file has no physical path.");
                session._priors.Add(change.LogicalPath, prior);
                if (!OperatingSystem.IsWindows()) session._priorModes.Add(change.LogicalPath, File.GetUnixFileMode(target));
                if (session.Mode == ToolBackupMode.Git) continue;
                var backup = ToolBackupPaths.Beside(target);
                File.Copy(target, backup, overwrite: false);
                var saved = await ToolFileReader.ReadAsync(lease.Request.Workspace, ToolBackupPaths.Beside(change.LogicalPath), CancellationToken.None).ConfigureAwait(false);
                session._backups.Add(change.LogicalPath, saved);
                if (saved.ContentHash != change.Expectation.ContentHash) throw new IOException($"The backup differs from the expected prior bytes: {backup}.");
            }
            return session;
        }
        catch
        {
            await session.RestoreAsync().ConfigureAwait(false);
            throw;
        }
    }

    internal async ValueTask ApplyAsync(CancellationToken token, Action<PlannedFileChange>? completed = null)
    {
        RequireLease();
        foreach (var change in _changes)
        {
            var check = await CheckAsync(change.Expectation, token).ConfigureAwait(false);
            if (_backups.TryGetValue(change.LogicalPath, out var backup))
                await CheckAsync(backup.Expectation, token).ConfigureAwait(false);
            _attempted.Add(change.LogicalPath);
            var target = check.PhysicalPath ?? throw new IOException("A checked change requires a physical destination.");
            if (change.Kind == PlannedFileChangeKind.Delete) File.Delete(target);
            else await WriteAsync(change.Expectation, change.IntendedBytes.AsMemory(), token).ConfigureAwait(false);
            await CheckAsync(Intended(change, target), token).ConfigureAwait(false);
            completed?.Invoke(change);
        }
    }

    internal async ValueTask<ToolBackupResidual[]> CompleteAsync(CancellationToken token)
    {
        RequireLease();
        foreach (var change in _changes)
        {
            var physical = change.Expectation.PhysicalPath ?? _validator.ResolvePath(_lease.Request.Workspace, change.LogicalPath).ResolvedPhysicalPath;
            if (change.Kind != PlannedFileChangeKind.Delete && physical is null) throw new IOException("The written file is unavailable.");
            await CheckAsync(Intended(change, physical ?? change.LogicalPath), token).ConfigureAwait(false);
        }
        var residuals = await DeleteBackupsAsync().ConfigureAwait(false);
        _closed = true;
        return residuals;
    }

    private async ValueTask<FileExpectationValidationResult> CheckAsync(FileExpectation expectation, CancellationToken token)
    {
        RequireLease();
        var check = await _validator.ValidateAsync(_lease.Request.Workspace, expectation, token).ConfigureAwait(false);
        if (check.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
        if (check.State != FileExpectationValidationState.Matched) throw new IOException(check.Cause ?? $"The file changed: {expectation.LogicalPath}.");
        return check;
    }

    private void RequireLease()
    {
        if (_closed || !_lease.IsHeldFor(_lease.Request.Workspace)) throw new InvalidOperationException("Tool file changes require their live workspace lease.");
    }

    private static FileExpectation Intended(PlannedFileChange change, string physicalPath)
        => change.Kind == PlannedFileChangeKind.Delete ? FileExpectation.Missing(change.LogicalPath)
            : FileExpectation.File(change.LogicalPath, physicalPath, FileExpectation.Hash(change.IntendedBytes.AsSpan()));

    private ValueTask WriteAsync(FileExpectation expected, ReadOnlyMemory<byte> bytes, CancellationToken token)
    {
        RequireLease();
        UnixFileMode? mode = _priorModes.TryGetValue(expected.LogicalPath, out var prior) ? prior : null;
        return ToolFileReplacement.WriteAsync(_lease.Request.Workspace, new ToolFileWrite(expected, bytes, mode), RequireLease, token);
    }
}
