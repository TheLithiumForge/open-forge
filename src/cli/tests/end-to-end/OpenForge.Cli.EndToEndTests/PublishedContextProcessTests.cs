using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedContextProcessTests
{
    [Fact(DisplayName = "Published Context help exposes direct grammar, projections, links, examples, and streams")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextHelpExposesCompleteContract()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "--help"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge context", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("[source-reference...]", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--additions-only", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--content=", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--follow-links=", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Examples", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("deterministic, stateless, and read-only", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published root help registers Context exactly once as a direct leaf")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRootHelpRegistersContextOnce()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["--help"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Single(
            result.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("context", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Published Context default startup closure is ordered human output on stdout")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
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

    [Theory(DisplayName = "Published Context distinguishes valid and malformed routed native Skill metadata")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PublishedContextUsesNativeSkillMetadata(bool malformed)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        working.AddRoutedSkills(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["experience-design"] = malformed
                ? "---\nname: experience-design\n---\n# Skill\n"
                : OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Skill(
                    "experience-design",
                    "Design user experiences."),
        });

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "--workspace", working.Path, "--content=metadata", "--view=compact"]);

        Assert.Equal(malformed ? 3 : 0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.StartsWith(malformed ? "context incomplete" : "context complete", result.StandardOutput, StringComparison.Ordinal);
        if (malformed)
        {
            Assert.Contains(
                $"context.closure-unavailable subject={PublishedContextWorkspace.ExperienceDesignSkillPath}",
                result.StandardOutput,
                StringComparison.Ordinal);
        }
        else
        {
            Assert.DoesNotContain(PublishedContextWorkspace.ExperienceDesignSkillPath, result.StandardOutput, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Published Context compact findings distinguish multiple malformed Skill paths")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextCompactFindingsDistinguishMalformedSkillPaths()
    {
        const string malformedSkill = "---\nname: incomplete\n---\n# Skill\n";
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        working.AddRoutedSkills(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["accessibility"] = malformedSkill,
            ["experience-design"] = malformedSkill,
        });

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "--workspace", working.Path, "--content=metadata", "--view=compact"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(PublishedContextWorkspace.AccessibilitySkillPath, result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(PublishedContextWorkspace.ExperienceDesignSkillPath, result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Context JSON retains additions, layers, links, and outside Markdown in canonical order")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
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

    [Theory(DisplayName = "Published Context honors bounded breadth-first traversal at depth two and higher"), Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    [InlineData(2, false)]
    [InlineData(3, true)]
    public async Task PublishedContextHonorsHigherBoundedDepth(int depth, bool expectsDeeper)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        working.ReplaceText(
            ".agents/projects/linked.md",
            "---\nopen-forge:\n  description: Linked\n  tags: [Guide]\n---\n\n"
            + "# Linked\n\n## Details\n\n[Deep](deep.md).\n");
        working.WriteText(
            ".agents/projects/deep.md",
            "---\nopen-forge:\n  description: Deep\n  tags: [Guide]\n---\n\n# Deep\n\n[Deeper](deeper.md).\n");
        working.WriteText(
            ".agents/projects/deeper.md",
            "---\nopen-forge:\n  description: Deeper\n  tags: [Guide]\n---\n\n# Deeper\n");

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            [
                "context", "projects/guide", "--additions-only", "--content=metadata",
                string.Create(System.Globalization.CultureInfo.InvariantCulture, $"--follow-links={depth}"), "--json",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var sources = document.RootElement.GetProperty("result").GetProperty("sources");
        Assert.Contains(sources.EnumerateArray(), source => source.GetProperty("path").GetString() == ".agents/projects/deep.md");
        Assert.Equal(
            expectsDeeper,
            sources.EnumerateArray().Any(source => source.GetProperty("path").GetString() == ".agents/projects/deeper.md"));
        Assert.All(
            document.RootElement.GetProperty("result").GetProperty("links").EnumerateArray(),
            link => Assert.InRange(link.GetProperty("depth").GetInt32(), 1, depth));
    }

    [Theory(DisplayName = "Published Context semantic cases retain typed status, exit, stream, finding, and next action")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    [InlineData("attention", 2, "context.section-missing", false)]
    [InlineData("incomplete", 3, "context.target-missing", true)]
    [InlineData("invalid", 4, "context.invalid-input", true)]
    [InlineData("blocked", 5, "context.workspace-unavailable", true)]
    public async Task PublishedContextSemanticJourneysRemainDistinct(
        string scenario,
        int expectedExit,
        string expectedFinding,
        bool expectsNext)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        string[] arguments = scenario switch
        {
            "attention" => [
                "context", "projects/guide", "--additions-only", "--content=section:Absent", "--json",
            ],
            "incomplete" => [
                "context", "projects/broken", "--additions-only", "--content=metadata", "--follow-links=1", "--json",
            ],
            "invalid" => ["context", "--additions-only", "--json"],
            "blocked" => ["context", "--workspace", working.Combine("missing-workspace"), "--json"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Context process case is not defined."),
        };
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(scenario, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        Assert.Equal(
            expectsNext ? JsonValueKind.Object : JsonValueKind.Null,
            document.RootElement.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Published Context JSON is repeatable and view-neutral beyond exact presentation evidence")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextJsonIsRepeatableAndViewNeutral()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "projects/guide", "--additions-only", "--content=headings,body", "--json", "--view=compact"]);
        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "projects/guide", "--additions-only", "--content=headings,body", "--json", "--view=compact"]);
        var expanded = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "projects/guide", "--additions-only", "--content=headings,body", "--json", "--view=expanded"]);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(compact.ExitCode, repeated.ExitCode);
        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(string.Empty, compact.StandardError);
        Assert.Equal(compact.StandardError, repeated.StandardError);
        Assert.Equal(compact.StandardError, expanded.StandardError);
        Assert.Equal(compact.StandardOutput, repeated.StandardOutput);
        using var compactDocument = JsonDocument.Parse(compact.StandardOutput);
        using var expandedDocument = JsonDocument.Parse(expanded.StandardOutput);
        Assert.Equal("compact", compactDocument.RootElement.GetProperty("result").GetProperty("presentation").GetProperty("view").GetProperty("supplied").GetString());
        Assert.Equal("expanded", expandedDocument.RootElement.GetProperty("result").GetProperty("presentation").GetProperty("view").GetProperty("supplied").GetString());
        Assert.Equal(
            compactDocument.RootElement.GetProperty("result").GetProperty("sources").GetRawText(),
            expandedDocument.RootElement.GetProperty("result").GetProperty("sources").GetRawText());
    }

    [Fact(DisplayName = "Published Context verbose mode preserves primary JSON and isolates bounded diagnostics")]
    [Trait("Feature", "context"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedContextVerboseStreamsRemainSeparate()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedContextWorkspace.Create();
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "projects/guide", "--additions-only", "--content=metadata", "--json"]);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["context", "projects/guide", "--additions-only", "--content=metadata", "--json", "--verbose"]);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
        Assert.EndsWith(Environment.NewLine, verbose.StandardError, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(verbose.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
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
