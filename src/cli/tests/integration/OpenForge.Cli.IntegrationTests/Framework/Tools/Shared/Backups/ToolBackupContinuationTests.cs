using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Backups;

[Trait("Feature", "tool-backups"), Trait("Evidence", "Integration")]
public sealed class ToolBackupContinuationTests
{
    private const string Target = "sequence.bin";
    private const string Other = "other.bin";
    private static readonly byte[] Original = [0, 127, 255];
    private static readonly byte[] First = [1, 2];
    private static readonly byte[] Second = [3, 4];
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Sequential edits keep the first backup until the complete result verifies")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task KeepsFirstBackup(bool git, bool restore)
    {
        using var fixture = new ToolRecordFixture();
        fixture.Temporary.WriteBytes(Target, Original);
        if (git) fixture.Temporary.WriteText(WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + ToolGitRecoveryReader.Head, "ref: refs/heads/fixture\n");
        using var lease = await fixture.LeaseAsync(Token);
        var before = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Replace(before.Expectation, First)], Token);
        await session.ApplyAsync(Token);
        var intermediate = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        await session.ContinueAsync([PlannedFileChange.Replace(intermediate.Expectation, Second)], Token);
        var backup = fixture.Temporary.Combine(ToolBackupPaths.Beside(Target));
        Assert.Equal(!git, File.Exists(backup));
        if (!git) Assert.Equal(Original, await File.ReadAllBytesAsync(backup, Token));
        if (restore) Assert.Empty(await session.RestoreAsync());
        else Assert.Empty(await session.CompleteAsync(Token));
        Assert.Equal(restore ? Original : Second, await File.ReadAllBytesAsync(fixture.Temporary.Combine(Target), Token));
        Assert.False(File.Exists(backup));
    }

    [Fact(DisplayName = "Sequential recovery removes a file created by the first managed edit")]
    public async Task RestoresCreatedFile()
    {
        using var fixture = new ToolRecordFixture();
        using var lease = await fixture.LeaseAsync(Token);
        var before = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Create(before.Expectation, First)], Token);
        await session.ApplyAsync(Token);
        var intermediate = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        await session.ContinueAsync([PlannedFileChange.Replace(intermediate.Expectation, Second)], Token);
        Assert.Empty(await session.RestoreAsync());
        Assert.False(File.Exists(fixture.Temporary.Combine(Target)));
    }

    [Fact(DisplayName = "A file created and then deleted in one managed group finishes with no target")]
    public async Task CompletesCreatedThenDeletedFile()
    {
        using var fixture = new ToolRecordFixture();
        using var lease = await fixture.LeaseAsync(Token);
        var before = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Create(before.Expectation, First)], Token);
        await session.ApplyAsync(Token);
        var intermediate = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        await session.ContinueAsync([PlannedFileChange.Delete(intermediate.Expectation)], Token);
        Assert.Empty(await session.CompleteAsync(Token));
        Assert.False(File.Exists(fixture.Temporary.Combine(Target)));
    }

    [Fact(DisplayName = "Continuation refuses a new target that wasn't covered by the original backup group")]
    public async Task RejectsNewTarget()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Temporary.WriteBytes(Target, Original);
        fixture.Temporary.WriteBytes(Other, Original);
        using var lease = await fixture.LeaseAsync(Token);
        var before = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Replace(before.Expectation, First)], Token);
        await session.ApplyAsync(Token);
        var other = await ToolFileReader.ReadAsync(fixture.Workspace, Other, Token);
        await Assert.ThrowsAsync<ArgumentException>(() => session.ContinueAsync([PlannedFileChange.Replace(other.Expectation, Second)], Token).AsTask());
        Assert.Equal(Original, await File.ReadAllBytesAsync(fixture.Temporary.Combine(Other), Token));
        Assert.Empty(await session.RestoreAsync());
    }

    [Fact(DisplayName = "An incomplete initial file group can't continue with another managed edit")]
    public async Task RejectsIncompleteInitialGroup()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Temporary.WriteBytes(Target, Original);
        fixture.Temporary.WriteBytes(Other, Original);
        using var lease = await fixture.LeaseAsync(Token);
        var before = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        var other = await ToolFileReader.ReadAsync(fixture.Workspace, Other, Token);
        var session = await ToolBackupSession.PrepareAsync(lease,
            [PlannedFileChange.Replace(before.Expectation, First), PlannedFileChange.Replace(other.Expectation, First)], Token);
        await Assert.ThrowsAsync<IOException>(() => session.ApplyAsync(Token, _ => throw new IOException()).AsTask());
        var intermediate = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.ContinueAsync([PlannedFileChange.Replace(intermediate.Expectation, Second)], Token).AsTask());
        Assert.Empty(await session.RestoreAsync());
        Assert.Equal(Original, await File.ReadAllBytesAsync(fixture.Temporary.Combine(Target), Token));
        Assert.Equal(Original, await File.ReadAllBytesAsync(fixture.Temporary.Combine(Other), Token));
    }

    [Fact(DisplayName = "A changed target blocks continuation and recovery keeps the unfamiliar bytes")]
    public async Task KeepsForeignChange()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Temporary.WriteBytes(Target, Original);
        using var lease = await fixture.LeaseAsync(Token);
        var before = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        var session = await ToolBackupSession.PrepareAsync(lease, [PlannedFileChange.Replace(before.Expectation, First)], Token);
        await session.ApplyAsync(Token);
        var intermediate = await ToolFileReader.ReadAsync(fixture.Workspace, Target, Token);
        byte[] foreign = [9];
        await File.WriteAllBytesAsync(fixture.Temporary.Combine(Target), foreign, Token);
        try
        {
            await Assert.ThrowsAsync<IOException>(() => session.ContinueAsync([PlannedFileChange.Replace(intermediate.Expectation, Second)], Token).AsTask());
            Assert.Single(await session.RestoreAsync());
            Assert.Equal(foreign, await File.ReadAllBytesAsync(fixture.Temporary.Combine(Target), Token));
        }
        finally { File.Delete(fixture.Temporary.Combine(ToolBackupPaths.Beside(Target))); }
    }
}
