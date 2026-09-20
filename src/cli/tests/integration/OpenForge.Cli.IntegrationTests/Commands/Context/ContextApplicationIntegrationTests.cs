using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context JSON repeats exactly and isolates view evidence and verbose diagnostics"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task JsonRepeatViewAndDiagnosticsPreservePrimaryFacts()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        string[] arguments = ["context", "projects/guide", "--additions-only", "--content=headings,body", "--format", "json"];
        var compact = await CliHostCapture.RunAsync([.. arguments, "--detail=minimal"], workspace.Path);
        var repeat = await CliHostCapture.RunAsync([.. arguments, "--detail=minimal"], workspace.Path);
        var expanded = await CliHostCapture.RunAsync([.. arguments, "--detail=standard"], workspace.Path);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--detail", "debug"], workspace.Path);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(compact.ExitCode, repeat.ExitCode);
        Assert.Equal(compact.ExitCode, expanded.ExitCode);
        Assert.Equal(compact.ExitCode, verbose.ExitCode);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(string.Empty, repeat.Error);
        Assert.Equal(string.Empty, expanded.Error);
        Assert.Equal(compact.Output, repeat.Output);
        Assert.Empty(verbose.Error);
        using var compactDocument = JsonDocument.Parse(compact.Output);
        using var expandedDocument = JsonDocument.Parse(expanded.Output);
        using var verboseDocument = JsonDocument.Parse(verbose.Output);
        var compactRoot = compactDocument.RootElement;
        var expandedRoot = expandedDocument.RootElement;
        var verboseRoot = verboseDocument.RootElement;
        Assert.Equal("minimal", compactRoot.GetProperty("detail").GetString());
        Assert.Equal("standard", expandedRoot.GetProperty("detail").GetString());
        Assert.Equal("debug", verboseRoot.GetProperty("detail").GetString());
        AssertNativeDataPreserved(compactRoot, expandedRoot);
        AssertNativeDataPreserved(compactRoot, verboseRoot);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context unavailable workspace preserves blocked JSON and next action without writes"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnavailableWorkspaceIsTypedBlocked()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["context", "--workspace", workspace.Combine("missing"), "--format", "json"], workspace.Path);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("next").ValueKind);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "context.workspace-unavailable");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed public root help exposes one direct Context leaf"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task RootHelpExposesOneContextLeaf()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["--help"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Single(
            result.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("context", StringComparison.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
        var leaf = await CliHostCapture.RunAsync(["context", "--help"], workspace.Path);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, leaf.Error);
        Assert.Contains("open-forge context", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("[source-reference...]", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("--additions-only", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("--content ", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("--follow-links ", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("Examples", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("Context preserves authored source bytes", leaf.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context composed application resolves startup order without workspace writes"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MetadataStartupClosureUsesRealWorkspaceWithoutWrites()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["context", "--content=metadata", "--detail", "standard"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("4 sources, about", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Status: complete", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Path:", result.Output, StringComparison.Ordinal);
        AssertOrdered(
            result.Output,
            "=== AGENTS.md ===",
            "=== .agents/loader.md (loader) ===",
            "=== .agents/startup/_startup.md (startup) ===",
            "=== .agents/startup/topic.md (startup/topic) ===");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context JSON returns route additions, overwrites, links, and outside Markdown in canonical order"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task JsonAdditionsRetainCompleteExpandedGraph()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "context",
                "projects/guide",
                "--additions-only",
                "--content=metadata",
                "--follow-links=all",
                "--format", "json",
                "--detail", "full",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
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
        var guide = commandResult.GetProperty("sources")[1];
        Assert.Equal("base", guide.GetProperty("layer").GetString());
        var overwrite = commandResult.GetProperty("sources")[2];
        Assert.Equal("overwrite", overwrite.GetProperty("layer").GetString());
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("sources")[4].GetProperty("id").ValueKind);
        Assert.Contains(commandResult.GetProperty("links").EnumerateArray(), link =>
            link.GetProperty("destination").GetString() == "https://example.invalid/context"
            && link.GetProperty("resolution").GetString() == "external-unchecked"
            && !link.GetProperty("followed").GetBoolean());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context composed application resolves multiple explicit routes with one ordered ancestor per chain"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MultipleExplicitRoutesRetainOperandAndClosureOrder()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "context", "projects/guide", "areas/nested/leaf", "projects/linked",
                "--additions-only", "--content=metadata", "--format", "json",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var sources = document.RootElement.GetProperty("data").GetProperty("sources");
        Assert.Equal(
            [
                ".agents/projects/_projects.md",
                ".agents/projects/guide.md",
                ".agents/projects/guide.overwrite.md",
                ".agents/areas/_areas.md",
                ".agents/areas/nested/_nested.md",
                ".agents/areas/nested/leaf.md",
                ".agents/areas/nested/automatic.md",
                ".agents/projects/linked.md",
            ],
            sources.EnumerateArray().Select(source => source.GetProperty("path").GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Context composed application exposes malformed and unreadable selected continuity metadata as incomplete"), Trait("Feature", "context"), Trait("Evidence", "Integration"),
     InlineData(false),
     InlineData(true)]
    public static async Task SelectedContinuityMetadataFailureCannotFalseComplete(bool invalidEncoding)
    {
        using var workspace = CreateWorkspace();
        var checkpoint = workspace.Combine(".agents/continuity/checkpoint.md");
        if (invalidEncoding)
        {
            File.WriteAllBytes(checkpoint, [0xFF, 0xFE, 0xFD]);
        }
        else
        {
            File.WriteAllText(
                checkpoint,
                "---\nopen-forge: [unterminated\n---\n# Checkpoint\n");
        }
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["context", "continuity", "--content=metadata", "--format", "json"],
            workspace.Path);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("counts").GetProperty("sources").ValueKind);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "context.closure-unavailable"
            && finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/continuity/checkpoint.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context composed application ignores unavailable metadata outside routed continuity"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnroutedMetadataFailureDoesNotInvalidateContinuityCoverage()
    {
        using var workspace = CreateWorkspace();
        workspace.WriteText(
            ".agents/unrouted.md",
            "---\nopen-forge: [unterminated\n---\n# Unrouted\n");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["context", "--content=metadata", "--format", "json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("data");
        Assert.DoesNotContain(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("subject").GetProperty("path").GetString() == ".agents/unrouted.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Context public statuses preserve JSON stdout, semantic exits, next actions, and no writes"),
     Trait("Feature", "context"), Trait("Evidence", "Integration"),
     InlineData("completed-with-warnings", 2, "context.section-missing"),
     InlineData("incomplete", 3, "context.target-missing"),
     InlineData("invalid-input", 4, "context.invalid-input")]
    public static async Task JsonSemanticJourneysRemainDistinct(
        string scenario,
        int expectedExit,
        string expectedFinding)
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        string[] arguments = scenario switch
        {
            "completed-with-warnings" => [
                "context", "projects/guide", "--additions-only", "--content=section:Absent", "--format", "json",
            ],
            "incomplete" => [
                "context", "projects/broken", "--additions-only", "--content=metadata", "--follow-links=1", "--format", "json",
            ],
            "invalid-input" => ["context", "--additions-only", "--format", "json"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Context scenario is not defined."),
        };

        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(scenario, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        Assert.Equal(
            scenario == "incomplete" ? JsonValueKind.Object : JsonValueKind.Null,
            document.RootElement.GetProperty("next").ValueKind);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context blocks a physical link-target escape on human stderr without following or writing"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task PhysicalTargetEscapeIsBlockedOnHumanError()
    {
        using var workspace = CreateWorkspace();
        using var outside = TemporaryWorkspace.Create("context-outside");
        outside.WriteText("outside.md", "# Outside\n");
        if (!workspace.TryCreateFileSymbolicLink(
                ".agents/projects/escape.md",
                outside.Combine("outside.md"),
                out _))
        {
            return;
        }

        var workspaceBefore = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(
            [
                "context", "projects/unsafe", "--additions-only", "--content=metadata", "--follow-links=1",
            ],
            workspace.Path);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.Contains("points outside the workspace. It was not followed.", result.Error, StringComparison.Ordinal);
        Assert.Equal(workspaceBefore, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("context-application");
        workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.WriteText(
            ".agents/loader.md",
            "# Loader\n\n## Entries\n\n"
            + "- [Startup](startup/_startup.md) - #LoadNow #Core\n"
            + "- [Projects](projects/_projects.md) - #Project\n"
            + "- [Areas](areas/_areas.md) - #Domain\n"
            + "- [Continuity](continuity/_continuity.md) - #Memory\n"
            );
        workspace.WriteText(
            ".agents/startup/_startup.md",
            Document(
                "Startup",
                "LoadNow, Core",
                "# Startup\n\n## Entries\n\n"
                    + "- [Topic](topic.md) - #LoadNow #Core\n"
                + ""));
        workspace.WriteText(
            ".agents/startup/topic.md",
            Document("Topic", "LoadNow, Core", "# Topic\n\nStartup topic.\n"));
        workspace.WriteText(
            ".agents/projects/_projects.md",
            Document(
                "Projects",
                "Project",
                "# Projects\n\n## Entries\n\n"
                    + "- [Guide](guide.md) - #Guide\n"
                + "- [Linked](linked.md) - #Guide\n"
                + "- [Broken](broken.md) - #Guide\n"
                + "- [Unsafe](unsafe.md) - #Guide\n"
                + ""));
        workspace.WriteText(
            ".agents/projects/guide.md",
            Document(
                "Guide",
                "Guide",
                "# Guide\n\n## Rules\n\nBase rule.\n\n"
                + "[Linked](linked.md#details) [Outside](../../README.md) [External](https://example.invalid/context).\n"));
        workspace.WriteText(
            ".agents/projects/guide.overwrite.md",
            Document("Guide overwrite", "Guide", "# Guide overwrite\n\n## Rules\n\nOverride.\n"));
        workspace.WriteText(
            ".agents/projects/linked.md",
            Document("Linked", "Guide", "# Linked\n\n## Details\n\n[Back](guide.md).\n"));
        workspace.WriteText(
            ".agents/projects/broken.md",
            Document("Broken", "Guide", "# Broken\n\n[Missing](missing.md).\n"));
        workspace.WriteText(
            ".agents/projects/unsafe.md",
            Document("Unsafe", "Guide", "# Unsafe\n\n[Escape](escape.md).\n"));
        workspace.WriteText(
            ".agents/areas/_areas.md",
            Document(
                "Areas",
                "Domain",
                "# Areas\n\n## Entries\n\n"
                    + "- [Nested](nested/_nested.md) - #Domain\n"
                + ""));
        workspace.WriteText(
            ".agents/areas/nested/_nested.md",
            Document(
                "Nested",
                "Domain",
                "# Nested\n\n## Entries\n\n"
                    + "- [Leaf](leaf.md) - #Guide\n"
                + "- [Automatic](automatic.md) - #LoadNow #Guide\n"
                + ""));
        workspace.WriteText(
            ".agents/areas/nested/leaf.md",
            Document("Nested leaf", "Guide", "# Nested leaf\n"));
        workspace.WriteText(
            ".agents/areas/nested/automatic.md",
            Document("Nested automatic", "LoadNow, Guide", "# Nested automatic\n"));
        workspace.WriteText(
            ".agents/continuity/_continuity.md",
            Document(
                "Continuity",
                "Memory",
                "# Continuity\n\n## Entries\n\n"
                    + "- [Checkpoint](checkpoint.md) - #KeepInMind #Memory\n"
                + ""));
        workspace.WriteText(
            ".agents/continuity/checkpoint.md",
            Document("Checkpoint", "KeepInMind, Memory", "# Checkpoint\n"));
        workspace.WriteText("README.md", "# Readme\n\nContained outside agents.\n");
        return workspace;
    }

    private static string Document(string description, string tags, string body)
        => OpenForgeDocumentSeed.Metadata(
            description: description,
            tags: tags.Split(", ", StringSplitOptions.None),
            body: $"\n{body}");

    private static void AssertNativeDataPreserved(JsonElement expectedRoot, JsonElement actualRoot)
    {
        foreach (var property in new[] { "command", "status", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "recovery", "next" })
        {
            Assert.Equal(
                expectedRoot.GetProperty(property).GetRawText(),
                actualRoot.GetProperty(property).GetRawText());
        }

        var expectedSources = expectedRoot.GetProperty("data").GetProperty("sources").EnumerateArray().ToArray();
        var actualSources = actualRoot.GetProperty("data").GetProperty("sources").EnumerateArray().ToArray();
        Assert.Equal(expectedSources.Length, actualSources.Length);
        for (var index = 0; index < expectedSources.Length; index++)
        {
            Assert.Equal(expectedSources[index].GetProperty("path").GetString(), actualSources[index].GetProperty("path").GetString());
            Assert.Equal(expectedSources[index].GetProperty("id").GetRawText(), actualSources[index].GetProperty("id").GetRawText());
            Assert.Equal(expectedSources[index].GetProperty("layer").GetString(), actualSources[index].GetProperty("layer").GetString());
            AssertNativePartsPreserved(expectedSources[index], actualSources[index]);
        }
    }

    private static void AssertNativePartsPreserved(JsonElement expectedSource, JsonElement actualSource)
    {
        var expectedParts = expectedSource.GetProperty("parts").EnumerateArray().ToArray();
        var actualParts = actualSource.GetProperty("parts").EnumerateArray().ToArray();
        Assert.Equal(expectedParts.Length, actualParts.Length);
        for (var index = 0; index < expectedParts.Length; index++)
        {
            Assert.Equal(expectedParts[index].GetProperty("part").GetString(), actualParts[index].GetProperty("part").GetString());
            if (expectedParts[index].TryGetProperty("text", out var expectedText))
            {
                Assert.Equal(expectedText.GetString(), actualParts[index].GetProperty("text").GetString());
            }
            else if (expectedParts[index].TryGetProperty("state", out var expectedState))
            {
                Assert.Equal(expectedState.GetString(), actualParts[index].GetProperty("state").GetString());
            }
            else if (expectedParts[index].TryGetProperty("paths", out var expectedPaths))
            {
                Assert.Equal(expectedPaths.GetRawText(), actualParts[index].GetProperty("paths").GetRawText());
            }
            else if (expectedParts[index].TryGetProperty("headings", out var expectedHeadings))
            {
                var expectedShape = expectedHeadings.EnumerateArray().Select(heading =>
                    (heading.GetProperty("text").GetString(), heading.GetProperty("level").GetInt32()));
                var actualShape = actualParts[index].GetProperty("headings").EnumerateArray().Select(heading =>
                    (heading.GetProperty("text").GetString(), heading.GetProperty("level").GetInt32()));
                Assert.Equal(expectedShape, actualShape);
            }
        }
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
