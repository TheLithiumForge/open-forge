using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F05NestedScopeJourneyTests
{
    private const string TargetId = "guidance/team/new-topic/notes";
    private const string RootEntrypointPath = ".agents/guidance/_guidance.md";
    private const string TeamDirectoryPath = ".agents/guidance/team";
    private const string TeamEntrypointPath = ".agents/guidance/team/_team.md";
    private const string NewTopicDirectoryPath = ".agents/guidance/team/new-topic";
    private const string NewTopicEntrypointPath = ".agents/guidance/team/new-topic/_new-topic.md";
    private const string LeafPath = ".agents/guidance/team/new-topic/notes.md";
    private const string PatternsEntrypointPath = ".agents/patterns/_patterns.md";
    private const string ReadmePath = "README.md";
    private const string ReadmeText = "# Outside\n\nThis authored README must survive the journey.\n";
    private const string AuthoredRootGuidance =
        "---\n"
        + "open-forge:\n"
        + "  description: Guidance\n"
        + "  tags: [Guidance]\n"
        + "---\n"
        + "# Guidance\n\n"
        + "This authored root guidance must survive route creation.\n\n"
        + "## Entries\n\n"
        + "- none - No entries - #Empty\n";

    private static readonly string[] CreatedRouteFiles =
    [
        TeamEntrypointPath,
        NewTopicEntrypointPath,
        LeafPath,
    ];

    [Fact(
        DisplayName = "F05 creates, refines, inspects, contextualizes, and repeats a nested scope")]
    [Trait("Feature", "nested-scope-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F05")]
    public async Task CreateRefineInspectContextAndRepeatNoOp()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f05-nested-scope");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles([ReadmePath, .. CreatedRouteFiles]);

        var installed = await workspace.RunAsync("install", "--automatic");
        AssertSuccessfulHuman(installed);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

        workspace.WriteText(ReadmePath, ReadmeText);
        Assert.True(File.Exists(workspace.Combine(RootEntrypointPath)));
        Assert.False(Directory.Exists(workspace.Combine(TeamDirectoryPath)));
        Assert.False(File.Exists(workspace.Combine(TeamEntrypointPath)));
        Assert.False(Directory.Exists(workspace.Combine(NewTopicDirectoryPath)));
        Assert.False(File.Exists(workspace.Combine(NewTopicEntrypointPath)));
        Assert.False(File.Exists(workspace.Combine(LeafPath)));

        workspace.WriteText(RootEntrypointPath, AuthoredRootGuidance);
        var authoredRootBytes = File.ReadAllBytes(workspace.Combine(RootEntrypointPath));
        Assert.Equal(Encoding.UTF8.GetBytes(AuthoredRootGuidance), authoredRootBytes);

        var beforeCreate = workspace.SnapshotState();
        var create = await workspace.RunAsync(
            "route",
            "create",
            TargetId,
            "--description",
            "Team topic notes",
            "--tag=Guidance");

        AssertSuccessfulHuman(create);
        AssertRouteTopology(workspace);
        AssertEntriesOutsideRegionPreserved(
            authoredRootBytes,
            File.ReadAllBytes(workspace.Combine(RootEntrypointPath)));
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            beforeCreate,
            workspace.SnapshotState(),
            AllowedCreateChanges.ToArray());

        var createdLeaf = File.ReadAllText(workspace.Combine(LeafPath), Encoding.UTF8);
        Assert.Contains("description: Team topic notes", createdLeaf, StringComparison.Ordinal);
        Assert.Contains("tags: [Guidance]", createdLeaf, StringComparison.Ordinal);
        Assert.DoesNotContain("tags: [Guidance, Team]", createdLeaf, StringComparison.Ordinal);
        AssertNoSuppliedIntermediateMetadata(workspace, TeamEntrypointPath);
        AssertNoSuppliedIntermediateMetadata(workspace, NewTopicEntrypointPath);
        Assert.Equal(ReadmeText, File.ReadAllText(workspace.Combine(ReadmePath), Encoding.UTF8));

        var createdLeafBody = ReadBody(createdLeaf);
        var update = await workspace.RunAsync(
            "route",
            "update",
            TargetId,
            "--tag=Guidance",
            "--tag=Team");

        AssertSuccessfulHuman(update);
        var updatedLeaf = File.ReadAllText(workspace.Combine(LeafPath), Encoding.UTF8);
        Assert.Contains("tags: [Guidance, Team]", updatedLeaf, StringComparison.Ordinal);
        Assert.DoesNotContain("tags: [Team, Guidance]", updatedLeaf, StringComparison.Ordinal);
        Assert.Equal(createdLeafBody, ReadBody(updatedLeaf));

        var inspect = await workspace.RunAsync("route", "inspect", TargetId);
        AssertSuccessfulHuman(inspect);
        Assert.Contains(
            "guidance/team/new-topic/notes",
            Normalize(inspect.StandardOutput),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "guidance/team",
            Normalize(inspect.StandardOutput),
            StringComparison.OrdinalIgnoreCase);

        var context = await workspace.RunAsync("context", TargetId);
        AssertSuccessfulHuman(context);
        Assert.Contains(
            "guidance/team/new-topic",
            Normalize(context.StandardOutput),
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "notes",
            Normalize(context.StandardOutput),
            StringComparison.OrdinalIgnoreCase);

        var beforeNoOp = SnapshotWithLock(workspace, workspace.Path);
        var noOp = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotWithLock(workspace, workspace.Path),
            [
                "route",
                "update",
                TargetId,
                "--tag=Guidance",
                "--tag=Team",
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.NotEmpty(noOp.StandardOutput);
        Assert.Equal(beforeNoOp, SnapshotWithLock(workspace, workspace.Path));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        PublishedJourneyAssertions.AssertOnlyFileMutations(
            beforeCreate,
            workspace.SnapshotState(),
            AllowedCreateChanges.ToArray());
        Assert.Equal(ReadmeText, File.ReadAllText(workspace.Combine(ReadmePath), Encoding.UTF8));
        Assert.Equal(
            beforeCreate[PatternsEntrypointPath],
            workspace.SnapshotState()[PatternsEntrypointPath]);
    }

    [Fact(
        DisplayName = "F05 labels explicit Route Init as a separate nested-scope recovery")]
    [Trait("Feature", "nested-scope-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F05")]
    public async Task ExplicitInitRecoveryCreatesTheSameNestedLeaf()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f05-init-recovery");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(CreatedRouteFiles);

        var installed = await workspace.RunAsync("install", "--automatic");
        AssertSuccessfulHuman(installed);

        var init = await workspace.RunAsync(
            "route",
            "init",
            "guidance/team/new-topic");
        AssertSuccessfulHuman(init);

        var create = await workspace.RunAsync(
            "route",
            "create",
            TargetId,
            "--description",
            "Team topic notes",
            "--tag=Guidance");
        AssertSuccessfulHuman(create);

        AssertRouteTopology(workspace);
        Assert.Contains(
            "description: Team topic notes",
            File.ReadAllText(workspace.Combine(LeafPath), Encoding.UTF8),
            StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(
        DisplayName = "F05 framework Route Init blocks without implicit installation")]
    [Trait("Feature", "nested-scope-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F05")]
    public async Task FrameworkInitWithoutInstallIsBlocked()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f05-framework-without-install");
        workspace.ExpectFiles(ReadmePath);
        workspace.WriteText(ReadmePath, ReadmeText);
        var before = workspace.SnapshotState();

        var result = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            workspace.SnapshotState,
            [
                "route",
                "init",
                "guidance/team",
                "--framework",
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains("install", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        workspace.LockStore.AssertNoInfrastructure();
    }

    [Fact(
        DisplayName = "F05 rejects an unknown root without inventing a route")]
    [Trait("Feature", "nested-scope-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F05")]
    public async Task UnknownRootCreateIsInvalidAndReadOnly()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f05-unknown-root");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(ReadmePath);

        var installed = await workspace.RunAsync("install", "--automatic");
        AssertSuccessfulHuman(installed);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
        workspace.WriteText(ReadmePath, ReadmeText);
        Assert.True(File.Exists(workspace.Combine(RootEntrypointPath)));
        Assert.True(Directory.Exists(workspace.Combine(".agents/guidance")));

        var before = SnapshotAllOwnedState(workspace);

        var result = await PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotAllOwnedState(workspace),
            [
                "route",
                "create",
                "nonsense/team/notes",
                "--description",
                "Team topic notes",
                "--tag=Guidance",
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Equal(before, SnapshotAllOwnedState(workspace));
        Assert.True(Directory.Exists(workspace.Combine(".agents/guidance")));
        Assert.False(Directory.Exists(workspace.Combine(".agents/nonsense")));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static IReadOnlySet<string> AllowedCreateChanges { get; } =
        new HashSet<string>(
        [
            RootEntrypointPath,
            TeamDirectoryPath,
            TeamEntrypointPath,
            NewTopicDirectoryPath,
            NewTopicEntrypointPath,
            LeafPath,
        ],
        StringComparer.Ordinal);

    private static void AssertSuccessfulHuman(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static void AssertRouteTopology(PublishedJourneyWorkspace workspace)
    {
        Assert.True(Directory.Exists(workspace.Combine(TeamDirectoryPath)));
        Assert.True(File.Exists(workspace.Combine(TeamEntrypointPath)));
        Assert.True(Directory.Exists(workspace.Combine(NewTopicDirectoryPath)));
        Assert.True(File.Exists(workspace.Combine(NewTopicEntrypointPath)));
        Assert.True(File.Exists(workspace.Combine(LeafPath)));

        var root = Normalize(File.ReadAllText(workspace.Combine(RootEntrypointPath), Encoding.UTF8));
        var team = Normalize(File.ReadAllText(workspace.Combine(TeamEntrypointPath), Encoding.UTF8));
        var newTopic = Normalize(File.ReadAllText(workspace.Combine(NewTopicEntrypointPath), Encoding.UTF8));

        Assert.Contains("team/_team.md", root, StringComparison.Ordinal);
        Assert.Contains("new-topic/_new-topic.md", team, StringComparison.Ordinal);
        Assert.Contains("notes.md", newTopic, StringComparison.Ordinal);
        AssertExactGeneratedEntries(
            root,
            [RelativeEntryPath(RootEntrypointPath, TeamEntrypointPath)]);
        AssertExactGeneratedEntries(
            team,
            [RelativeEntryPath(TeamEntrypointPath, NewTopicEntrypointPath)]);
        AssertExactGeneratedEntries(
            newTopic,
            [RelativeEntryPath(NewTopicEntrypointPath, LeafPath)]);
    }

    private static void AssertEntriesOutsideRegionPreserved(byte[] before, byte[] after)
    {
        var beforeRegion = SplitEntriesRegion(before);
        var afterRegion = SplitEntriesRegion(after);
        Assert.Equal(beforeRegion.Prefix, afterRegion.Prefix);
        Assert.Equal(beforeRegion.Suffix, afterRegion.Suffix);
    }

    private static ByteEntriesRegion SplitEntriesRegion(byte[] document)
    {
        var marker = Encoding.UTF8.GetBytes("## Entries");
        var markerStart = document.AsSpan().IndexOf(marker);
        Assert.True(markerStart >= 0, "The route document has no Entries heading.");
        var bodyStart = markerStart + marker.Length;
        var nextHeadingMarker = Encoding.UTF8.GetBytes("\n## ");
        var nextHeadingOffset = document.AsSpan(bodyStart).IndexOf(nextHeadingMarker);
        return nextHeadingOffset < 0
            ? new ByteEntriesRegion(
                document[..bodyStart],
                document[bodyStart..],
                Array.Empty<byte>())
            : new ByteEntriesRegion(
                document[..bodyStart],
                document[bodyStart..(bodyStart + nextHeadingOffset)],
                document[(bodyStart + nextHeadingOffset)..]);
    }

    private static void AssertExactGeneratedEntries(
        string document,
        IReadOnlyList<string> expectedPaths)
    {
        var region = SplitEntriesRegion(document);
        var lines = region.Body
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var actualPaths = new List<string>(lines.Length);
        foreach (var line in lines)
        {
            Assert.StartsWith("- [", line, StringComparison.Ordinal);
            var linkStart = line.IndexOf("](", StringComparison.Ordinal);
            Assert.True(linkStart >= 0, $"Generated Entries row has no Markdown link: {line}");
            var linkEnd = line.IndexOf(')', linkStart + 2);
            Assert.True(linkEnd > linkStart + 2, $"Generated Entries row has no link target: {line}");
            actualPaths.Add(line[(linkStart + 2)..linkEnd]);
        }

        Assert.Equal(expectedPaths, actualPaths);
    }

    private static string RelativeEntryPath(string parentEntrypointPath, string childPath)
    {
        var parentDirectory = Path.GetDirectoryName(parentEntrypointPath)
            ?? throw new InvalidOperationException("The authored route fixture parent has no directory.");
        return Path.GetRelativePath(parentDirectory, childPath).Replace('\\', '/');
    }

    private static EntriesRegion SplitEntriesRegion(string document)
    {
        var marker = "## Entries";
        var markerStart = document.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(markerStart >= 0, "The route document has no Entries heading.");
        var bodyStart = markerStart + marker.Length;
        var nextHeading = document.IndexOf("\n## ", bodyStart, StringComparison.Ordinal);
        return nextHeading < 0
            ? new EntriesRegion(document[..bodyStart], document[bodyStart..], string.Empty)
            : new EntriesRegion(
                document[..bodyStart],
                document[bodyStart..nextHeading],
                document[nextHeading..]);
    }

    private static void AssertNoSuppliedIntermediateMetadata(
        PublishedJourneyWorkspace workspace,
        string relativePath)
    {
        var frontMatter = ExtractFrontMatter(
            File.ReadAllText(workspace.Combine(relativePath), Encoding.UTF8));
        Assert.DoesNotContain("description:", frontMatter, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tags:", frontMatter, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, string> SnapshotWithLock(
        PublishedJourneyWorkspace workspace,
        string workspacePath)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in workspace.SnapshotState())
        {
            state[pair.Key] = pair.Value;
        }

        var lockPath = workspace.LockStore.Track(workspacePath);
        state[$"external-lock:{workspacePath}"] = PathState(lockPath);
        return state;
    }

    private static IReadOnlyDictionary<string, string> SnapshotExternalStore(
        PublishedJourneyWorkspace workspace)
    {
        var root = workspace.LockStore.LocalApplicationDataDirectory;
        return Directory.Exists(root)
            ? PublishedWorkspaceTreeSnapshot.Capture(root)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static IReadOnlyDictionary<string, string> SnapshotAllOwnedState(
        PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in workspace.SnapshotState())
        {
            state[$"workspace/{pair.Key}"] = pair.Value;
        }

        foreach (var pair in SnapshotExternalStore(workspace))
        {
            state[$"external/{pair.Key}"] = pair.Value;
        }

        return state;
    }

    private static string PathState(string path)
    {
        if (!File.Exists(path))
        {
            return "absent";
        }

        var info = new FileInfo(path);
        return string.Join(
            ";",
            $"length={info.Length}",
            $"creationUtcTicks={info.CreationTimeUtc.Ticks}",
            $"lastWriteUtcTicks={info.LastWriteTimeUtc.Ticks}",
            $"sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
    }

    private static string ReadBody(string document)
    {
        var normalized = document.Replace("\r\n", "\n", StringComparison.Ordinal);
        var first = normalized.IndexOf("---\n", StringComparison.Ordinal);
        if (first < 0)
        {
            return normalized;
        }

        var second = normalized.IndexOf("\n---\n", first + 4, StringComparison.Ordinal);
        return second < 0 ? normalized : normalized[(second + 5)..];
    }

    private static string ExtractFrontMatter(string document)
    {
        var normalized = document.Replace("\r\n", "\n", StringComparison.Ordinal);
        var first = normalized.IndexOf("---\n", StringComparison.Ordinal);
        if (first < 0)
        {
            return string.Empty;
        }

        var second = normalized.IndexOf("\n---\n", first + 4, StringComparison.Ordinal);
        return second < 0 ? string.Empty : normalized[(first + 4)..second];
    }

    private static string Normalize(string value)
        => value.Replace('\\', '/');

    private sealed record EntriesRegion(string Prefix, string Body, string Suffix);
    private sealed record ByteEntriesRegion(byte[] Prefix, byte[] Body, byte[] Suffix);
}
