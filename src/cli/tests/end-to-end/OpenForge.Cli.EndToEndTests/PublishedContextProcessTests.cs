using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedContextProcessTests
{
    [Fact(DisplayName = "Published Context default startup closure is ordered human output on stdout"),
     Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextDefaultStartupClosureIsOrderedAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "--content=metadata", "--view=expanded"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Result: complete", result.StandardOutput, StringComparison.Ordinal);
        AssertOrdered(
            result.StandardOutput,
            "Path: AGENTS.md",
            "Path: .agents/loader.md",
            "Path: .agents/startup/_startup.md",
            "Path: .agents/startup/topic.md");
    }

    [Fact(DisplayName = "Published Context JSON retains additions, layers, links, and outside Markdown in canonical order"),
     Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextJsonRetainsExpandedGraph()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "context",
                "projects/guide",
                "--workspace", working.Path,
                "--additions-only",
                "--content=metadata",
                "--follow-links=all",
                "--json",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("context", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Equal(
            [
                ".agents/projects/_projects.md",
                ".agents/projects/guide.md",
                ".agents/projects/linked.md",
                "README.md",
            ],
            commandResult.GetProperty("sources").EnumerateArray()
                .Select(source => source.GetProperty("path").GetString()));
        Assert.Equal(
            ["base", "overwrite"],
            commandResult.GetProperty("sources")[1].GetProperty("layers").EnumerateArray()
                .Select(layer => layer.GetProperty("kind").GetString()));
        Assert.Contains(commandResult.GetProperty("links").EnumerateArray(), link =>
            link.GetProperty("target").GetProperty("kind").GetString() == "external"
            && link.GetProperty("target").GetProperty("network").GetString() == "network-not-attempted");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Published Context reports fragment failure through exact JSON status and stream"), Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextFragmentFailureIsIncompleteAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        working.ReplaceText(
            ".agents/projects/guide.md",
            "---\nopen-forge:\n  description: Guide\n  tags: [Guide]\n---\n\n"
            + "# Guide\n\n[Missing fragment](linked.md#absent).\n");

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "projects/guide", "--additions-only", "--content=metadata", "--follow-links=all", "--json"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Contains(commandResult.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "context.fragment-missing");
        var link = Assert.Single(commandResult.GetProperty("links").EnumerateArray());
        Assert.Equal("fragment-missing", link.GetProperty("target").GetProperty("resolution").GetString());
        Assert.Equal("unresolved", link.GetProperty("disposition").GetString());
    }

    private static void AssertOrdered(string value, params string[] expected)
    {
        var previous = -1;
        foreach (var item in expected)
        {
            var next = value.IndexOf(item, previous + 1, StringComparison.Ordinal);
            Assert.True(
                next > previous,
                string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Expected '{item}' after offset {previous}."));
            previous = next;
        }
    }
}
