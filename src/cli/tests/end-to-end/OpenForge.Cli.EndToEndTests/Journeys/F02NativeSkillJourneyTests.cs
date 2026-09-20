using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F02NativeSkillJourneyTests
{
    private const string ReadmePath = "README.md";
    private const string SkillsListPath = ".agents/skills/_skills.md";
    private const string GuidanceListPath = ".agents/guidance/_guidance.md";
    private const string NativeSkillPath = ".agents/skills/team-notes/SKILL.md";
    private const string NativeReferencePath = ".agents/skills/team-notes/references/format.md";
    private const string NativeAssetPath = ".agents/skills/team-notes/assets/example.txt";
    private const string BadNativeSkillPath = ".agents/skills/bad-native/SKILL.md";
    private const string ValidGuidancePath = ".agents/guidance/review-note.md";

    private static readonly IReadOnlyList<string> ExpectedStartupSourcePaths =
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
    ];

    private const string NativeSkillText = """
        ---
        name: team-notes
        description: Organize a team's notes into clear decisions and follow-up work.
        ---

        # Team notes

        Read the selected project notes. Preserve the difference between decisions, proposals and unfinished work.

        Use [the note format](references/format.md) when an output structure is useful. The example in `assets/example.txt` is optional reference material.
        """;

    private const string NativeReferenceText = """
        # Note format

        Use a short subject, the decision or question, and the next action when there is one.

        This package resource intentionally has no frontmatter. It is not an independently routed Open Forge source.
        """;

    private const string NativeAssetText = """
        Subject: Scenario review
        Decision: Compare the actual file changes before approving the output.
        Next action: Run the same scenario on the corrected build.
        """;

    private const string MalformedNativeSkillText = """
        ---
        name: bad-native
        description: This native Skill deliberately has an unclosed header.
        """;

    private const string ValidGuidanceText = """
        ---
        open-forge:
          description: A valid guidance note added beside malformed native input.
          tags: [Guidance]
        ---

        # Review note

        Keep independently complete guidance work visible.
        """;

    [Fact(DisplayName = "F02 pastes a native Skill, indexes only its Skills navigation, and reads it unchanged"),
     Trait("Feature", "native-skill-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F02")]
    public async Task NativeSkillIsIndexedDiscoveredInspectedAndRead()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f02-main");
        await InstallW1Async(workspace);
        workspace.WriteBytes(NativeSkillPath, Encoding.UTF8.GetBytes(NativeSkillText));
        workspace.WriteBytes(NativeReferencePath, Encoding.UTF8.GetBytes(NativeReferenceText));
        workspace.WriteBytes(NativeAssetPath, Encoding.UTF8.GetBytes(NativeAssetText));

        var beforeIndex = SnapshotWorkspaceEntries(workspace.Path);
        var beforeExternal = SnapshotExternalStore(workspace);
        var checkedLists = CountEntriesSections(workspace.Path);
        var index = await workspace.RunAsync("index");
        Assert.Equal(0, index.ExitCode);
        Assert.NotEqual(string.Empty, index.StandardOutput);
        Assert.Equal(string.Empty, index.StandardError);

        var afterIndex = SnapshotWorkspaceEntries(workspace.Path);
        PublishedJourneyAssertions.AssertOnlyFileMutations(beforeIndex, afterIndex, SkillsListPath);
        AssertOrdinaryFileEntry(afterIndex, SkillsListPath);
        var changedLists = ChangedOrdinaryFilePaths(beforeIndex, afterIndex);
        Assert.Equal([SkillsListPath], changedLists);
        Assert.Contains(
            $"Updated the Entries section in {changedLists.Count} of {checkedLists} files.",
            index.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("team-notes", File.ReadAllText(workspace.Combine(SkillsListPath)), StringComparison.Ordinal);
        Assert.Contains(
            "Organize a team's notes into clear decisions and follow-up work.",
            File.ReadAllText(workspace.Combine(SkillsListPath)),
            StringComparison.Ordinal);
        Assert.DoesNotContain("references/format.md", File.ReadAllText(workspace.Combine(SkillsListPath)), StringComparison.Ordinal);
        Assert.DoesNotContain("assets/example.txt", File.ReadAllText(workspace.Combine(SkillsListPath)), StringComparison.Ordinal);
        AssertExternalMutationBoundary(workspace, beforeExternal);
        AssertNativePackageBytes(workspace);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        var listedBefore = workspace.SnapshotState();
        var listedExternalBefore = SnapshotExternalStore(workspace);
        var listed = await workspace.RunAsync("route", "list", "skills", "--depth=all");
        AssertSuccessful(listed);
        Assert.Contains("team-notes", listed.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "Organize a team's notes into clear decisions and follow-up work.",
            listed.StandardOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain(NativeReferencePath, listed.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(NativeAssetPath, listed.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(listedBefore, workspace.SnapshotState());
        Assert.Equal(listedExternalBefore, SnapshotExternalStore(workspace));

        var listedJsonBefore = workspace.SnapshotState();
        var listedJsonExternalBefore = SnapshotExternalStore(workspace);
        var listedJson = await workspace.RunAsync(
            "route",
            "list",
            "skills",
            "--depth=all",
            "--detail=full",
            "--format=json");
        AssertSuccessfulJson(listedJson, "route list");
        using (var listedDocument = JsonDocument.Parse(listedJson.StandardOutput))
        {
            var rows = listedDocument.RootElement
                .GetProperty("data")
                .GetProperty("rows")
                .EnumerateArray()
                .ToArray();
            var skill = Assert.Single(rows, row => row.GetProperty("path").GetString() == NativeSkillPath);
            Assert.Contains("team-notes", skill.GetProperty("id").GetString(), StringComparison.Ordinal);
            Assert.Equal(
                "Organize a team's notes into clear decisions and follow-up work.",
                skill.GetProperty("description").GetString());
            Assert.DoesNotContain(rows, row => row.GetProperty("path").GetString() == NativeReferencePath);
            Assert.DoesNotContain(rows, row => row.GetProperty("path").GetString() == NativeAssetPath);
        }
        Assert.Equal(listedJsonBefore, workspace.SnapshotState());
        Assert.Equal(listedJsonExternalBefore, SnapshotExternalStore(workspace));

        var inspectBefore = workspace.SnapshotState();
        var inspectExternalBefore = SnapshotExternalStore(workspace);
        var inspected = await workspace.RunAsync("route", "inspect", NativeSkillPath);
        AssertSuccessful(inspected);
        Assert.Contains(NativeSkillPath, inspected.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("team-notes", inspected.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(inspectBefore, workspace.SnapshotState());
        Assert.Equal(inspectExternalBefore, SnapshotExternalStore(workspace));

        var inspectedJsonBefore = workspace.SnapshotState();
        var inspectedJsonExternalBefore = SnapshotExternalStore(workspace);
        var inspectedJson = await workspace.RunAsync(
            "route",
            "inspect",
            NativeSkillPath,
            "--detail=full",
            "--format=json");
        AssertSuccessfulJson(inspectedJson, "route inspect");
        using (var inspectedDocument = JsonDocument.Parse(inspectedJson.StandardOutput))
        {
            var data = inspectedDocument.RootElement.GetProperty("data");
            Assert.Equal(NativeSkillPath, data.GetProperty("path").GetString());
            Assert.Equal("native", data.GetProperty("kind").GetString());
            Assert.Contains("team-notes", data.GetProperty("id").GetString(), StringComparison.Ordinal);
            var layer = Assert.Single(
                data.GetProperty("layers").EnumerateArray(),
                item => item.GetProperty("path").GetString() == NativeSkillPath);
            Assert.Equal("base", layer.GetProperty("kind").GetString());
        }
        Assert.Equal(inspectedJsonBefore, workspace.SnapshotState());
        Assert.Equal(inspectedJsonExternalBefore, SnapshotExternalStore(workspace));

        var contextBefore = workspace.SnapshotState();
        var contextExternalBefore = SnapshotExternalStore(workspace);
        var context = await workspace.RunAsync("context", NativeSkillPath);
        AssertSuccessful(context);
        Assert.Contains("# Team notes", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "Read the selected project notes. Preserve the difference between decisions, proposals and unfinished work.",
            context.StandardOutput,
            StringComparison.Ordinal);
        Assert.Equal(contextBefore, workspace.SnapshotState());
        Assert.Equal(contextExternalBefore, SnapshotExternalStore(workspace));

        var contextJsonBefore = workspace.SnapshotState();
        var contextJsonExternalBefore = SnapshotExternalStore(workspace);
        var contextJson = await workspace.RunAsync(
            "context",
            NativeSkillPath,
            "--content=body",
            "--detail=full",
            "--format=json");
        AssertSuccessfulJson(contextJson, "context");
        using (var contextDocument = JsonDocument.Parse(contextJson.StandardOutput))
        {
            var sources = contextDocument.RootElement
                .GetProperty("data")
                .GetProperty("sources")
                .EnumerateArray()
                .ToArray();
            Assert.Equal(
                ExpectedStartupSourcePaths.Append(NativeSkillPath).Order(StringComparer.Ordinal),
                sources.Select(source => source.GetProperty("path").GetString()).Order(StringComparer.Ordinal));

            foreach (var source in sources)
            {
                var path = source.GetProperty("path").GetString();
                Assert.NotNull(path);
                var parts = source.GetProperty("parts").EnumerateArray().ToArray();
                var body = Assert.Single(parts);
                Assert.Equal("body", body.GetProperty("part").GetString());
                if (string.Equals(path, NativeSkillPath, StringComparison.Ordinal))
                {
                    Assert.Equal(ExtractBody(NativeSkillText), body.GetProperty("text").GetString());
                }
                Assert.Equal(
                    ReadPhysicalBody(workspace.Combine(path!)),
                    body.GetProperty("text").GetString());
            }
        }
        Assert.Equal(contextJsonBefore, workspace.SnapshotState());
        Assert.Equal(contextJsonExternalBefore, SnapshotExternalStore(workspace));

        var repeatBefore = workspace.SnapshotState();
        var repeatExternalBefore = SnapshotExternalStore(workspace);
        var repeat = await workspace.RunAsync("index");
        AssertSuccessful(repeat);
        Assert.Contains("Nothing to do.", repeat.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(repeatBefore, workspace.SnapshotState());
        Assert.Equal(repeatExternalBefore, SnapshotExternalStore(workspace));
        AssertNativePackageBytes(workspace);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F02 updates independently complete guidance while reporting malformed native Skill input as incomplete"),
     Trait("Feature", "native-skill-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F02")]
    public async Task MalformedNativeSkillDoesNotHideIndependentGuidanceProgress()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f02-malformed-native");
        await InstallW1Async(workspace);
        workspace.WriteText(ValidGuidancePath, ValidGuidanceText);
        workspace.WriteText(BadNativeSkillPath, MalformedNativeSkillText);
        var malformedBytes = Encoding.UTF8.GetBytes(MalformedNativeSkillText);
        var guidanceBytes = Encoding.UTF8.GetBytes(ValidGuidanceText);
        var before = SnapshotWorkspaceEntries(workspace.Path);
        var externalBefore = SnapshotExternalStore(workspace);

        var result = await workspace.RunAsync("index");
        Assert.Equal(3, result.ExitCode);
        Assert.NotEqual(string.Empty, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(BadNativeSkillPath, result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("frontmatter", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("skipped", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Entries sections are current in all", result.StandardOutput, StringComparison.Ordinal);

        var after = SnapshotWorkspaceEntries(workspace.Path);
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            before,
            after,
            SkillsListPath,
            GuidanceListPath);
        AssertOrdinaryFileEntry(after, SkillsListPath);
        AssertOrdinaryFileEntry(after, GuidanceListPath);
        Assert.Equal([GuidanceListPath], ChangedOrdinaryFilePaths(before, after));
        AssertExternalMutationBoundary(workspace, externalBefore);
        Assert.Equal(malformedBytes, File.ReadAllBytes(workspace.Combine(BadNativeSkillPath)));
        Assert.Equal(guidanceBytes, File.ReadAllBytes(workspace.Combine(ValidGuidancePath)));
        Assert.Contains(
            "review-note.md",
            File.ReadAllText(workspace.Combine(GuidanceListPath)),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "bad-native",
            File.ReadAllText(workspace.Combine(SkillsListPath)),
            StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static async Task InstallW1Async(PublishedJourneyWorkspace workspace)
    {
        workspace.WriteText(ReadmePath, "# F02 workspace\n\nKeep this authored file unchanged.\n");
        workspace.ExpectCoreInstall();
        var install = await workspace.RunAsync("install", "--automatic");
        AssertSuccessful(install);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static void AssertSuccessful(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEqual(string.Empty, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertSuccessfulJson(ProcessRunResult result, string command)
    {
        AssertSuccessful(result);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
    }

    private static void AssertNativePackageBytes(PublishedJourneyWorkspace workspace)
    {
        Assert.Equal(
            Encoding.UTF8.GetBytes(NativeSkillText),
            File.ReadAllBytes(workspace.Combine(NativeSkillPath)));
        Assert.Equal(
            Encoding.UTF8.GetBytes(NativeReferenceText),
            File.ReadAllBytes(workspace.Combine(NativeReferencePath)));
        Assert.Equal(
            Encoding.UTF8.GetBytes(NativeAssetText),
            File.ReadAllBytes(workspace.Combine(NativeAssetPath)));
        Assert.Equal(
            Encoding.UTF8.GetBytes("# F02 workspace\n\nKeep this authored file unchanged.\n"),
            File.ReadAllBytes(workspace.Combine(ReadmePath)));
    }

    private static void AssertExternalMutationBoundary(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> before)
    {
        var after = SnapshotExternalStore(workspace);
        var expectedRecoveryDirectories = ExpectedRecoveryDirectories(workspace);
        foreach (var path in before.Keys.Union(after.Keys, StringComparer.Ordinal))
        {
            var hasBefore = before.ContainsKey(path);
            var hasAfter = after.ContainsKey(path);
            if (!hasBefore)
            {
                Assert.True(hasAfter, $"Expected recovery directory was not observed after mutation: {path}");
                Assert.Contains(path, expectedRecoveryDirectories);
                Assert.StartsWith("type=directory;", after[path], StringComparison.Ordinal);
                continue;
            }

            Assert.True(hasAfter, $"Unexpected removed external path: {path}");
            if (expectedRecoveryDirectories.Contains(path))
            {
                Assert.StartsWith("type=directory;", before[path], StringComparison.Ordinal);
                Assert.StartsWith("type=directory;", after[path], StringComparison.Ordinal);
                Assert.True(PublishedJourneyAssertions.DirectoryMetadataMatchesAfterChildMutation(before[path], after[path]));
            }
            else
            {
                Assert.Equal(before[path], after[path]);
            }
        }

        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static IReadOnlySet<string> ExpectedRecoveryDirectories(
        PublishedJourneyWorkspace workspace)
    {
        var recoveryDirectory = Path.GetRelativePath(
            workspace.LockStore.LocalApplicationDataDirectory,
            workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path)).Replace('\\', '/');
        var expected = new HashSet<string>(StringComparer.Ordinal) { "." };
        var current = recoveryDirectory;
        while (current != ".")
        {
            expected.Add(current);
            current = Path.GetDirectoryName(current)?.Replace('\\', '/') ?? ".";
        }

        return expected;
    }

    private static IReadOnlyDictionary<string, string> SnapshotExternalStore(
        PublishedJourneyWorkspace workspace)
    {
        var root = workspace.LockStore.LocalApplicationDataDirectory;
        if (!Directory.Exists(root))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return PublishedWorkspaceTreeSnapshot.Capture(root);
    }

    private static IReadOnlyDictionary<string, string> SnapshotWorkspaceEntries(string rootPath)
        => PublishedWorkspaceTreeSnapshot.Capture(rootPath);

    private static IReadOnlyList<string> ChangedOrdinaryFilePaths(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
        => before.Keys
            .Union(after.Keys, StringComparer.Ordinal)
            .Where(path => after.TryGetValue(path, out var afterValue)
                && afterValue.StartsWith("type=file;", StringComparison.Ordinal)
                && (!before.TryGetValue(path, out var beforeValue)
                    || !string.Equals(beforeValue, afterValue, StringComparison.Ordinal)))
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static void AssertOrdinaryFileEntry(
        IReadOnlyDictionary<string, string> snapshot,
        string path)
    {
        Assert.True(snapshot.TryGetValue(path, out var description));
        Assert.StartsWith("type=file;", description, StringComparison.Ordinal);
    }

    private static int CountEntriesSections(string rootPath)
        => Directory
            .EnumerateFiles(
                System.IO.Path.Combine(rootPath, ".agents"),
                "*.md",
                SearchOption.AllDirectories)
            .Count(path => File.ReadLines(path).Any(line => line == "## Entries"));

    private static string ExtractBody(string source)
    {
        var separator = source.StartsWith("---\n", StringComparison.Ordinal)
            ? source.IndexOf("\n---\n", 4, StringComparison.Ordinal)
            : -1;
        return separator >= 0
            ? source[(separator + "\n---\n".Length)..]
            : source;
    }

    private static string ReadPhysicalBody(string path)
        => ExtractBody(Encoding.UTF8.GetString(File.ReadAllBytes(path)));
}
