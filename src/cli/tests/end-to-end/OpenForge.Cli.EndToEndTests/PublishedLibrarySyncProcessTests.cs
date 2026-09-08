using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibrarySyncProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task UnchangedInventoryIsAnEffectFreeNoOp()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "sync", "team-knowledge", "--json"), "complete");
        var result = document.RootElement.GetProperty("result");
        Assert.Empty(result.GetProperty("plan").GetProperty("links").EnumerateArray());
        Assert.Equal("none", result.GetProperty("plan").GetProperty("recordEffect").GetString());
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task DryRunThenApplyReconcilesOneAdditionAndRetirement()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link(".agents/directives/old.md");
        workspace.Record(".agents/directives/old.md");
        var target = PublishedExecutableTarget.Discover();
        using var preview = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(target, "library", "sync", "team-knowledge", "--dry-run", "--json"), "complete");
        workspace.AssertNoInfrastructure();
        using var applied = PublishedLibraryWorkspace.Result(
            await workspace.RunAsync(target, "library", "sync", "team-knowledge", "--json"), "complete");
        Assert.Equal(preview.RootElement.GetProperty("result").GetProperty("plan").GetRawText(),
            applied.RootElement.GetProperty("result").GetProperty("plan").GetRawText());
        Assert.Null(new FileInfo(workspace.Combine(".agents/directives/old.md")).LinkTarget);
        Assert.Equal(PublishedLibraryWorkspace.RawReviewTarget, new FileInfo(workspace.Combine(PublishedLibraryWorkspace.ReviewPath)).LinkTarget);
        using var record = JsonDocument.Parse(File.ReadAllText(workspace.Combine(PublishedLibraryWorkspace.RecordPath)));
        Assert.Equal(PublishedLibraryWorkspace.ReviewPath,
            Assert.Single(Assert.Single(record.RootElement.GetProperty("libraries").EnumerateArray()).GetProperty("paths").EnumerateArray()).GetString());
        workspace.AssertSource();
        workspace.AssertAppliedInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ChangedOccupantBlocksWholeSync()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Source(".agents/directives/new.md");
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        workspace.Write(PublishedLibraryWorkspace.ReviewPath, "Changed consumer occupant.");
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "sync", "team-knowledge", "--json"), "blocked", 5);
        Assert.False(File.Exists(workspace.Combine(".agents/directives/new.md")));
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }
}
