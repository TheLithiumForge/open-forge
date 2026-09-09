using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibraryListProcessTests
{
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task AbsentRecordIsCompleteWithoutInventory()
    {
        using var workspace = new PublishedLibraryWorkspace();
        var target = PublishedExecutableTarget.Discover();
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(target, "library", "list", "--json"), "complete");
        var result = document.RootElement.GetProperty("result");
        Assert.Empty(result.GetProperty("libraries").EnumerateArray());
        Assert.Equal("not-requested", result.GetProperty("inventory").GetString());
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task HealthyRecordsAreDeterministic()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Source(".agents/directives/alpha.md");
        workspace.Link();
        workspace.Link(".agents/directives/alpha.md");
        workspace.Write("shared/alpha/.agents/guidance/note.md", "# Note");
        workspace.MappedLink("docs/note.md", "../shared/alpha/.agents/guidance/note.md");
        workspace.Write(PublishedLibraryWorkspace.RecordPath, """
            {"schemaVersion":1,"libraries":[
              {"id":"alpha","sourceRoot":"shared/alpha/.agents/guidance","destinationRoot":"docs","paths":["note.md"]},
              {"id":"team-knowledge","sourceRoot":"shared/team-knowledge","destinationRoot":".","paths":[".agents/directives/alpha.md",".agents/directives/review.md"]}
            ]}
            """);
        var target = PublishedExecutableTarget.Discover();
        var first = await workspace.ReadOnlyAsync(target, "library", "list", "--json");
        var second = await workspace.ReadOnlyAsync(target, "library", "list", "--json");
        using var document = PublishedLibraryWorkspace.Result(first, "complete");
        Assert.Equal(first.ExitCode, second.ExitCode);
        Assert.Equal(first.StandardOutput, second.StandardOutput);
        Assert.Equal(first.StandardError, second.StandardError);
        var libraries = document.RootElement.GetProperty("result").GetProperty("libraries").EnumerateArray().ToArray();
        Assert.Equal(["alpha", "team-knowledge"], libraries.Select(library => library.GetProperty("id").GetString()).ToArray());
        Assert.Equal("docs", libraries[0].GetProperty("destinationRoot").GetString());
        var external = Assert.Single(libraries[0].GetProperty("paths").EnumerateArray());
        Assert.Equal("docs/note.md", external.GetProperty("destinationPath").GetString());
        Assert.Equal(System.Text.Json.JsonValueKind.Null, external.GetProperty("sourceId").ValueKind);
        var library = libraries[1];
        var paths = library.GetProperty("paths").EnumerateArray().ToArray();
        Assert.Equal([".agents/directives/alpha.md", ".agents/directives/review.md"],
            paths.Select(path => path.GetProperty("destinationPath").GetString()).ToArray());
        Assert.Equal(["directives/alpha", "directives/review"],
            paths.Select(path => path.GetProperty("sourceId").GetString()).ToArray());
        Assert.All(paths, path => Assert.Equal("current", path.GetProperty("state").GetString()));
        workspace.AssertNoInfrastructure();
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task MissingProjectionIsAttentionWithoutInventory()
    {
        using var workspace = new PublishedLibraryWorkspace();
        workspace.Source();
        workspace.Record(PublishedLibraryWorkspace.ReviewPath);
        using var document = PublishedLibraryWorkspace.Result(
            await workspace.ReadOnlyAsync(PublishedExecutableTarget.Discover(), "library", "list", "--json"), "attention", 2);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("not-requested", result.GetProperty("inventory").GetString());
        Assert.Contains(result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "library-list.link-missing");
        workspace.AssertNoInfrastructure();
    }
}
