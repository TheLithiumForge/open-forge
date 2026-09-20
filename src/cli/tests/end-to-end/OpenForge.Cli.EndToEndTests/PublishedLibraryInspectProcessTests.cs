using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibraryInspectProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task HealthyProjectionHasCompleteInventoryAndDestinationIdentity()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source("_guide.md");
        workspace.MappedLink("docs/_guide.md", "../shared/team-knowledge/_guide.md");
        workspace.RecordAt("docs", "_guide.md");
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "inspect", "team-knowledge", "--format=json", "--detail=full"), "completed");
        var result = document.RootElement.GetProperty("data");
        Assert.True(result.GetProperty("current").GetBoolean());

        // File rows are selected from standard upward, and full adds their link targets.
        var comparison = Assert.Single(result.GetProperty("files").EnumerateArray());
        Assert.Equal("current", comparison.GetProperty("relation").GetString());
        Assert.Equal("docs/_guide.md", comparison.GetProperty("destinationPath").GetString());
        Assert.Equal("_guide.md", comparison.GetProperty("sourcePath").GetString());
        Assert.Equal(
            comparison.GetProperty("expectedTarget").GetString(),
            comparison.GetProperty("observedTarget").GetString());
        Assert.Equal(PublishedLibraryWorkspace.SourceBody, File.ReadAllText(workspace.Combine("shared/team-knowledge/_guide.md")));
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task RequiredIdOmissionIsInvalidWithoutObservation()
    {
        using var workspace = new PublishedLibraryWorkspace();
        var response = await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "inspect");
        Assert.True(response.ExitCode == 4,
            $"Expected exit 4, actual {response.ExitCode}.\nStandard error:\n{response.StandardError}\nStandard output:\n{response.StandardOutput}");
        Assert.Equal(string.Empty, response.StandardOutput);
        Assert.Contains("Cannot inspect", response.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge library list", response.StandardError, StringComparison.Ordinal);

        // Finding codes reach text only from full detail; JSON carries them at every level.
        var detailed = await workspace.ReadOnlyAsync(
            PublishedExecutableTarget.Discover(), "library", "inspect", "--detail=full");
        Assert.Equal(4, detailed.ExitCode);
        Assert.Contains("library-inspect.invalid-id", detailed.StandardError, StringComparison.Ordinal);
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task CompleteInventoryExplainsAdditionRetirementAndMissingProjection()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Source(".agents/directives/new.md");
        workspace.Link(".agents/directives/old.md");
        workspace.Record(".agents/directives/old.md", PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "inspect", "team-knowledge", "--format=json"), "completed-with-warnings", 2);
        var comparisons = document.RootElement.GetProperty("data").GetProperty("files").EnumerateArray().ToArray();
        Assert.Equal(["added", "retired", "missing"], comparisons.Select(row => row.GetProperty("relation").GetString()).ToArray());
        Assert.Equal([".agents/directives/new.md", ".agents/directives/old.md", ".agents/directives/review.md"],
            comparisons.Select(row => row.GetProperty("destinationPath").GetString()).ToArray());
        workspace.AssertNoInfrastructure();
    }
}
