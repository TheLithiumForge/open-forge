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

    private static string[] NonEmptyLines(string output) => output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

}
