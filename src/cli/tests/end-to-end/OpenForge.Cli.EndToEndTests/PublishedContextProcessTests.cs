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
            ["context", "--content=metadata", "--detail=standard"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("4 sources, about", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/projects/", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("tags: KeepInMind, Core", result.StandardOutput, StringComparison.Ordinal);
        AssertOrdered(
            result.StandardOutput,
            "=== AGENTS.md ===",
            "=== .agents/loader.md (loader) ===",
            "=== .agents/startup/_startup.md (startup) ===",
            "=== .agents/startup/topic.md (startup/topic) ===");
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
                "--detail=full",
                "--format=json",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("context", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.Equal(
            [
                ".agents/projects/_projects.md",
                ".agents/projects/guide.md",
                ".agents/projects/guide.overwrite.md",
                ".agents/projects/linked.md",
                "README.md",
            ],
            commandResult.GetProperty("sources").EnumerateArray()
                .Select(source => source.GetProperty("path").GetString()));
        var links = commandResult.GetProperty("links").EnumerateArray().ToArray();
        Assert.Contains(links, link =>
            link.GetProperty("from").GetString() == ".agents/projects/guide.md"
            && link.GetProperty("destination").GetString() == "linked.md#details"
            && link.GetProperty("resolution").GetString() == "complete"
            && link.GetProperty("followed").GetBoolean());
        Assert.Contains(links, link =>
            link.GetProperty("destination").GetString() == "https://example.invalid/context"
            && link.GetProperty("resolution").GetString() == "external-unchecked"
            && !link.GetProperty("followed").GetBoolean());
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
            ["context", "projects/guide", "--additions-only", "--content=metadata", "--follow-links=all", "--detail=full", "--format=json"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "context.fragment-missing");
        var link = Assert.Single(commandResult.GetProperty("links").EnumerateArray());
        Assert.Equal(".agents/projects/guide.md", link.GetProperty("from").GetString());
        Assert.Equal("linked.md#absent", link.GetProperty("destination").GetString());
        Assert.Equal(".agents/projects/linked.md", link.GetProperty("resolvedPath").GetString());
        Assert.Equal("fragment-missing", link.GetProperty("resolution").GetString());
        Assert.False(link.GetProperty("followed").GetBoolean());
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
