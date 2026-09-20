using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Status;
using OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

public sealed class IndexEntriesPreservationTests
{
    private const string MapsPath = ".agents/maps/_maps.md";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Index preserves authored prose around the first Entries list and is stable on a second run")]
    [InlineData("\n", false)]
    [InlineData("\n", true)]
    [InlineData("\r\n", false)]
    [InlineData("\r\n", true)]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task AuthoredEntriesProseSurvives(string lineEnding, bool proseBefore)
    {
        using var workspace = IndexOperationWorkspace.Create("index-entries-prose");
        var prefix = "# Root\n\n## Entries\n\n" + (proseBefore ? "A note before.\n\n" : string.Empty);
        const string suffix = "\n\nA note.\n\n- An authored second list.\n\n### More notes\n\nKeep this too.\n";
        var before = (prefix + "- stale" + suffix).Replace("\n", lineEnding, StringComparison.Ordinal);
        var expected = (prefix + IndexOperationWorkspace.ExpectedEntry + suffix).Replace("\n", lineEnding, StringComparison.Ordinal);
        workspace.ReplaceRootText(before);

        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);
        var beforeHashes = workspace.SnapshotHashes();
        var preview = await operation.ExecuteAsync(workspace.Request(IndexMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        var previewChange = Assert.Single(preview.Regions).Change;
        Assert.NotNull(previewChange);
        Assert.Equal($"- stale{lineEnding}", previewChange.BeforeBody);
        Assert.Equal($"{IndexOperationWorkspace.ExpectedEntry}{lineEnding}", previewChange.ExpectedBody);
        Assert.Equal(beforeHashes, workspace.SnapshotHashes());
        var first = await operation.ExecuteAsync(workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);

        Assert.True(first.Status == CliSemanticStatus.Complete,
            string.Join("\n", first.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.Equal(expected, await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        var hashes = workspace.SnapshotHashes();

        var second = await operation.ExecuteAsync(workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Equal(hashes, workspace.SnapshotHashes());
        Assert.Equal(expected, await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Index inserts a missing Entries list before authored prose and remains stable")]
    [InlineData("\n", false)]
    [InlineData("\n", true)]
    [InlineData("\r\n", false)]
    [InlineData("\r\n", true)]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task MissingListIsInserted(string lineEnding, bool hasProse)
    {
        using var workspace = IndexOperationWorkspace.Create("index-insert-entries");
        var authored = hasProse ? "Authored prose.\n" : string.Empty;
        workspace.ReplaceRootText(("# Root\n\n## Entries\n\n" + authored).Replace("\n", lineEnding, StringComparison.Ordinal));
        var expected = ("# Root\n\n## Entries\n\n" + IndexOperationWorkspace.ExpectedEntry + "\n\n" + authored)
            .Replace("\n", lineEnding, StringComparison.Ordinal);
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);
        var first = await operation.ExecuteAsync(workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);

        Assert.True(first.Status == CliSemanticStatus.Complete,
            string.Join("\n", first.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.Equal(expected, await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        var hashes = workspace.SnapshotHashes();
        var second = await operation.ExecuteAsync(workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Equal(hashes, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Retired guard removal keeps a second authored list separate on both Index runs")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task RetiredGuardKeepsAuthoredListSeparate(string lineEnding)
    {
        using var workspace = IndexOperationWorkspace.Create("index-guard-list-separator");
        const string before = "# Root\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- stale\n<!-- open-forge:generated-index:end -->\n- An authored second list.\n";
        var expected = ("# Root\n\n## Entries\n\n\n" + IndexOperationWorkspace.ExpectedEntry + "\n\n- An authored second list.\n")
            .Replace("\n", lineEnding, StringComparison.Ordinal);
        workspace.ReplaceRootText(before.Replace("\n", lineEnding, StringComparison.Ordinal));
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);

        var first = await operation.ExecuteAsync(workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(expected, await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
        var hashes = workspace.SnapshotHashes();

        var second = await operation.ExecuteAsync(workspace.Request(IndexMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Equal(hashes, workspace.SnapshotHashes());
        Assert.Equal(expected, await workspace.ReadRootAsync(TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Status and Update agree about authored Entries prose before and after Index")]
    [InlineData("\n", false)]
    [InlineData("\n", true)]
    [InlineData("\r\n", false)]
    [InlineData("\r\n", true)]
    [Trait("Feature", "index-command"), Trait("Evidence", "Integration")]
    public async Task InstalledMapsKeepAuthoredDifferences(string lineEnding, bool proseBefore)
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("index-maps-authored-note");
        var original = File.ReadAllText(workspace.Combine(MapsPath)).Replace("\r\n", "\n", StringComparison.Ordinal);
        Assert.Contains("- none - No entries - #Empty", original, StringComparison.Ordinal);
        var expected = proseBefore
            ? original.Replace("## Entries\n\n", "## Entries\n\nA note before.\n\n", StringComparison.Ordinal)
            : original + "\nA note.\n";
        var stale = expected.Replace("- none - No entries - #Empty", "- [Stale](stale.md) - #Map", StringComparison.Ordinal);
        expected = expected.Replace("\n", lineEnding, StringComparison.Ordinal);
        workspace.OverwriteInstalledText(MapsPath, stale.Replace("\n", lineEnding, StringComparison.Ordinal));

        await AssertAuthoredDifferenceAsync(workspace);
        var operation = IndexOperationFactory.Create(workspace.LockStoreRoot);
        var request = new IndexRequest(workspace.Workspace, [MapsPath], IndexMode.Apply);
        var first = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(expected, File.ReadAllText(workspace.Combine(MapsPath)));
        await AssertAuthoredDifferenceAsync(workspace);
        var hashes = workspace.SnapshotHashes();

        var second = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Equal(hashes, workspace.SnapshotHashes());
    }

    private static async Task AssertAuthoredDifferenceAsync(StatusIntegrationWorkspace workspace)
    {
        var before = workspace.SnapshotHashes();
        var status = await StatusIntegrationApplication.RunAsync(workspace, "status", "--format", "json", "--detail", "full");
        Assert.Equal(2, status.ExitCode);
        using var json = StatusIntegrationApplication.ParseJson(status);
        var result = StatusJsonAssertions.Result(json.RootElement);
        var targets = result.GetProperty("frameworkFiles").EnumerateArray();
        var target = Assert.Single(targets, item => item.GetProperty("path").GetString() == MapsPath
            && item.GetProperty("state").GetString() == "changed");

        var update = await UpdateOperationFactory.Create(
                UpdateInteractionTestSupport.Unavailable(), workspace.LockStoreRoot)
            .ExecuteAsync(new UpdateRequest(workspace.Workspace, UpdateMode.DryRun,
                force: true, prune: false, automatic: true, allowsInteractiveConfirmation: false), TestContext.Current.CancellationToken);
        var comparison = Assert.Single(update.Comparisons, item => item.RelativePath == MapsPath
            && item.Kind == UpdateComparisonTargetKind.File);
        Assert.Equal(UpdateComparisonCurrentState.Changed, comparison.CurrentState);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
