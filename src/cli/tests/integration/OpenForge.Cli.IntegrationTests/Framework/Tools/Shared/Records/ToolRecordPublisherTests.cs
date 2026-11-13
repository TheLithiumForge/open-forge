using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

[Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
public sealed class ToolRecordPublisherTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Tool execution states persist matching summaries and details without claiming an installation")]
    [InlineData((int)ToolCommandState.Planned, null)]
    [InlineData((int)ToolCommandState.Started, null)]
    [InlineData((int)ToolCommandState.Exited, 0)]
    [InlineData((int)ToolCommandState.Failed, 7)]
    [InlineData((int)ToolCommandState.Interrupted, null)]
    public async Task PersistsExecutionEvidence(int state, int? exit)
    {
        using var fixture = new ToolRecordFixture();
        var write = await fixture.WriteAsync((ToolCommandState)state, exit, Token);
        await using var lease = await fixture.LeaseAsync(Token);
        var publication = await new ToolRecordPublisher().PublishAsync(lease, write, Token);
        var settings = WorkspaceSettingsCodec.Read(publication.Settings.Bytes.AsMemory()).Document;
        var ownership = WorkspaceOwnershipCodec.Read(publication.Lock.Bytes.AsMemory()).Document;
        Assert.NotNull(settings);
        Assert.NotNull(ownership);
        Assert.Empty(settings.Tools);
        Assert.Empty(ownership.Tools);
        Assert.Equal((ToolCommandState)state, settings.ToolRuns[ToolFixtureSource.Id].State);
        Assert.Equal(exit, ownership.ToolRuns[ToolFixtureSource.Id].Commands[0].ExitCode);
        Assert.Equal(fixture.OriginalOwnership.Framework?.Source, ownership.Framework?.Source);
        using var json = JsonDocument.Parse(publication.Settings.Bytes.AsMemory());
        Assert.Equal(ToolRecordFixture.UserValue, json.RootElement.GetProperty(ToolRecordFixture.UserProperty).GetString());
        Assert.Equal(ToolBackupMode.Sibling, publication.BackupMode);
        Assert.Empty(publication.Residuals);
        Assert.False(File.Exists(ToolBackupPaths.Beside(publication.Settings.LogicalPath)));
        Assert.False(File.Exists(ToolBackupPaths.Beside(publication.Lock.LogicalPath)));
    }

    [Theory(DisplayName = "Cancellation between paired writes restores both prior records using the Git or sibling policy")]
    [InlineData(false), InlineData(true)]
    public async Task RestoresAfterInterruptedPair(bool git)
    {
        using var fixture = new ToolRecordFixture();
        if (git) fixture.Temporary.WriteText(WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + ToolGitRecoveryReader.Head, "ref: refs/heads/fixture\n");
        var write = await fixture.WriteAsync(ToolCommandState.Started, null, Token);
        await using var lease = await fixture.LeaseAsync(Token);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var writes = 0;
        var publisher = new ToolRecordPublisher(_ => { writes++; cancellation.Cancel(); });
        var failure = await Assert.ThrowsAsync<ToolRecordWriteException>(async () => await publisher.PublishAsync(lease, write, cancellation.Token));
        Assert.True(failure.Interrupted);
        Assert.Equal(1, writes);
        Assert.Equal(git ? ToolBackupMode.Git : ToolBackupMode.Sibling, failure.BackupMode);
        Assert.Empty(failure.Residuals);
        Assert.Equal(write.Settings.Bytes.ToArray(), await File.ReadAllBytesAsync(write.Settings.LogicalPath, Token));
        Assert.Equal(write.Lock.Bytes.ToArray(), await File.ReadAllBytesAsync(write.Lock.LogicalPath, Token));
        Assert.False(File.Exists(ToolBackupPaths.Beside(write.Settings.LogicalPath)));
        Assert.False(File.Exists(ToolBackupPaths.Beside(write.Lock.LogicalPath)));
    }

    [Theory(DisplayName = "Recovery keeps a concurrent control edit and reports its exact unfinished restoration")]
    [InlineData(false), InlineData(true)]
    public async Task KeepsConcurrentControlEdit(bool git)
    {
        using var fixture = new ToolRecordFixture();
        if (git) fixture.Temporary.WriteText(WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + ToolGitRecoveryReader.Head, "ref: refs/heads/fixture\n");
        var write = await fixture.WriteAsync(ToolCommandState.Started, null, Token);
        await using var lease = await fixture.LeaseAsync(Token);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var concurrent = Encoding.UTF8.GetBytes("{\"fixture-concurrent\":true}\n");
        var publisher = new ToolRecordPublisher(change =>
        {
            File.WriteAllBytes(change.LogicalPath, concurrent);
            cancellation.Cancel();
        });
        var failure = await Assert.ThrowsAsync<ToolRecordWriteException>(async () => await publisher.PublishAsync(lease, write, cancellation.Token));
        Assert.True(failure.Interrupted);
        Assert.Single(failure.Residuals);
        Assert.Equal(write.Settings.LogicalPath, failure.Residuals[0].Target);
        Assert.Equal(concurrent, await File.ReadAllBytesAsync(write.Settings.LogicalPath, Token));
        Assert.Equal(write.Lock.Bytes.ToArray(), await File.ReadAllBytesAsync(write.Lock.LogicalPath, Token));
        Assert.Equal(!git, File.Exists(ToolBackupPaths.Beside(write.Settings.LogicalPath)));
    }

    [Fact(DisplayName = "First record publication creates only the fixed control parent")]
    public async Task CreatesMissingParent()
    {
        using var fixture = new ToolRecordFixture(existing: false);
        var write = await fixture.WriteAsync(ToolCommandState.Planned, null, Token);
        await using var lease = await fixture.LeaseAsync(Token);
        var published = await new ToolRecordPublisher().PublishAsync(lease, write, Token);
        Assert.Single(published.CreatedDirectories);
        Assert.True(File.Exists(published.Settings.LogicalPath));
        Assert.True(File.Exists(published.Lock.LogicalPath));
        Assert.Empty(published.Residuals);
    }

    [Fact(DisplayName = "Cancellation of first publication removes only its unchanged newly created files and parent")]
    public async Task RemovesCreatedRecordsOnFailure()
    {
        using var fixture = new ToolRecordFixture(existing: false);
        var write = await fixture.WriteAsync(ToolCommandState.Started, null, Token);
        await using var lease = await fixture.LeaseAsync(Token);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        var failure = await Assert.ThrowsAsync<ToolRecordWriteException>(async () => await new ToolRecordPublisher(_ => cancellation.Cancel())
            .PublishAsync(lease, write, cancellation.Token));
        Assert.True(failure.Interrupted);
        Assert.Empty(failure.Residuals);
        Assert.False(Directory.Exists(fixture.Temporary.Combine(WorkspaceSettingsDefinitions.DirectoryName)));
    }

    [Fact(DisplayName = "A changed control record blocks publication before any backup or replacement")]
    public async Task RejectsStaleControlBytes()
    {
        using var fixture = new ToolRecordFixture();
        var write = await fixture.WriteAsync(ToolCommandState.Started, null, Token);
        await File.WriteAllTextAsync(write.Settings.LogicalPath, "{\"fixture-changed\":true}", Token);
        await using var lease = await fixture.LeaseAsync(Token);
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await new ToolRecordPublisher().PublishAsync(lease, write, Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Mismatched execution records block a paired write before effects")]
    public async Task RejectsMismatchedEvidence()
    {
        using var fixture = new ToolRecordFixture();
        var write = await fixture.WriteAsync(ToolCommandState.Started, null, Token);
        write = write with { LockBytes = write.Lock.Bytes.ToArray() };
        await using var lease = await fixture.LeaseAsync(Token);
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAsync<JsonException>(async () => await new ToolRecordPublisher().PublishAsync(lease, write, Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "A leftover sibling backup blocks record changes even in a Git workspace")]
    public async Task KeepsExistingBackup()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Temporary.WriteText(WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + ToolGitRecoveryReader.Head, "ref: refs/heads/fixture\n");
        var write = await fixture.WriteAsync(ToolCommandState.Planned, null, Token);
        fixture.Temporary.WriteBytes(ToolBackupPaths.Beside(WorkspaceOwnershipDefinitions.RelativePath), write.Lock.Bytes.ToArray());
        await using var lease = await fixture.LeaseAsync(Token);
        var before = fixture.Temporary.SnapshotHashes();
        await Assert.ThrowsAsync<IOException>(async () => await new ToolRecordPublisher().PublishAsync(lease, write, Token));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "A null command in an external detailed record is rejected at decode")]
    public async Task RejectsNullCommandRecord()
    {
        using var fixture = new ToolRecordFixture();
        var write = await fixture.WriteAsync(ToolCommandState.Planned, null, Token);
        var root = JsonNode.Parse(write.LockBytes)?.AsObject() ?? throw new InvalidOperationException("The fixture lock is missing.");
        var run = root[WorkspaceSettingsDefinitions.ToolRunsProperty]?[ToolFixtureSource.Id]?.AsObject()
            ?? throw new InvalidOperationException("The fixture run is missing.");
        run[JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolRunRecord.Commands))] = new JsonArray { null };
        var read = WorkspaceOwnershipCodec.Read(Encoding.UTF8.GetBytes(root.ToJsonString()));
        Assert.Null(read.Document);
    }
}
