using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Backups;

[Trait("Feature", "tool-backups"), Trait("Evidence", "Integration")]
public sealed class ToolBackupSessionTests
{
    private const string First = "first.txt";
    private const string Second = "second.txt";
    private static readonly byte[] Original = [0, 127, 255, 13, 10];
    private static readonly byte[] Changed = Encoding.UTF8.GetBytes("changed\n");
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Tool sibling backups protect replacement, deletion and empty prior files until validation"), InlineData(false, false), InlineData(true, false), InlineData(false, true)]
    public async Task ProtectsExistingFiles(bool delete, bool empty)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-existing");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        var prior = empty ? [] : Original;
        temporary.WriteBytes(First, prior);
        await using var lease = await LeaseAsync(locks, workspace);
        var before = await ToolFileReader.ReadAsync(workspace, temporary.Combine(First), Token);
        var change = delete ? PlannedFileChange.Delete(before.Expectation) : PlannedFileChange.Replace(before.Expectation, Changed);
        var session = await ToolBackupSession.PrepareAsync(lease, [change], Token);
        Assert.Equal(prior, await File.ReadAllBytesAsync(ToolBackupPaths.Beside(temporary.Combine(First)), Token));
        await session.ApplyAsync(Token);
        Assert.Equal(!delete, File.Exists(temporary.Combine(First)));
        Assert.True(File.Exists(ToolBackupPaths.Beside(temporary.Combine(First))));
        Assert.Empty(await session.CompleteAsync(Token));
        Assert.False(File.Exists(ToolBackupPaths.Beside(temporary.Combine(First))));
        if (!delete) Assert.Equal(Changed, await File.ReadAllBytesAsync(temporary.Combine(First), Token));
    }

    [Theory(DisplayName = "Tool failed validation restores every prior byte and removes newly created files"), InlineData(false), InlineData(true)]
    public async Task RestoresAfterValidationFailure(bool delete)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-restore");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        temporary.WriteBytes(First, Original);
        await using var lease = await LeaseAsync(locks, workspace);
        var before = await ToolFileReader.ReadAsync(workspace, temporary.Combine(First), Token);
        var existing = delete ? PlannedFileChange.Delete(before.Expectation) : PlannedFileChange.Replace(before.Expectation, Changed);
        var created = PlannedFileChange.Create(FileExpectation.Missing(temporary.Combine(Second)), Changed);
        var session = await ToolBackupSession.PrepareAsync(lease, [existing, created], Token);
        Assert.False(File.Exists(ToolBackupPaths.Beside(temporary.Combine(Second))));
        await session.ApplyAsync(Token);
        Assert.Empty(await session.RestoreAsync());
        Assert.Equal(Original, await File.ReadAllBytesAsync(temporary.Combine(First), Token));
        Assert.False(File.Exists(temporary.Combine(Second)));
        Assert.False(File.Exists(ToolBackupPaths.Beside(temporary.Combine(First))));
    }

    [Theory(DisplayName = "Tool preflight rejects stale targets or existing backups before creating any backup"), InlineData(false), InlineData(true)]
    public async Task ChecksCompleteGroupBeforeEffects(bool existingBackup)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-preflight");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        temporary.WriteBytes(First, Original);
        temporary.WriteBytes(Second, Original);
        await using var lease = await LeaseAsync(locks, workspace);
        var first = await ToolFileReader.ReadAsync(workspace, temporary.Combine(First), Token);
        var second = await ToolFileReader.ReadAsync(workspace, temporary.Combine(Second), Token);
        if (existingBackup) temporary.WriteBytes(ToolBackupPaths.Beside(Second), Changed);
        else temporary.ReplaceBytes(Second, Changed);
        var hashes = temporary.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await ToolBackupSession.PrepareAsync(lease,
            [PlannedFileChange.Replace(first.Expectation, Changed), PlannedFileChange.Replace(second.Expectation, Changed)], Token));
        Assert.Equal(hashes, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Tool restoration keeps changed targets and backups for later recovery"), InlineData(false), InlineData(true)]
    public async Task KeepsUnknownChanges(bool changeBackup)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-changed");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        temporary.WriteBytes(First, Original);
        await using var lease = await LeaseAsync(locks, workspace);
        var before = await ToolFileReader.ReadAsync(workspace, temporary.Combine(First), Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Replace(before.Expectation, Changed)], Token);
        var backupPath = ToolBackupPaths.Beside(temporary.Combine(First));
        try
        {
            await session.ApplyAsync(Token);
            var changedPath = changeBackup ? backupPath : temporary.Combine(First);
            byte[] foreign = [1, 2, 3];
            await File.WriteAllBytesAsync(changedPath, foreign, Token);
            var residual = Assert.Single(await session.RestoreAsync());
            Assert.Equal(temporary.Combine(First), residual.Target);
            Assert.Equal(backupPath, residual.Backup);
            Assert.Equal(foreign, await File.ReadAllBytesAsync(changedPath, Token));
            Assert.True(File.Exists(backupPath));
        }
        finally
        {
            File.Delete(backupPath);
        }
    }

    [Fact(DisplayName = "Tool cancellation after backup preparation restores without leaving a backup")]
    public async Task CancelsBeforeWrite()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-cancel");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        temporary.WriteBytes(First, Original);
        await using var lease = await LeaseAsync(locks, workspace);
        var before = await ToolFileReader.ReadAsync(workspace, temporary.Combine(First), Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Replace(before.Expectation, Changed)], Token);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await session.ApplyAsync(cancelled.Token));
        Assert.Empty(await session.RestoreAsync());
        Assert.Equal(Original, await File.ReadAllBytesAsync(temporary.Combine(First), Token));
        Assert.False(File.Exists(ToolBackupPaths.Beside(temporary.Combine(First))));
    }

    [Fact(DisplayName = "Tool failure during a related file group restores completed effects and keeps later edits")]
    public async Task RestoresPartialGroup()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-partial");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        temporary.WriteBytes(First, Original);
        temporary.WriteBytes(Second, Original);
        await using var lease = await LeaseAsync(locks, workspace);
        var first = await ToolFileReader.ReadAsync(workspace, temporary.Combine(First), Token);
        var second = await ToolFileReader.ReadAsync(workspace, temporary.Combine(Second), Token);
        var session = await ToolBackupSession.PrepareAsync(lease,
            [PlannedFileChange.Replace(first.Expectation, Changed), PlannedFileChange.Replace(second.Expectation, Changed)], Token);
        temporary.ReplaceBytes(Second, [1, 2, 3]);
        await Assert.ThrowsAsync<IOException>(async () => await session.ApplyAsync(Token));
        Assert.Equal(Changed, await File.ReadAllBytesAsync(temporary.Combine(First), Token));
        Assert.Empty(await session.RestoreAsync());
        Assert.Equal(Original, await File.ReadAllBytesAsync(temporary.Combine(First), Token));
        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(temporary.Combine(Second), Token));
        Assert.False(File.Exists(ToolBackupPaths.Beside(temporary.Combine(First))));
        Assert.False(File.Exists(ToolBackupPaths.Beside(temporary.Combine(Second))));
    }

    [Fact(DisplayName = "Tool backup groups reject duplicate or overlapping target and backup paths")]
    public async Task RejectsOverlappingPaths()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-overlap");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        await using var lease = await LeaseAsync(locks, workspace);
        var change = PlannedFileChange.Create(FileExpectation.Missing(temporary.Combine(First)), Changed);
        var backupChange = PlannedFileChange.Create(FileExpectation.Missing(ToolBackupPaths.Beside(temporary.Combine(First))), Changed);
        var before = temporary.SnapshotHashes();
        await Assert.ThrowsAsync<ArgumentException>(async () => await ToolBackupSession.PrepareAsync(lease, [change, change], Token));
        await Assert.ThrowsAsync<ArgumentException>(async () => await ToolBackupSession.PrepareAsync(lease, [change, backupChange], Token));
        var child = PlannedFileChange.Create(FileExpectation.Missing(temporary.Combine(First + "/" + Second)), Changed);
        await Assert.ThrowsAsync<ArgumentException>(async () => await ToolBackupSession.PrepareAsync(lease, [change, child], Token));
        var backupChild = PlannedFileChange.Create(FileExpectation.Missing(temporary.Combine(ToolBackupPaths.Beside(First) + "/" + Second)), Changed);
        await Assert.ThrowsAsync<ArgumentException>(async () => await ToolBackupSession.PrepareAsync(lease, [change, backupChild], Token));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool backup changes require their live workspace lease")]
    public async Task RejectsReleasedLease()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-lease");
        using var locks = WorkspaceLockTestStore.Create("tool-backup-lock");
        var workspace = Selected(temporary);
        var lease = await LeaseAsync(locks, workspace);
        await lease.DisposeAsync();
        var before = temporary.SnapshotHashes();
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await ToolBackupSession.PrepareAsync(lease,
            [PlannedFileChange.Create(FileExpectation.Missing(temporary.Combine(First)), Changed)], Token));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static CliWorkspace Selected(TemporaryWorkspace temporary) => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);

    private static async ValueTask<WorkspaceLockLease> LeaseAsync(WorkspaceLockTestStore locks, CliWorkspace workspace)
        => Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(new(workspace, "tool install", Guid.NewGuid()), Token)).Lease);
}
