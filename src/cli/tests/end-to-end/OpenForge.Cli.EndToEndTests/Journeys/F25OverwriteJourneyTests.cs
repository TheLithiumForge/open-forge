using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F25OverwriteJourneyTests
{
    private const string GuidanceEntrypointPath = ".agents/guidance/_guidance.md";
    private const string TeamNotePath = ".agents/guidance/team-note.md";
    private const string TeamNoteOverwritePath = ".agents/guidance/team-note.overwrite.md";

    private const string BaseTeamNoteBody =
        "\n# Team Note\n\nF25 base bytes remain the maintainer guidance.\n";

    private const string OverwriteTeamNoteBody =
        "\n# Team Note Local Adjustment\n\nF25 local adjustment remains a separate layer.\n";

    private static readonly string[] ExpectedContextPaths =
    [
        "AGENTS.md",
        ".agents/loader.md",
        ".agents/directives/_directives.md",
        ".agents/guidance/_guidance.md",
        ".agents/maps/_maps.md",
        ".agents/memory/_memory.md",
        ".agents/memory/crystallized/_crystallized.md",
        ".agents/memory/emerging/_emerging.md",
        ".agents/memory/working/_working.md",
        ".agents/patterns/_patterns.md",
        ".agents/skills/_skills.md",
        TeamNotePath,
        TeamNoteOverwritePath,
    ];

    [Fact(DisplayName = "F25 customizes one guidance source through an overwrite and converges index")]
    [Trait("Feature", "f25-overwrite-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F25")]
    public async Task CustomizeThroughOverwritePreservesLayersAndAuthoredBytes()
    {
        using var workspace = PublishedJourneyWorkspace.Create("e2e-f25-overwrite-journey");
        workspace.ExpectCoreInstall();

        var install = await workspace.RunAsync("install", "--automatic");
        AssertSuccessfulHumanResult(install);

        var baseDocument = OpenForgeDocumentSeed.Metadata(
            description: "F25 base team note",
            tags: ["Guidance", "TeamNote"],
            body: BaseTeamNoteBody);
        var overwriteDocument = OpenForgeDocumentSeed.Metadata(
            description: "F25 local team note adjustment",
            tags: ["Guidance", "TeamNote", "Overwrite"],
            body: OverwriteTeamNoteBody);
        workspace.WriteText(TeamNotePath, baseDocument);
        workspace.WriteText(TeamNoteOverwritePath, overwriteDocument);

        var baseBytes = File.ReadAllBytes(workspace.Combine(TeamNotePath));
        var overwriteBytes = File.ReadAllBytes(workspace.Combine(TeamNoteOverwritePath));

        var beforeFirstIndex = workspace.SnapshotState();
        var guidanceBeforeIndex = File.ReadAllText(workspace.Combine(GuidanceEntrypointPath));
        var index = await workspace.RunAsync("index");
        AssertSuccessfulHumanResult(index);
        var afterFirstIndex = workspace.SnapshotState();
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            beforeFirstIndex,
            afterFirstIndex,
            GuidanceEntrypointPath);
        Assert.NotEqual(
            beforeFirstIndex[GuidanceEntrypointPath],
            afterFirstIndex[GuidanceEntrypointPath]);
        var guidanceAfterIndex = File.ReadAllText(workspace.Combine(GuidanceEntrypointPath));
        var beforeEntries = guidanceBeforeIndex.IndexOf("## Entries", StringComparison.Ordinal);
        var afterEntries = guidanceAfterIndex.IndexOf("## Entries", StringComparison.Ordinal);
        Assert.True(beforeEntries >= 0 && afterEntries >= 0);
        Assert.Equal(guidanceBeforeIndex[..beforeEntries], guidanceAfterIndex[..afterEntries]);
        Assert.Equal(baseBytes, File.ReadAllBytes(workspace.Combine(TeamNotePath)));
        Assert.Equal(overwriteBytes, File.ReadAllBytes(workspace.Combine(TeamNoteOverwritePath)));
        AssertGeneratedGuidanceEntries(workspace);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        var beforeRoute = workspace.SnapshotState();
        var route = await workspace.RunAsync("route", "list", "guidance", "--depth=all");
        AssertSuccessfulHumanResult(route);
        Assert.Contains("team-note", route.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("team-note.overwrite", route.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(beforeRoute, workspace.SnapshotState());

        var routeFacts = await workspace.RunAsync(
            "route", "list", "guidance", "--depth=all", "--format=json", "--detail=full");
        AssertSuccessfulJsonResult(routeFacts, "route list");
        AssertRouteRows(routeFacts.StandardOutput);
        Assert.Equal(beforeRoute, workspace.SnapshotState());

        var beforeContext = workspace.SnapshotState();
        var context = await workspace.RunAsync("context", "guidance/team-note");
        AssertSuccessfulHumanResult(context);
        Assert.Contains(BaseTeamNoteBody.Trim(), context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(OverwriteTeamNoteBody.Trim(), context.StandardOutput, StringComparison.Ordinal);
        AssertOrdered(
            context.StandardOutput,
            BaseTeamNoteBody.Trim(),
            OverwriteTeamNoteBody.Trim());
        Assert.Equal(beforeContext, workspace.SnapshotState());

        var contextFacts = await workspace.RunAsync(
            "context",
            "guidance/team-note",
            "--content=body",
            "--detail=full",
            "--format=json");
        AssertSuccessfulJsonResult(contextFacts, "context");
        AssertContextLayers(contextFacts.StandardOutput, workspace, baseDocument, overwriteDocument);
        Assert.Equal(beforeContext, workspace.SnapshotState());

        var beforeRepeatedIndex = workspace.SnapshotState();
        var repeatedIndex = await workspace.RunAsync("index");
        AssertSuccessfulHumanResult(repeatedIndex);
        Assert.Equal(beforeRepeatedIndex, workspace.SnapshotState());
        Assert.Equal(baseBytes, File.ReadAllBytes(workspace.Combine(TeamNotePath)));
        Assert.Equal(overwriteBytes, File.ReadAllBytes(workspace.Combine(TeamNoteOverwritePath)));
        AssertGeneratedGuidanceEntries(workspace);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static void AssertSuccessfulHumanResult(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertSuccessfulJsonResult(ProcessRunResult result, string command)
    {
        AssertSuccessfulHumanResult(result);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
    }

    private static void AssertGeneratedGuidanceEntries(PublishedJourneyWorkspace workspace)
    {
        var document = File.ReadAllText(workspace.Combine(GuidanceEntrypointPath));
        Assert.DoesNotContain("team-note.overwrite.md", document, StringComparison.Ordinal);
        Assert.Equal(
            ["team-note.md"],
            ReadEntryDestinations(workspace, GuidanceEntrypointPath));
    }

    private static void AssertRouteRows(string json)
    {
        using var document = JsonDocument.Parse(json);
        var rows = document.RootElement
            .GetProperty("data")
            .GetProperty("rows")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, rows.Length);
        Assert.Equal(
            ["guidance", "guidance/team-note"],
            rows.Select(row => row.GetProperty("id").GetString()).ToArray());
        Assert.Equal(
            [GuidanceEntrypointPath, TeamNotePath],
            rows.Select(row => row.GetProperty("path").GetString()).ToArray());
        Assert.Equal(
            [0, 1],
            rows.Select(row => row.GetProperty("relativeDepth").GetInt32()).ToArray());
        Assert.False(rows[0].GetProperty("hasOverwrite").GetBoolean());
        Assert.True(rows[1].GetProperty("hasOverwrite").GetBoolean());
    }

    private static void AssertContextLayers(
        string json,
        PublishedJourneyWorkspace workspace,
        string baseDocument,
        string overwriteDocument)
    {
        using var document = JsonDocument.Parse(json);
        var sources = document.RootElement
            .GetProperty("data")
            .GetProperty("sources")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(
            ExpectedContextPaths.Order(StringComparer.Ordinal),
            sources.Select(source => source.GetProperty("path").GetString()).Order(StringComparer.Ordinal));
        foreach (var source in sources)
        {
            var parts = source.GetProperty("parts").EnumerateArray().ToArray();
            var part = Assert.Single(parts);
            Assert.Equal("body", part.GetProperty("part").GetString());
        }

        var selectedLayers = sources[^2..];
        Assert.Equal(
            [TeamNotePath, TeamNoteOverwritePath],
            selectedLayers.Select(source => source.GetProperty("path").GetString()).ToArray());
        Assert.Equal(
            ["base", "overwrite"],
            selectedLayers.Select(source => source.GetProperty("layer").GetString()).ToArray());
        Assert.Equal(
            ["guidance/team-note", "guidance/team-note"],
            selectedLayers.Select(source => source.GetProperty("id").GetString()).ToArray());

        Assert.Equal(BaseTeamNoteBody, BodyPart(selectedLayers[0]));
        Assert.Equal(OverwriteTeamNoteBody, BodyPart(selectedLayers[1]));
        Assert.Equal(
            baseDocument,
            File.ReadAllText(workspace.Combine(selectedLayers[0].GetProperty("path").GetString()!)));
        Assert.Equal(
            overwriteDocument,
            File.ReadAllText(workspace.Combine(selectedLayers[1].GetProperty("path").GetString()!)));
    }

    private static string BodyPart(JsonElement source)
    {
        var part = Assert.Single(source.GetProperty("parts").EnumerateArray());
        Assert.Equal("body", part.GetProperty("part").GetString());
        return part.GetProperty("text").GetString()!;
    }

    private static string[] ReadEntryDestinations(
        PublishedJourneyWorkspace workspace,
        string relativePath)
    {
        var lines = File.ReadAllText(workspace.Combine(relativePath)).Replace("\r\n", "\n")
            .Split('\n');
        var headings = lines
            .Select((line, index) => (line, index))
            .Where(value => value.line.Trim() == "## Entries")
            .Select(value => value.index)
            .ToArray();
        Assert.Single(headings);

        var destinations = new List<string>();
        for (var index = headings[0] + 1; index < lines.Length; index++)
        {
            var line = lines[index];
            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                break;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("- none -", StringComparison.Ordinal))
            {
                continue;
            }

            if (!trimmed.StartsWith("- [", StringComparison.Ordinal))
            {
                Assert.False(
                    trimmed.StartsWith("- ", StringComparison.Ordinal),
                    $"Unexpected generated Entries row: {line}");
                continue;
            }

            var linkStart = line.IndexOf("](", StringComparison.Ordinal);
            Assert.True(linkStart >= 0, $"Generated Entries line has no link: {line}");
            var linkEnd = line.IndexOf(')', linkStart + 2);
            Assert.True(linkEnd > linkStart + 2, $"Generated Entries link is incomplete: {line}");
            destinations.Add(line[(linkStart + 2)..linkEnd]);
        }

        Assert.Equal(destinations.Count, destinations.Distinct(StringComparer.Ordinal).Count());
        return destinations.Order(StringComparer.Ordinal).ToArray();
    }

    private static void AssertOrdered(string value, params string[] expected)
    {
        var previous = -1;
        foreach (var item in expected)
        {
            var next = value.IndexOf(item, previous + 1, StringComparison.Ordinal);
            Assert.True(next > previous, $"Expected '{item}' after offset {previous}.");
            previous = next;
        }
    }
}
