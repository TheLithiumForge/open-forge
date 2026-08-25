using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedFindProcessRedTests
{
    [Fact(DisplayName = "Published Find help exposes direct-leaf grammar, sections, exits, examples, and related commands"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindHelpExposesCompleteContract()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var missing = working.Combine("missing-help-workspace");
        AssertPathAbsent(missing);
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--help", "--workspace", missing]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge find", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--include", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--content", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Related commands", result.StandardOutput, StringComparison.Ordinal);
        AssertPathAbsent(missing);
    }

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
        Assert.Contains("Matches: 3", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("docs", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("guide", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("result=complete", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Find explicit --workspace uses the default expanded view"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindExplicitWorkspaceUsesDefaultExpandedView()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("Selected by: --workspace", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Coverage: complete", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Matches: 3", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("result=complete", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Find compact filtering returns the exact matched source identity"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindCompactFilteringReturnsMatch()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--view=compact", "--tag=Architecture", "--heading=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Equal(
            [
                "result=complete\tcoverage=complete\tuniverse=default\tmatches=1",
                "docs\t.agents/docs.md",
            ],
            NonEmptyLines(result.StandardOutput));
    }

    [Fact(DisplayName = "Published Find JSON content projection is complete and JSON view is a no-op"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindJsonContentAndViewAreStable()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--tag=Architecture", "--json", "--view=compact", "--content=metadata,frontmatter,headings,body,section:Target"]);
        var expanded = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--tag=Architecture", "--json", "--view=expanded", "--content=metadata,frontmatter,headings,body,section:Target"]);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(0, expanded.ExitCode);
        Assert.Equal(string.Empty, compact.StandardError);
        Assert.Equal(string.Empty, expanded.StandardError);
        using var compactDocument = JsonDocument.Parse(compact.StandardOutput);
        using var expandedDocument = JsonDocument.Parse(expanded.StandardOutput);
        AssertJsonViewChangesOnlyEchoedSelection(compactDocument, expandedDocument);
        AssertCompleteDocsProjection(compactDocument);
        AssertCompleteDocsProjection(expandedDocument);
    }

    [Fact(DisplayName = "Published Find typed invalid content has the invalid exit and JSON projection presence distinction"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindInvalidContentIsTyped()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--json", "--content=unknown"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("not-started", document.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("projection").GetString());
        Assert.Equal([], document.RootElement.GetProperty("result").GetProperty("presentation").GetProperty("content").GetProperty("effective").EnumerateArray().Select(value => value.GetString()));
    }

    [Fact(DisplayName = "Published Find blocked workspace emits stderr only for human output and null workspace in JSON"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindBlockedWorkspaceUsesBlockedPolicy()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var missing = working.Combine("missing-workspace");
        AssertPathAbsent(missing);
        var human = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", missing, "--view=compact"]);

        Assert.Equal(5, human.ExitCode);
        Assert.Equal(string.Empty, human.StandardOutput);
        Assert.Contains("result=blocked", human.StandardError, StringComparison.Ordinal);
        Assert.Contains("find.workspace-unavailable", human.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge doctor", human.StandardError, StringComparison.Ordinal);
        AssertPathAbsent(missing);

        var json = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", missing, "--json"]);

        Assert.Equal(5, json.ExitCode);
        Assert.Equal(string.Empty, json.StandardError);
        using var document = JsonDocument.Parse(json.StandardOutput);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        AssertPathAbsent(missing);
    }

    [Theory(DisplayName = "Published Find deterministic attention and incomplete journeys retain typed status, findings, and next actions"),
        InlineData("attention", "attention", 2, "find.identity-collision"),
        InlineData("unreadable", "incomplete", 3, "find.inspection-unavailable"),
        InlineData("invalid-encoding", "incomplete", 3, "find.invalid-encoding"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindDeterministicFindingsRemainTyped(
        string scenario,
        string status,
        int exitCode,
        string findingCode)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = scenario == "attention"
            ? PublishedFindWorkspace.CreateAttention()
            : PublishedFindWorkspace.CreateIncomplete(scenario == "invalid-encoding");
        var content = scenario == "attention" ? "body" : "metadata,body";
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--json", $"--content={content}"]);

        Assert.Equal(exitCode, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(status, document.RootElement.GetProperty("status").GetString());
        var findings = document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray().ToArray();
        Assert.Contains(findings, finding => finding.GetProperty("code").GetString() == findingCode);
        Assert.Equal(
            status == "attention" ? JsonValueKind.Null : JsonValueKind.Object,
            document.RootElement.GetProperty("next").ValueKind);
    }

    [Fact(DisplayName = "Published Find verbose mode preserves the primary result and writes only bounded diagnostics to stderr"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindVerboseStreamsRemainSeparate()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--view=expanded"]);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--view=expanded", "--verbose"]);

        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
    }

    [Fact(DisplayName = "Published Find invalid JSON keeps primary output, status, and exit invariant under verbose diagnostics"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindInvalidJsonVerboseModePreservesPrimaryDocument()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        string[] arguments = ["find", "--workspace", working.Path, "--json", "--content=unknown"];
        var plain = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            arguments.Append("--verbose").ToArray());

        Assert.Equal(4, plain.ExitCode);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(string.Empty, plain.StandardError);
        Assert.Equal(plain.StandardOutput, verbose.StandardOutput);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);

        using var plainDocument = JsonDocument.Parse(plain.StandardOutput);
        using var verboseDocument = JsonDocument.Parse(verbose.StandardOutput);
        Assert.Equal("invalid", plainDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("status").GetString(),
            verboseDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("projection").GetString(),
            verboseDocument.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("projection").GetString());
    }

    [Fact(DisplayName = "Published Find representative public journeys leave all workspace bytes unchanged"), Trait("Feature", "find-presentation"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedFindRepresentativeJourneysDoNotWrite()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedFindWorkspace.CreateBare();
        var json = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--json"]);
        var compact = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotState,
            ["find", "--workspace", working.Path, "--view=compact", "--tag=Architecture"]);

        Assert.Equal(0, json.ExitCode);
        Assert.Equal(string.Empty, json.StandardError);
        using (var document = JsonDocument.Parse(json.StandardOutput))
        {
            Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        }

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(string.Empty, compact.StandardError);
        Assert.Equal(
            [
                "result=complete\tcoverage=complete\tuniverse=default\tmatches=1",
                "docs\t.agents/docs.md",
            ],
            NonEmptyLines(compact.StandardOutput));
    }

    private static string[] NonEmptyLines(string output)
        => output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

    private static void AssertCompleteDocsProjection(JsonDocument document)
    {
        var root = document.RootElement;
        Assert.Equal("find", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(3, root.GetProperty("result").GetProperty("universe").GetProperty("candidateCount").GetInt32());
        Assert.Equal(3, root.GetProperty("result").GetProperty("universe").GetProperty("inspectedCount").GetInt32());
        Assert.Equal(1, root.GetProperty("result").GetProperty("universe").GetProperty("matchedCount").GetInt32());

        var presentation = root.GetProperty("result").GetProperty("presentation");
        string[] expectedContent = ["metadata", "frontmatter", "headings", "body", "section:Target"];
        Assert.Equal(expectedContent, presentation.GetProperty("content").GetProperty("supplied").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(expectedContent, presentation.GetProperty("content").GetProperty("effective").EnumerateArray().Select(value => value.GetString()));

        var match = Assert.Single(root.GetProperty("result").GetProperty("matches").EnumerateArray());
        Assert.Equal("docs", match.GetProperty("id").GetString());
        Assert.Equal(".agents/docs.md", match.GetProperty("path").GetString());
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
            match.GetProperty("projections").EnumerateArray().Select(projection => projection.GetProperty("part").GetString()));
        Assert.Equal(
            ["available", "available", "available", "available", "available", "available", "available", "available", "available"],
            match.GetProperty("projections").EnumerateArray().Select(projection => projection.GetProperty("state").GetString()));

        var projections = match.GetProperty("projections").EnumerateArray().ToArray();
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
            projections.Select(projection => projection.GetProperty("part").GetString()));
        Assert.Equal(
            new string?[] { null, "base", "base", "base", "base", "overwrite", "overwrite", "overwrite", "overwrite" },
            projections.Select(projection => projection.GetProperty("layer").GetString()));
        Assert.Equal(
            new string?[] { null, ".agents/docs.md", ".agents/docs.md", ".agents/docs.md", ".agents/docs.md", ".agents/docs.overwrite.md", ".agents/docs.overwrite.md", ".agents/docs.overwrite.md", ".agents/docs.overwrite.md" },
            projections.Select(projection => projection.GetProperty("path").GetString()));

        var metadata = projections[0].GetProperty("metadata");
        Assert.Equal("docs", metadata.GetProperty("id").GetString());
        Assert.Equal(".agents/docs.md", metadata.GetProperty("path").GetString());
        Assert.Equal("unrouted", metadata.GetProperty("routeState").GetString());
        Assert.Null(metadata.GetProperty("route").GetString());
        var metadataLayers = metadata.GetProperty("layers").EnumerateArray().ToArray();
        Assert.Equal(["base", "overwrite"], metadataLayers.Select(layer => layer.GetProperty("kind").GetString()));
        Assert.Equal(
            [".agents/docs.md", ".agents/docs.overwrite.md"],
            metadataLayers.Select(layer => layer.GetProperty("path").GetString()));

        AssertHeadingProjection(
            projections[2],
            ["Architecture", "Target"],
            [6, 8]);
        AssertHeadingProjection(
            projections[6],
            ["Architecture", "Target"],
            [5, 7]);

        AssertTextProjection(
            projections[1],
            ".agents/docs.md",
            "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n---",
            1);
        AssertTextProjection(
            projections[3],
            ".agents/docs.md",
            "# Architecture\n\n## Target\n\nTarget body\n",
            6);
        AssertTextProjection(
            projections[4],
            ".agents/docs.md",
            "## Target\n\nTarget body\n",
            8);
        AssertTextProjection(
            projections[5],
            ".agents/docs.overwrite.md",
            "---\nopen-forge:\n  tags: [Architecture]\n---",
            1);
        AssertTextProjection(
            projections[7],
            ".agents/docs.overwrite.md",
            "# Architecture\n\n## Target\n\nOverwrite body\n",
            5);
        AssertTextProjection(
            projections[8],
            ".agents/docs.overwrite.md",
            "## Target\n\nOverwrite body\n",
            7);
    }

    private static void AssertJsonViewChangesOnlyEchoedSelection(
        JsonDocument compactDocument,
        JsonDocument expandedDocument)
    {
        var compactRoot = compactDocument.RootElement;
        var expandedRoot = expandedDocument.RootElement;
        var compactView = compactRoot.GetProperty("result").GetProperty("presentation").GetProperty("view");
        var expandedView = expandedRoot.GetProperty("result").GetProperty("presentation").GetProperty("view");
        Assert.Equal("compact", compactView.GetProperty("supplied").GetString());
        Assert.Equal("compact", compactView.GetProperty("effective").GetString());
        Assert.Equal("expanded", expandedView.GetProperty("supplied").GetString());
        Assert.Equal("expanded", expandedView.GetProperty("effective").GetString());
        Assert.Equal(compactRoot.GetProperty("schemaVersion").GetRawText(), expandedRoot.GetProperty("schemaVersion").GetRawText());
        Assert.Equal(compactRoot.GetProperty("command").GetRawText(), expandedRoot.GetProperty("command").GetRawText());
        Assert.Equal(compactRoot.GetProperty("status").GetRawText(), expandedRoot.GetProperty("status").GetRawText());
        Assert.Equal(compactRoot.GetProperty("workspace").GetRawText(), expandedRoot.GetProperty("workspace").GetRawText());
        Assert.Equal(compactRoot.GetProperty("next").GetRawText(), expandedRoot.GetProperty("next").GetRawText());

        var compactResult = compactRoot.GetProperty("result");
        var expandedResult = expandedRoot.GetProperty("result");
        foreach (var property in new[] { "universe", "query", "coverage", "findings", "matches" })
        {
            Assert.Equal(compactResult.GetProperty(property).GetRawText(), expandedResult.GetProperty(property).GetRawText());
        }

        Assert.Equal(
            compactResult.GetProperty("presentation").GetProperty("content").GetRawText(),
            expandedResult.GetProperty("presentation").GetProperty("content").GetRawText());
    }

    private static void AssertHeadingProjection(
        JsonElement projection,
        string[] expectedTexts,
        int[] expectedLines)
    {
        Assert.Equal("headings", projection.GetProperty("part").GetString());
        var headings = projection.GetProperty("headings").EnumerateArray().ToArray();
        Assert.Equal(expectedTexts, headings.Select(heading => heading.GetProperty("text").GetString()));
        Assert.Equal(expectedLines, headings.Select(heading => heading.GetProperty("location").GetProperty("line").GetInt32()));
        Assert.All(
            headings,
            heading => Assert.NotEqual(JsonValueKind.Null, heading.GetProperty("location").ValueKind));
    }

    private static void AssertTextProjection(
        JsonElement projection,
        string expectedPath,
        string expectedText,
        int expectedLine)
    {
        Assert.Equal("available", projection.GetProperty("state").GetString());
        Assert.Equal(expectedPath, projection.GetProperty("path").GetString());
        Assert.Equal(expectedText, projection.GetProperty("text").GetString());
        var location = projection.GetProperty("location");
        Assert.NotEqual(JsonValueKind.Null, location.ValueKind);
        Assert.Equal(expectedLine, location.GetProperty("line").GetInt32());
        Assert.Equal(1, location.GetProperty("column").GetInt32());
    }

    private static void AssertPathAbsent(string path)
    {
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(path));
    }
}
