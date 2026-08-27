using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextApplicationIntegrationTests
{
    [Fact(DisplayName = "Composed public root help exposes one direct Context leaf")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task RootHelpExposesOneContextLeaf()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(["--help"], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Single(
            result.Output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("context", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Context composed application resolves startup order without workspace writes")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MetadataStartupClosureUsesRealWorkspaceWithoutWrites()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["context", "--content=metadata"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("Result: complete", result.Output, StringComparison.Ordinal);
        AssertOrdered(
            result.Output,
            "Path: AGENTS.md",
            "Path: .agents/loader.md",
            "Path: .agents/startup/_startup.md",
            "Path: .agents/startup/topic.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Context JSON returns route additions, overwrites, links, and outside Markdown in canonical order")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
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
                "--json",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
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
        var guide = commandResult.GetProperty("sources")[1];
        Assert.Equal(["base", "overwrite"], guide.GetProperty("layers").EnumerateArray()
            .Select(layer => layer.GetProperty("kind").GetString()));
        Assert.Equal(JsonValueKind.Null, commandResult.GetProperty("sources")[3].GetProperty("id").ValueKind);
        Assert.Contains(commandResult.GetProperty("links").EnumerateArray(), link =>
            link.GetProperty("target").GetProperty("kind").GetString() == "external"
            && link.GetProperty("target").GetProperty("network").GetString() == "network-not-attempted");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Context composed application resolves multiple explicit routes with one ordered ancestor per chain"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MultipleExplicitRoutesRetainOperandAndClosureOrder()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "context", "projects/guide", "areas/nested/leaf", "projects/linked",
                "--additions-only", "--content=metadata", "--json",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var sources = document.RootElement.GetProperty("result").GetProperty("sources");
        Assert.Equal(
            [
                ".agents/projects/_projects.md",
                ".agents/projects/guide.md",
                ".agents/areas/_areas.md",
                ".agents/areas/nested/_nested.md",
                ".agents/areas/nested/leaf.md",
                ".agents/areas/nested/automatic.md",
                ".agents/projects/linked.md",
            ],
            sources.EnumerateArray().Select(source => source.GetProperty("path").GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Context composed application exposes malformed and unreadable global-continuity metadata as incomplete"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GlobalContinuityMetadataFailureCannotFalseComplete(bool invalidEncoding)
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
            ["context", "--content=metadata", "--json"],
            workspace.Path);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Equal("incomplete", commandResult.GetProperty("coverage").GetProperty("selection").GetString());
        Assert.Contains(commandResult.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "context.closure-unavailable"
            && finding.GetProperty("path").GetString() == ".agents/continuity/checkpoint.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Context composed application ignores unavailable metadata outside routed continuity"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnroutedMetadataFailureDoesNotInvalidateContinuityCoverage()
    {
        using var workspace = CreateWorkspace();
        workspace.WriteText(
            ".agents/unrouted.md",
            "---\nopen-forge: [unterminated\n---\n# Unrouted\n");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["context", "--content=metadata", "--json"],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var commandResult = root.GetProperty("result");
        Assert.Equal("complete", commandResult.GetProperty("coverage").GetProperty("selection").GetString());
        Assert.DoesNotContain(commandResult.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("path").GetString() == ".agents/unrouted.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Context public statuses preserve JSON stdout, semantic exits, next actions, and no writes")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData("attention", 2, "context.section-missing")]
    [InlineData("incomplete", 3, "context.target-missing")]
    [InlineData("invalid", 4, "context.invalid-input")]
    public async Task JsonSemanticJourneysRemainDistinct(
        string scenario,
        int expectedExit,
        string expectedFinding)
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        string[] arguments = scenario switch
        {
            "attention" => [
                "context", "projects/guide", "--additions-only", "--content=section:Absent", "--json",
            ],
            "incomplete" => [
                "context", "projects/broken", "--additions-only", "--content=metadata", "--follow-links=1", "--json",
            ],
            "invalid" => ["context", "--additions-only", "--json"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Context scenario is not defined."),
        };

        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(expectedExit, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(scenario, document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == expectedFinding);
        Assert.Equal(
            scenario == "attention" ? JsonValueKind.Null : JsonValueKind.Object,
            document.RootElement.GetProperty("next").ValueKind);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Context blocks a physical link-target escape on human stderr without following or writing")]
    [Trait("Feature", "context"), Trait("Evidence", "Integration")]
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
        Assert.Contains("context.target-unsafe", result.Error, StringComparison.Ordinal);
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
            + "<!-- open-forge:generated-index:start -->\n"
            + "- [Startup](startup/_startup.md) - #LoadNow #Core\n"
            + "- [Projects](projects/_projects.md) - #Project\n"
            + "- [Areas](areas/_areas.md) - #Domain\n"
            + "- [Continuity](continuity/_continuity.md) - #Memory\n"
            + "<!-- open-forge:generated-index:end -->\n");
        workspace.WriteText(
            ".agents/startup/_startup.md",
            Document(
                "Startup",
                "LoadNow, Core",
                "# Startup\n\n## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Topic](topic.md) - #LoadNow #Core\n"
                + "<!-- open-forge:generated-index:end -->\n"));
        workspace.WriteText(
            ".agents/startup/topic.md",
            Document("Topic", "LoadNow, Core", "# Topic\n\nStartup topic.\n"));
        workspace.WriteText(
            ".agents/projects/_projects.md",
            Document(
                "Projects",
                "Project",
                "# Projects\n\n## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Guide](guide.md) - #Guide\n"
                + "- [Linked](linked.md) - #Guide\n"
                + "- [Broken](broken.md) - #Guide\n"
                + "- [Unsafe](unsafe.md) - #Guide\n"
                + "<!-- open-forge:generated-index:end -->\n"));
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
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Nested](nested/_nested.md) - #Domain\n"
                + "<!-- open-forge:generated-index:end -->\n"));
        workspace.WriteText(
            ".agents/areas/nested/_nested.md",
            Document(
                "Nested",
                "Domain",
                "# Nested\n\n## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Leaf](leaf.md) - #Guide\n"
                + "- [Automatic](automatic.md) - #LoadNow #Guide\n"
                + "<!-- open-forge:generated-index:end -->\n"));
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
                + "<!-- open-forge:generated-index:start -->\n"
                + "- [Checkpoint](checkpoint.md) - #KeepInMind #Memory\n"
                + "<!-- open-forge:generated-index:end -->\n"));
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
