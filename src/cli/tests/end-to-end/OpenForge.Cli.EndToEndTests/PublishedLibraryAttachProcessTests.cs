using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibraryAttachProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task DryRunReportsCompletePlanWithoutEffects()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "attach", "team-knowledge", "shared/team-knowledge", "--dry-run", "--json"), "complete");
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("dry-run", result.GetProperty("identity").GetProperty("mode").GetString());
        Assert.Equal("complete", result.GetProperty("source").GetProperty("inventoryState").GetString());
        var link = Assert.Single(result.GetProperty("plan").GetProperty("links").EnumerateArray());
        Assert.Equal("create", link.GetProperty("kind").GetString());
        Assert.Equal(PublishedLibraryWorkspace.RawReviewTarget, link.GetProperty("rawRelativeTarget").GetString());
        Assert.Equal("create", result.GetProperty("plan").GetProperty("recordEffect").GetString());
        Assert.False(Directory.Exists(workspace.Combine(".agents/directives")));
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyCreatesExactRelativeProjectionAndStrictRecord()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.ConsumerRoute();
        workspace.Source("README.md");
        workspace.GrantDocs();
        try
        {
            using var document = PublishedLibraryWorkspace.Result(
                await workspace.RunAsync(PublishedExecutableTarget.Discover(), "library", "attach", "team-knowledge", "shared/team-knowledge", "--to", "docs", "--json"), "complete");
            Assert.Equal("../shared/team-knowledge/README.md", new FileInfo(workspace.Combine("docs/README.md")).LinkTarget);
            Assert.Null(new DirectoryInfo(workspace.Combine("docs")).LinkTarget);
            using var record = JsonDocument.Parse(File.ReadAllText(workspace.Combine(PublishedLibraryWorkspace.RecordPath)));
            Assert.Equal(["schemaVersion", "libraries"], [.. record.RootElement.EnumerateObject().Select(property => property.Name)]);
            var library = Assert.Single(record.RootElement.GetProperty("libraries").EnumerateArray());
            Assert.Equal(["id", "sourceRoot", "destinationRoot", "paths"], [.. library.EnumerateObject().Select(property => property.Name)]);
            Assert.Equal("team-knowledge", library.GetProperty("id").GetString());
            Assert.Equal("docs", library.GetProperty("destinationRoot").GetString());
            Assert.Equal("README.md", Assert.Single(library.GetProperty("paths").EnumerateArray()).GetString());
            Assert.True(document.RootElement.GetProperty("result").GetProperty("application").GetProperty("recordPublication").GetProperty("publishedLast").GetBoolean());
            Assert.Equal(PublishedLibraryWorkspace.SourceBody, File.ReadAllText(workspace.Combine("shared/team-knowledge/README.md")));
            var parent = File.ReadAllText(workspace.Combine(".agents/directives/_directives.md"));
            Assert.Contains("# Authored prefix", parent, StringComparison.Ordinal);
            Assert.DoesNotContain("(README.md)", parent, StringComparison.Ordinal);
            workspace.AssertAppliedInfrastructure();
        }
        finally
        {
            if (new FileInfo(workspace.Combine("docs/README.md")).LinkTarget == "../shared/team-knowledge/README.md")
            {
                File.Delete(workspace.Combine("docs/README.md"));
            }
            var parent = workspace.Combine("docs");
            if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
            {
                Directory.Delete(parent);
            }
        }
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task OccupiedDestinationBlocksWholeAttach()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Write(PublishedLibraryWorkspace.ReviewPath, "Consumer-owned bytes.");
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "attach", "team-knowledge", "shared/team-knowledge", "--json"), "blocked", 5);
        Assert.False(File.Exists(workspace.Combine(PublishedLibraryWorkspace.RecordPath)));
        workspace.AssertSource();
        workspace.AssertNoInfrastructure();
    }
}
