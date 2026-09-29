using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedFindProcessTests
{
    [Fact(DisplayName = "Published Find bare invocation uses the workspace current directory and default minimal detail"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindBareUsesCurrentDirectoryAndDefaultMinimalDetail()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("Workspace:", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Selected by:", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("result=", result.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(3, NonEmptyLines(result.StandardOutput).Length);
        Assert.Contains("docs", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("guide", result.StandardOutput, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Published Find compact filtering accepts native tag delimiters and returns the exact source"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    [InlineData(" ")]
    [InlineData("=")]
    [InlineData(":")]
    public async Task PublishedFindCompactFilteringReturnsMatch(string delimiter)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--detail=minimal",
                .. (delimiter == " " ? new[] { "--tag", "Architecture" } : [$"--tag{delimiter}Architecture"]),
                "--heading=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Workspace: ", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("  docs  .agents/docs.md", NonEmptyLines(result.StandardOutput));
    }

    [Fact(DisplayName = "Published Find projects the matched source body as JSON"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindJsonProjectsMatchedContent()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.Path, working.SnapshotState,
            ["find", "--tag=Architecture", "--heading=Architecture", "--content=body", "--format=json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var match = Assert.Single(document.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray());
        Assert.Equal("docs", match.GetProperty("id").GetString());
        Assert.Equal(".agents/docs.md", match.GetProperty("path").GetString());
        Assert.Contains(match.GetProperty("parts").EnumerateArray(), part =>
            part.GetProperty("part").GetString() == "body"
            && part.GetProperty("layer").GetString() == "base"
            && part.GetProperty("text").GetString() == "# Architecture\n\n## Target\n\nTarget body\n");
    }

    [Fact(DisplayName = "Published Find filters planned workspace paths and reports normalized applicability"), Trait("Feature", "find-applicability"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindFiltersPlannedPathsAndReportsApplicability()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        File.WriteAllText(
            working.Combine(".agents/docs.md"),
            "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n  applyTo: \"src/**/*.cs\"\n---\n# Architecture\n\n## Target\n\nTarget body\n");

        var relativePlannedPath = Path.Combine("src", "Planned.cs");
        Assert.False(File.Exists(working.Combine(relativePlannedPath)));
        var filtered = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--tag=Architecture", "--for", relativePlannedPath, "--for=docs/Notes.md", "--format=json", "--detail=standard"]);

        Assert.Equal(0, filtered.ExitCode);
        Assert.Equal(string.Empty, filtered.StandardError);
        using (var document = JsonDocument.Parse(filtered.StandardOutput))
        {
            var query = document.RootElement.GetProperty("data").GetProperty("query");
            Assert.Equal(
                ["src/Planned.cs", "docs/Notes.md"],
                query.GetProperty("workingPaths").EnumerateArray().Select(path => path.GetString()));
            var match = Assert.Single(document.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray());
            Assert.Equal(".agents/docs.md", match.GetProperty("path").GetString());
            var applicability = match.GetProperty("applicability");
            Assert.Equal("matched", applicability.GetProperty("state").GetString());
            var condition = Assert.Single(applicability.GetProperty("conditions").EnumerateArray());
            Assert.Equal(".agents/docs.md", condition.GetProperty("source").GetString());
            Assert.Equal(
                ["src/**/*.cs"],
                condition.GetProperty("patterns").EnumerateArray().Select(pattern => pattern.GetString()));
            Assert.Equal(
                ["src/Planned.cs"],
                applicability.GetProperty("matchingPaths").EnumerateArray().Select(path => path.GetString()));
        }

        var pending = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--tag=Architecture", "--format=json", "--detail=standard"]);
        Assert.Equal(0, pending.ExitCode);
        Assert.Equal(string.Empty, pending.StandardError);
        using var pendingDocument = JsonDocument.Parse(pending.StandardOutput);
        var pendingQuery = pendingDocument.RootElement.GetProperty("data").GetProperty("query");
        Assert.False(pendingQuery.TryGetProperty("workingPaths", out _));
        var pendingMatch = Assert.Single(pendingDocument.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray());
        Assert.Equal(
            "pending",
            pendingMatch.GetProperty("applicability").GetProperty("state").GetString());
    }

    private static string[] NonEmptyLines(string output) => output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

}
