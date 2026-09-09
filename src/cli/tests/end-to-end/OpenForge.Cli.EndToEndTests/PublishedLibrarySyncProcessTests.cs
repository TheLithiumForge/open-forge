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
        workspace.Source("future/new.md");
        workspace.MappedLink("docs/old.md", "../shared/team-knowledge/old.md");
        workspace.RecordAt("docs", "old.md");
        workspace.GrantDocs();
        var target = PublishedExecutableTarget.Discover();
        using var preview = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(target, "library", "sync", "team-knowledge", "--dry-run", "--json"), "complete");
        workspace.AssertNoInfrastructure();
        try
        {
            using var applied = PublishedLibraryWorkspace.Result(
                await workspace.RunAsync(target, "library", "sync", "team-knowledge", "--json"), "complete");
            Assert.Equal(preview.RootElement.GetProperty("result").GetProperty("plan").GetRawText(),
                applied.RootElement.GetProperty("result").GetProperty("plan").GetRawText());
            Assert.Null(new FileInfo(workspace.Combine("docs/old.md")).LinkTarget);
            Assert.Equal("../../shared/team-knowledge/future/new.md", new FileInfo(workspace.Combine("docs/future/new.md")).LinkTarget);
            using var record = JsonDocument.Parse(File.ReadAllText(workspace.Combine(PublishedLibraryWorkspace.RecordPath)));
            Assert.Equal("future/new.md",
                Assert.Single(Assert.Single(record.RootElement.GetProperty("libraries").EnumerateArray()).GetProperty("paths").EnumerateArray()).GetString());
            Assert.Equal(PublishedLibraryWorkspace.SourceBody, File.ReadAllText(workspace.Combine("shared/team-knowledge/future/new.md")));
            workspace.AssertAppliedInfrastructure();
        }
        finally
        {
            if (new FileInfo(workspace.Combine("docs/future/new.md")).LinkTarget == "../../shared/team-knowledge/future/new.md")
            {
                File.Delete(workspace.Combine("docs/future/new.md"));
            }
            var parent = workspace.Combine("docs/future");
            if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
            {
                Directory.Delete(parent);
            }
        }
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
