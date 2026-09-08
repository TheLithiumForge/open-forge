using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibraryInspectProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task HealthyProjectionHasCompleteInventoryAndDestinationIdentity()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "inspect", "team-knowledge", "--json"), "complete");
        var result = document.RootElement.GetProperty("result");
        var comparison = Assert.Single(result.GetProperty("projection").GetProperty("comparisons").EnumerateArray());
        Assert.Equal("current", comparison.GetProperty("relation").GetString());
        Assert.Equal("directives/review", comparison.GetProperty("sourceId").GetString());
        workspace.AssertSource();
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
        Assert.Contains("invalid", response.StandardError, StringComparison.Ordinal);
        Assert.Contains("library inspect", response.StandardError, StringComparison.Ordinal);
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
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "inspect", "team-knowledge", "--json"), "attention", 2);
        var comparisons = document.RootElement.GetProperty("result").GetProperty("projection").GetProperty("comparisons").EnumerateArray().ToArray();
        Assert.Equal(["added", "retired", "missing"], comparisons.Select(row => row.GetProperty("relation").GetString()).ToArray());
        Assert.Equal([".agents/directives/new.md", ".agents/directives/old.md", ".agents/directives/review.md"],
            comparisons.Select(row => row.GetProperty("destinationPath").GetString()).ToArray());
        workspace.AssertNoInfrastructure();
    }
}
