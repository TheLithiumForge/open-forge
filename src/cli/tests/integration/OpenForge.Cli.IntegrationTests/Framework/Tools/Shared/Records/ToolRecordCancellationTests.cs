using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

[Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
public sealed class ToolRecordCancellationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Cancellation before record publication returns a typed cancellation with no effects")]
    public async Task CancelledBeforePublication()
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(Token);
        cancelled.Cancel();
        var result = await fixture.Service.WriteAsync(fixture.Records.Write(before), cancelled.Token);
        Assert.True(result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.Unfinished);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.False(Directory.Exists(fixture.Records.OpenForgeHome));
        Assert.False(Directory.Exists(fixture.Records.Workspace.Combine(WorkspaceSettingsDefinitions.DirectoryName)));
    }

    [Fact(DisplayName = "Cancellation after a verified record write retains its completed prefix and the unstarted backup handle")]
    public async Task CancelledAfterWrittenPrefix()
    {
        using var fixture = new ToolRecordPublicationFixture();
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var before = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        fixture.Backups.AfterPrepare = (backup, call) => { if (call == 2) cancelled.Cancel(); return backup; };
        var result = await fixture.Service.WriteAsync(fixture.Records.Write(before), cancelled.Token);
        Assert.True(result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Equal(fixture.Records.PortablePath(WorkspaceSettingsDefinitions.RelativePath), Assert.Single(result.WrittenPaths));
        Assert.Equal(2, result.PendingBackups.Count);
        Assert.True(result.PendingBackups[0].Verified);
        Assert.False(result.PendingBackups[1].Verified);
        Assert.Contains(fixture.Records.PortablePath(WorkspaceOwnershipDefinitions.RelativePath), result.Unfinished);
        Assert.Contains(fixture.SelectedPath(runtime: false), result.Unfinished);
        Assert.True(File.Exists(fixture.Records.PortablePath(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.False(File.Exists(fixture.Records.PortablePath(WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.False(Directory.Exists(fixture.Records.OpenForgeHome));
    }

    [Theory(DisplayName = "Failed or cancelled deletion retains the verified deletion prefix and both original sibling backups")]
    [InlineData(false), InlineData(true)]
    public async Task InterruptedDeletionPrefix(bool cancel)
    {
        using var fixture = new ToolRecordPublicationFixture();
        using var cancelled = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var before = await fixture.SeedAsync();
        var tool = fixture.SelectedPath(runtime: false);
        var runtime = fixture.SelectedPath(runtime: true);
        var originals = new Dictionary<string, byte[]>
        {
            [tool] = await File.ReadAllBytesAsync(tool, Token),
            [runtime] = await File.ReadAllBytesAsync(runtime, Token),
        };
        fixture.Backups.AfterPrepare = (backup, call) =>
        {
            if (call != 2) return backup;
            if (cancel) { cancelled.Cancel(); return backup; }
            return backup with { Failure = "synthetic publication failure" };
        };
        var result = await fixture.Service.WriteAsync(fixture.Publication(before) with
        {
            RemovedToolIds = [ToolRecordFixture.Id],
            RemovedRuntimeVersions = [ToolRecordFixture.InitialVersion],
        }, cancelled.Token);
        Assert.NotNull(result.Failure);
        Assert.Equal(cancel, result.Cancelled);
        Assert.Equal(tool, Assert.Single(result.WrittenPaths));
        Assert.False(File.Exists(tool));
        Assert.Equal(originals[runtime], await File.ReadAllBytesAsync(runtime, Token));
        Assert.Contains(runtime, result.Unfinished);
        Assert.DoesNotContain(tool, result.Unfinished);
        Assert.Equal(2, result.PendingBackups.Count);
        Assert.True(result.PendingBackups[0].Verified);
        Assert.False(result.PendingBackups[1].Verified);
        foreach (var pending in result.PendingBackups)
        {
            Assert.Null(pending.ExpectedAppliedSha256);
            var sibling = Assert.IsType<string>(pending.Backup.BackupPath);
            Assert.Contains(sibling, result.Unfinished);
            Assert.Equal(originals[pending.Backup.TargetPath], await File.ReadAllBytesAsync(sibling, Token));
        }
        var after = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        Assert.Empty(after.Machine);
        Assert.Single(after.Runtimes);
    }

    [Theory(DisplayName = "A service cancellation exception is typed as cancellation even before the caller token is signalled")]
    [InlineData(false), InlineData(true)]
    public async Task BackupCancellationException(bool grant)
    {
        using var fixture = new ToolRecordPublicationFixture();
        fixture.Records.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        var before = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        var path = fixture.Records.PortablePath(WorkspaceSettingsDefinitions.RelativePath);
        var bytes = await File.ReadAllBytesAsync(path, Token);
        fixture.Backups.BeforePrepare = () => throw new OperationCanceledException();
        ToolRecordWriteResult result;
        if (grant) result = await fixture.Service.AddAllowInstallPathsAsync(new(fixture.Records.Locations, before, ["synthetic-grant"]), Token);
        else result = await fixture.Service.WriteAsync(fixture.Records.Write(before), Token);
        Assert.False(Token.IsCancellationRequested);
        Assert.True(result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Contains(path, result.Unfinished);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(path, Token));
        Assert.False(File.Exists(fixture.Records.PortablePath(WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.False(Directory.Exists(fixture.Records.OpenForgeHome));
    }
}
