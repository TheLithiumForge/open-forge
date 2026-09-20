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
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "detach", "team-knowledge", "--dry-run", "--detail=standard", "--format=json"), "completed");
        var result = document.RootElement.GetProperty("data");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal("team-knowledge", result.GetProperty("id").GetString());
        Assert.False(result.GetProperty("registrationRemoved").GetBoolean());
        var link = Assert.Single(result.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("kind").GetString() == "link");
        Assert.Equal(PublishedLibraryWorkspace.RawReviewTarget, link.GetProperty("target").GetString());
        Assert.Equal("delete", link.GetProperty("action").GetString());
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyRemovesExactAndDanglingLinksBeforeLastRecord()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source("review.md");
        workspace.MappedLink("docs/review.md", "../shared/team-knowledge/review.md");
        workspace.MappedLink("docs/retired.md", "../shared/team-knowledge/retired.md");
        workspace.RecordAt("docs", "retired.md", "review.md");
        workspace.GrantDocs();
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.RunAsync(PublishedExecutableTarget.Discover(), "library", "detach", "team-knowledge", "--automatic", "--detail=standard", "--format=json"), "completed");
        Assert.Null(new FileInfo(workspace.Combine("docs/review.md")).LinkTarget);
        Assert.Null(new FileInfo(workspace.Combine("docs/retired.md")).LinkTarget);
        using var ownership = System.Text.Json.JsonDocument.Parse(File.ReadAllText(workspace.Combine(PublishedLibraryWorkspace.OwnershipPath)));
        Assert.Empty(ownership.RootElement.GetProperty("libraries").EnumerateArray());
        Assert.True(document.RootElement.GetProperty("data").GetProperty("registrationRemoved").GetBoolean());
        Assert.Equal(PublishedLibraryWorkspace.SourceBody, File.ReadAllText(workspace.Combine("shared/team-knowledge/review.md")));
        workspace.AssertAppliedInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ChangedOccupantPreservesEveryProjectionAndRecordWithoutConfirmation()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link(".agents/directives/alpha.md");
        workspace.Write(PublishedLibraryWorkspace.ReviewPath, "Changed consumer occupant.");
        workspace.Record(".agents/directives/alpha.md", PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "detach", "team-knowledge", "--format=json"), "invalid-input", 4);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "library-detach.confirmation-required");
        Assert.NotNull(new FileInfo(workspace.Combine(".agents/directives/alpha.md")).LinkTarget);
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }
}
