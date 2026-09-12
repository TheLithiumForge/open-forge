using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedFindProcessTests
{
    [Fact(DisplayName = "Published Find bare invocation uses the workspace current directory and default expanded view"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindBareUsesCurrentDirectoryAndDefaultExpandedView()
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
        Assert.Contains($"Workspace: {working.Path}", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Selected by: current directory", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Coverage: complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Found 3 matching sources.", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("docs", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("guide", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("result=complete", result.StandardOutput, StringComparison.Ordinal);
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
            ["find", "--workspace", working.Path, "--view=compact",
                .. (delimiter == " " ? new[] { "--tag", "Architecture" } : [$"--tag{delimiter}Architecture"]),
                "--heading=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Equal(
            [
                "result=complete\tcoverage=complete\tuniverse=default\tmatches=1",
                "docs\t.agents/docs.md",
            ],
            NonEmptyLines(result.StandardOutput));
    }

    [Fact(DisplayName = "Published Find projects the matched source body as JSON"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindJsonProjectsMatchedContent()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.Path, working.SnapshotState,
            ["find", "--tag=Architecture", "--heading=Architecture", "--content=body", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var match = Assert.Single(document.RootElement.GetProperty("result").GetProperty("matches").EnumerateArray());
        Assert.Equal("docs", match.GetProperty("id").GetString());
        Assert.Equal(".agents/docs.md", match.GetProperty("path").GetString());
        Assert.Contains(match.GetProperty("projections").EnumerateArray(), projection =>
            projection.GetProperty("layer").GetString() == "base"
            && projection.GetProperty("text").GetString() == "# Architecture\n\n## Target\n\nTarget body\n");
    }

    private static string[] NonEmptyLines(string output) => output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
}
