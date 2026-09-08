using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibraryDetachProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task DryRunReportsExactDeletionWithoutEffects()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "detach", "team-knowledge", "--dry-run", "--json"), "complete");
        var result = document.RootElement.GetProperty("result");
        Assert.True(result.GetProperty("identity").GetProperty("sourceIndependent").GetBoolean());
        var link = Assert.Single(result.GetProperty("plan").GetProperty("links").EnumerateArray());
        Assert.Equal("delete", link.GetProperty("kind").GetString());
        Assert.Equal(PublishedLibraryWorkspace.RawReviewTarget, link.GetProperty("rawRelativeTarget").GetString());
        Assert.Equal("delete", result.GetProperty("plan").GetProperty("recordEffect").GetString());
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyRemovesExactAndDanglingLinksBeforeLastRecord()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Link(".agents/directives/retired.md");
        workspace.Record(PublishedLibraryWorkspace.ReviewPath, ".agents/directives/retired.md");
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.RunAsync(PublishedExecutableTarget.Discover(), "library", "detach", "team-knowledge", "--json"), "complete");
        Assert.Null(new FileInfo(workspace.Combine(PublishedLibraryWorkspace.ReviewPath)).LinkTarget);
        Assert.Null(new FileInfo(workspace.Combine(".agents/directives/retired.md")).LinkTarget);
        Assert.False(File.Exists(workspace.Combine(PublishedLibraryWorkspace.RecordPath)));
        Assert.True(document.RootElement.GetProperty("result").GetProperty("application").GetProperty("recordPublication").GetProperty("publishedLast").GetBoolean());
        workspace.AssertSource();
        workspace.AssertAppliedInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ChangedOccupantPreservesEveryProjectionAndRecord()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link(".agents/directives/alpha.md");
        workspace.Write(PublishedLibraryWorkspace.ReviewPath, "Changed consumer occupant.");
        workspace.Record(".agents/directives/alpha.md", PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "detach", "team-knowledge", "--json"), "blocked", 5);
        Assert.NotNull(new FileInfo(workspace.Combine(".agents/directives/alpha.md")).LinkTarget);
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }
}
