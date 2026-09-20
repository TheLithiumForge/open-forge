using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Presentation.Index.Shared.Help;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class IndexBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<IndexResult> Renderers = CommandOutputRenderers<IndexResult>.From(OpenForge.Cli.Core.Presentation.Index.IndexPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index output preserves partial progress after a later Windows replacement is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = IndexOperationWorkspace.CreateMultiTarget("index-output-partial");
        var betaBefore = await workspace.ReadTargetAsync(IndexOperationWorkspace.BetaPath, TestContext.Current.CancellationToken);
        IndexResult result;
        using (var held = File.Open(Path.Combine(workspace.Workspace.PhysicalRoot, IndexOperationWorkspace.BetaPath),
                   FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                new IndexRequest(workspace.Workspace, [IndexOperationWorkspace.AlphaPath, IndexOperationWorkspace.BetaPath], IndexMode.Apply),
                TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Contains(IndexOperationWorkspace.AlphaExpectedEntry,
            await workspace.ReadTargetAsync(IndexOperationWorkspace.AlphaPath, TestContext.Current.CancellationToken), StringComparison.Ordinal);
        Assert.Equal(betaBefore, await workspace.ReadTargetAsync(IndexOperationWorkspace.BetaPath, TestContext.Current.CancellationToken));
        Assert.Equal(Core.Commands.Index.Models.Operation.IndexRecoveryState.Retained, result.Recovery.State);
        Assert.True(File.Exists(result.Recovery.ResidualPath));
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index output preserves incomplete discovery when a child cannot be read")]
    public async Task UnreadableChild()
    {
        using var workspace = IndexOperationWorkspace.Create("index-output-unreadable");
        var before = workspace.SnapshotHashes();
        IndexResult result;
        using (var held = File.Open(Path.Combine(workspace.Workspace.PhysicalRoot, IndexOperationWorkspace.ChildPath),
                   FileMode.Open, FileAccess.Read, FileShare.None))
        {
            result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "incomplete-unreadable-child");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Index output preserves real generated entries updates and previews")]
    [InlineData("all-current")]
    [InlineData("one-stale")]
    [InlineData("dry-run-one-stale")]
    [InlineData("explicit-source")]
    public async Task GeneratedEntries(string situation)
    {
        using var workspace = IndexOperationWorkspace.Create("index-output", situation == "all-current" ? IndexOperationWorkspace.ExpectedEntry : "- stale");
        workspace.ReplaceRootText("---\nopen-forge:\n  description: Root\n  tags: [Docs]\n---\n" + await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        workspace.SeedLoader();
        if (situation == "all-current")
        {
            var established = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
                new IndexRequest(workspace.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, established.Status);
        }

        var before = workspace.SnapshotHashes();
        var mode = situation == "dry-run-one-stale" ? IndexMode.DryRun : IndexMode.Apply;
        var request = new IndexRequest(workspace.Workspace,
            situation == "explicit-source" ? [IndexOperationWorkspace.RootPath] : [], mode);
        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        if (situation is "all-current" or "dry-run-one-stale")
        {
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        else
        {
            Assert.Contains(IndexOperationWorkspace.ExpectedEntry, await workspace.ReadRootAsync(TestContext.Current.CancellationToken), StringComparison.Ordinal);
        }

        Renderers.MatchDetails(result, situation, testName: $"{nameof(GeneratedEntries)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Index output preserves invalid selection and unsafe metadata without effects")]
    [InlineData("folder-operand", (int)CliSemanticStatus.Invalid)]
    [InlineData("unknown-source", (int)CliSemanticStatus.Invalid)]
    [InlineData("blocked-malformed-leaf", (int)CliSemanticStatus.Blocked)]
    public async Task InvalidSelectionOrMetadata(string situation, int status)
    {
        using var workspace = IndexOperationWorkspace.Create("index-output-invalid");
        if (situation == "blocked-malformed-leaf")
        {
            workspace.ReplaceChildBytes([0xc3, 0x28]);
        }

        var reference = situation switch
        {
            "folder-operand" => ".agents/root",
            "unknown-source" => "missing",
            _ => IndexOperationWorkspace.RootPath,
        };
        var before = workspace.SnapshotHashes();
        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            new IndexRequest(workspace.Workspace, [reference], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.NotEmpty(result.Findings);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, situation, testName: $"{nameof(InvalidSelectionOrMetadata)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index output preserves a held mutation lock without effects")]
    public async Task LockHeld()
    {
        using var workspace = IndexOperationWorkspace.Create("index-output-lock");
        var before = workspace.SnapshotHashes();
        await using var held = workspace.HoldLock();
        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "lock-held");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Index output preserves cancellation before discovery without effects")]
    public async Task Cancelled()
    {
        using var workspace = IndexOperationWorkspace.Create("index-output-cancelled");
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await IndexOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(workspace.Request(IndexMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "cancelled");
    }
}
