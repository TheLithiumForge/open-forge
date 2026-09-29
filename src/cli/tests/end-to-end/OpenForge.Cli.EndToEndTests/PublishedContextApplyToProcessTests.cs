using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedContextApplyToProcessTests
{
    [Fact(DisplayName = "Published Context reports pending applyTo conditions for paths content at every detail"),
     Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PendingApplicabilityIsVisibleInEveryJsonDetail()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        working.ReplaceText(
            ".agents/startup/_startup.md",
            ConditionedDocument(
                "Startup",
                ["LoadNow", "Core"],
                ["src/**"],
                "# Startup\n\n## Entries\n\n- [Topic](topic.md) - #KeepInMind #Core\n"));

        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
                target,
                working.Path,
                working.SnapshotState,
                ["context", "--content=paths", $"--detail={detail}", "--format=json"]);

            Assert.Equal(3, result.ExitCode);
            Assert.True(string.IsNullOrWhiteSpace(result.StandardError), result.StandardError);
            using var document = JsonDocument.Parse(result.StandardOutput);
            var root = document.RootElement;
            Assert.Equal("incomplete", root.GetProperty("status").GetString());
            Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() == "context.applicability-pending");
            var pending = Assert.Single(root.GetProperty("data").GetProperty("pendingConditions").EnumerateArray());
            Assert.Equal(".agents/startup/_startup.md", pending.GetProperty("source").GetString());
            Assert.Equal(["src/**"], pending.GetProperty("patterns").EnumerateArray().Select(value => value.GetString()));
            Assert.Contains(
                "applyTo conditions await one or more --for paths",
                result.StandardOutput,
                StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Published Context matches inherited applyTo conditions using repeated concrete paths"),
     Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task ConcretePathsSelectMatchingInheritedSources()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        working.ReplaceText(
            ".agents/startup/_startup.md",
            ConditionedDocument(
                "Startup",
                ["LoadNow", "Core"],
                ["src/**"],
                "# Startup\n\n## Entries\n\n- [Topic](topic.md) - #KeepInMind #Core\n"));
        working.ReplaceText(
            ".agents/startup/topic.md",
            ConditionedDocument("Topic", ["KeepInMind", "Core"], ["src/*.cs"], "# Topic\n\nStartup topic.\n"));

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "context",
                "--for", "src/./app.cs",
                "--for=src/../src/app.cs",
                "--content=metadata",
                "--detail=full",
                "--format=json",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(["src/app.cs"], data.GetProperty("workingPaths").EnumerateArray().Select(value => value.GetString()));
        var topic = Assert.Single(
            data.GetProperty("sources").EnumerateArray(),
            source => source.GetProperty("path").GetString() == ".agents/startup/topic.md");
        var applicability = topic.GetProperty("applicability");
        Assert.Equal("matched", applicability.GetProperty("state").GetString());
        Assert.Equal("src/app.cs", Assert.Single(applicability.GetProperty("matchingPaths").EnumerateArray()).GetString());
        Assert.Equal(
            [".agents/startup/_startup.md", ".agents/startup/topic.md"],
            applicability.GetProperty("conditions").EnumerateArray()
                .Select(condition => condition.GetProperty("source").GetString()));
    }

    private static string ConditionedDocument(
        string description,
        IReadOnlyList<string> tags,
        IReadOnlyList<string> patterns,
        string body)
    {
        var applyTo = string.Join("\n", patterns.Select(pattern => $"  - \"{pattern}\""));
        var tagList = string.Join(", ", tags);
        return $"---\napplyTo:\n{applyTo}\nopen-forge:\n  description: {description}\n  tags: [{tagList}]\n---\n\n{body}";
    }
}
