using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F06ExactWorkspaceJourneyTests
{
    private const string WorkspaceAName = "workspace-a";
    private const string WorkspaceBName = "workspace-b";
    private const string UninstalledChildPath = WorkspaceAName + "/uninstalled-subdir";
    private const string ChildReadmePath = UninstalledChildPath + "/README.md";
    private const string BrokenPatternPath = WorkspaceBName + "/.agents/patterns/broken.md";
    private const string HealthyGuidancePath = WorkspaceBName + "/.agents/guidance/healthy-note.md";
    private const string GuidanceEntrypointPath = WorkspaceBName + "/.agents/guidance/_guidance.md";
    private const string ReadmeText = "# Uninstalled child\n\nThis authored README must survive.\n";
    private const string HealthyGuidanceText =
        "---\n"
        + "open-forge:\n"
        + "  description: Healthy note\n"
        + "  tags: [Guidance]\n"
        + "---\n"
        + "# Healthy note\n\nAn independently valid note.\n";
    private static readonly byte[] BrokenPatternBytes = Encoding.UTF8.GetBytes(
        "---\nopen-forge:\n  description: Broken pattern\n");

    [Fact(
        DisplayName = "F06 stays in the exact selected workspace across status, index preview, and a missing path")]
    [Trait("Feature", "exact-workspace-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F06")]
    public async Task MainJourneyUsesExplicitWorkspaceWithoutFallback()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f06-exact-workspace");
        var (workspaceA, workspaceB) = await SeedTwoWorkspacesAsync(
            workspace,
            includeUninstalledChild: true,
            includeBrokenPattern: true,
            includeHealthyGuidance: false);

        var missingWorkspace = workspace.Combine(WorkspaceAName, "missing-dir");
        var beforeReadOnlySequence = SnapshotAllOwnedTrees(workspace);
        var childStatus = await RunReadOnlyFrom(
            workspace,
            workspace.Combine(UninstalledChildPath),
            ["status"]);

        Assert.Equal(0, childStatus.ExitCode);
        Assert.Equal(string.Empty, childStatus.StandardError);
        Assert.Contains("not installed", childStatus.StandardOutput, StringComparison.OrdinalIgnoreCase);

        var childJson = await RunReadOnlyFrom(
            workspace,
            workspace.Combine(UninstalledChildPath),
            ["status", "--format=json"]);
        AssertWorkspaceJson(childJson, "status", workspace.Combine(UninstalledChildPath), "current-directory");
        using (var childDocument = JsonDocument.Parse(childJson.StandardOutput))
        {
            Assert.Equal(
                "uninstalled",
                childDocument.RootElement
                    .GetProperty("data")
                    .GetProperty("installation")
                    .GetProperty("state")
                    .GetString());
        }

        var explicitStatus = await RunReadOnlyFrom(
            workspace,
            workspaceB,
            ["status", "--workspace", workspaceA]);

        Assert.Equal(0, explicitStatus.ExitCode);
        Assert.Equal(string.Empty, explicitStatus.StandardError);
        Assert.Contains(
            Normalize(workspaceA),
            Normalize(explicitStatus.StandardOutput),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            Normalize(workspaceB),
            Normalize(explicitStatus.StandardOutput),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("broken.md", explicitStatus.StandardOutput, StringComparison.OrdinalIgnoreCase);

        var explicitStatusJson = await RunReadOnlyFrom(
            workspace,
            workspaceB,
            ["status", "--workspace", workspaceA, "--format=json"]);
        AssertWorkspaceJson(explicitStatusJson, "status", workspaceA, "explicit-workspace");
        AssertJsonExcludesWorkspaceBFacts(explicitStatusJson, workspace, workspaceB);
        using (var statusDocument = JsonDocument.Parse(explicitStatusJson.StandardOutput))
        {
            Assert.Equal(
                "installed",
                statusDocument.RootElement
                    .GetProperty("data")
                    .GetProperty("installation")
                    .GetProperty("state")
                    .GetString());
        }

        var indexPreview = await RunReadOnlyFrom(
            workspace,
            workspaceB,
            ["index", "--workspace", workspaceA, "--dry-run"]);

        Assert.Equal(0, indexPreview.ExitCode);
        Assert.Equal(string.Empty, indexPreview.StandardError);
        Assert.NotEmpty(indexPreview.StandardOutput);
        Assert.DoesNotContain(
            Normalize(workspaceB),
            Normalize(indexPreview.StandardOutput),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("broken.md", indexPreview.StandardOutput, StringComparison.OrdinalIgnoreCase);

        var indexJson = await RunReadOnlyFrom(
            workspace,
            workspaceB,
            ["index", "--workspace", workspaceA, "--dry-run", "--format=json"]);
        AssertWorkspaceJson(indexJson, "index", workspaceA, "explicit-workspace");
        AssertJsonExcludesWorkspaceBFacts(indexJson, workspace, workspaceB);
        using (var indexDocument = JsonDocument.Parse(indexJson.StandardOutput))
        {
            Assert.Equal(
                "dry-run",
                indexDocument.RootElement.GetProperty("data").GetProperty("mode").GetString());
        }

        var missingStatus = await RunReadOnlyFrom(
            workspace,
            workspaceA,
            ["status", "--workspace", missingWorkspace]);

        Assert.Equal(5, missingStatus.ExitCode);
        Assert.Equal(string.Empty, missingStatus.StandardOutput);
        Assert.NotEmpty(missingStatus.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(
            beforeReadOnlySequence,
            SnapshotAllOwnedTrees(workspace));
    }

    [Fact(
        DisplayName = "F06 reports malformed B while indexing an independently valid guidance note")]
    [Trait("Feature", "exact-workspace-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F06")]
    public async Task MalformedSelectedWorkspaceContinuesIndependentGuidanceIndexing()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f06-malformed-selected-workspace");
        var (workspaceA, workspaceB) = await SeedTwoWorkspacesAsync(
            workspace,
            includeUninstalledChild: false,
            includeBrokenPattern: true,
            includeHealthyGuidance: true);

        var beforeWorkspace = workspace.SnapshotState();
        var beforeA = PublishedWorkspaceTreeSnapshot.Capture(workspaceA);
        var beforeB = PublishedWorkspaceTreeSnapshot.Capture(workspaceB);
        var beforeDataHome = SnapshotDataHome(workspace);
        var brokenBefore = File.ReadAllBytes(workspace.Combine(BrokenPatternPath));
        var healthyBefore = File.ReadAllBytes(workspace.Combine(HealthyGuidancePath));
        var guidanceBefore = File.ReadAllBytes(workspace.Combine(GuidanceEntrypointPath));
        var lockBytesBefore = CaptureLockBytes(workspace, workspaceA, workspaceB);

        var index = await PublishedJourneyProcess.RunAsync(
            workspace.Target,
            workspaceB,
            ["index"],
            workspace.ProcessEnvironment);

        Assert.Equal(3, index.ExitCode);
        Assert.Equal(string.Empty, index.StandardError);
        Assert.NotEmpty(index.StandardOutput);
        Assert.Contains("broken.md", index.StandardOutput, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(brokenBefore, File.ReadAllBytes(workspace.Combine(BrokenPatternPath)));
        Assert.Equal(healthyBefore, File.ReadAllBytes(workspace.Combine(HealthyGuidancePath)));
        Assert.NotEqual(
            guidanceBefore,
            File.ReadAllBytes(workspace.Combine(GuidanceEntrypointPath)));
        AssertEntriesOutsideRegionPreserved(
            guidanceBefore,
            File.ReadAllBytes(workspace.Combine(GuidanceEntrypointPath)));
        Assert.Contains(
            "healthy-note.md",
            Normalize(File.ReadAllText(workspace.Combine(GuidanceEntrypointPath), Encoding.UTF8)),
            StringComparison.OrdinalIgnoreCase);

        var afterWorkspace = workspace.SnapshotState();
        var afterA = PublishedWorkspaceTreeSnapshot.Capture(workspaceA);
        var afterB = PublishedWorkspaceTreeSnapshot.Capture(workspaceB);
        var afterDataHome = SnapshotDataHome(workspace);
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            beforeWorkspace,
            afterWorkspace,
            GuidanceEntrypointPath);
        PublishedJourneyAssertions.AssertOnlyFileMutations(
            beforeB,
            afterB,
            ".agents/guidance/_guidance.md");
        Assert.Equal(beforeA, afterA);
        AssertLockBytesPreserved(workspace, lockBytesBefore);
        // This is a mutating incomplete index, not a read-only operation: only ordinary empty
        // recovery infrastructure may differ in the isolated data home.
        AssertRecoveryInfrastructureOnlyContainsEmptyDirectories(
            beforeDataHome,
            afterDataHome,
            workspace);
        workspace.LockStore.AssertPersistentZeroByteLock(workspaceA);
        workspace.LockStore.AssertPersistentZeroByteLock(workspaceB);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspaceA);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspaceB);
    }

    private static async Task<(string WorkspaceA, string WorkspaceB)> SeedTwoWorkspacesAsync(
        PublishedJourneyWorkspace workspace,
        bool includeUninstalledChild,
        bool includeBrokenPattern,
        bool includeHealthyGuidance)
    {
        var workspaceA = workspace.Combine(WorkspaceAName);
        var workspaceB = workspace.Combine(WorkspaceBName);
        workspace.LockStore.Track(workspaceA);
        workspace.LockStore.Track(workspaceB);

        var reserved = CoreInstallFiles(WorkspaceAName)
            .Concat(CoreInstallFiles(WorkspaceBName))
            .ToList();

        if (includeUninstalledChild)
        {
            reserved.Add(ChildReadmePath);
        }

        if (includeBrokenPattern)
        {
            reserved.Add(BrokenPatternPath);
        }

        if (includeHealthyGuidance)
        {
            reserved.Add(HealthyGuidancePath);
        }

        workspace.ExpectFiles(reserved.ToArray());
        Directory.CreateDirectory(workspaceA);
        Directory.CreateDirectory(workspaceB);

        await InstallAsync(workspace, workspaceA);
        await InstallAsync(workspace, workspaceB);

        if (includeUninstalledChild)
        {
            workspace.WriteText(ChildReadmePath, ReadmeText);
        }

        if (includeBrokenPattern)
        {
            workspace.WriteBytes(BrokenPatternPath, BrokenPatternBytes);
        }

        if (includeHealthyGuidance)
        {
            workspace.WriteText(HealthyGuidancePath, HealthyGuidanceText);
        }

        return (workspaceA, workspaceB);
    }

    private static async Task InstallAsync(
        PublishedJourneyWorkspace workspace,
        string workspacePath)
    {
        var result = await PublishedJourneyProcess.RunAsync(
            workspace.Target,
            workspace.Path,
            ["install", "--automatic", "--workspace", workspacePath],
            workspace.ProcessEnvironment);
        AssertSuccessfulHuman(result);
        workspace.LockStore.AssertPersistentZeroByteLock(workspacePath);
    }

    private static Task<ProcessRunResult> RunReadOnlyFrom(
        PublishedJourneyWorkspace workspace,
        string workingDirectory,
        IReadOnlyList<string> arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workingDirectory,
            () => SnapshotAllOwnedTrees(workspace),
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertWorkspaceJson(
        ProcessRunResult result,
        string command,
        string expectedWorkspace,
        string selectedBy)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(command, root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(
            Path.GetFullPath(expectedWorkspace),
            root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(selectedBy, root.GetProperty("workspace").GetProperty("selectedBy").GetString());
    }

    private static string[] CoreInstallFiles(string root)
        => PublishedInstallWorkspace.EmbeddedPayloadPaths
            .Select(path => $"{root}/{path}")
            .Append($"{root}/AGENTS.md")
            .Append($"{root}/CLAUDE.md")
            .Append($"{root}/.agents/open-forge.lock.json")
            .ToArray();

    private static IReadOnlyDictionary<string, string> SnapshotAllOwnedTrees(
        PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        AddTreeSnapshot(state, "workspace-root", workspace.Path);
        AddTreeSnapshot(state, WorkspaceAName, workspace.Combine(WorkspaceAName));
        AddTreeSnapshot(state, WorkspaceBName, workspace.Combine(WorkspaceBName));
        AddTreeSnapshot(state, "isolated-data-home", workspace.LockStore.LocalApplicationDataDirectory);

        return state;
    }

    private static IReadOnlyDictionary<string, string> SnapshotDataHome(
        PublishedJourneyWorkspace workspace)
    {
        var dataHome = workspace.LockStore.LocalApplicationDataDirectory;
        return Directory.Exists(dataHome)
            ? PublishedWorkspaceTreeSnapshot.Capture(dataHome)
            : new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["."] = "absent",
            };
    }

    private static void AddTreeSnapshot(
        IDictionary<string, string> state,
        string prefix,
        string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            state[$"{prefix}/."] = "absent";
            return;
        }

        foreach (var pair in PublishedWorkspaceTreeSnapshot.Capture(rootPath))
        {
            state[$"{prefix}/{pair.Key}"] = pair.Value;
        }
    }

    private static IReadOnlyDictionary<string, byte[]> CaptureLockBytes(
        PublishedJourneyWorkspace workspace,
        params string[] workspacePaths)
    {
        var state = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var workspacePath in workspacePaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var lockPath = workspace.LockStore.Track(workspacePath);
            state[workspacePath] = File.ReadAllBytes(lockPath);
        }

        return state;
    }

    private static void AssertLockBytesPreserved(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, byte[]> before)
    {
        foreach (var pair in before)
        {
            var lockPath = workspace.LockStore.Track(pair.Key);
            Assert.Equal(pair.Value, File.ReadAllBytes(lockPath));
        }
    }

    private static void AssertJsonExcludesWorkspaceBFacts(
        ProcessRunResult result,
        PublishedJourneyWorkspace workspace,
        string workspaceB)
    {
        using var document = JsonDocument.Parse(result.StandardOutput);
        var brokenAbsolutePath = workspace.Combine(BrokenPatternPath);
        var forbidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Normalize(workspaceB),
            Normalize(Path.GetFullPath(workspaceB)),
            Normalize(workspace.Combine(BrokenPatternPath)),
            Normalize(Path.GetFullPath(brokenAbsolutePath)),
            Normalize(BrokenPatternPath),
            Normalize($"{WorkspaceBName}/.agents/patterns/broken.md"),
            ".agents/patterns/broken.md",
            "broken.md",
        };

        foreach (var value in EnumerateJsonStringValues(document.RootElement))
        {
            var normalized = Normalize(value);
            foreach (var forbiddenValue in forbidden)
            {
                Assert.DoesNotContain(forbiddenValue, normalized, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    private static IEnumerable<string> EnumerateJsonStringValues(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                yield return element.GetString() ?? string.Empty;
                yield break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    foreach (var value in EnumerateJsonStringValues(property.Value))
                    {
                        yield return value;
                    }
                }

                yield break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var value in EnumerateJsonStringValues(item))
                    {
                        yield return value;
                    }
                }

                yield break;
            default:
                yield break;
        }
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

    private static void AssertRecoveryInfrastructureOnlyContainsEmptyDirectories(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        PublishedJourneyWorkspace workspace)
    {
        var dataHome = workspace.LockStore.LocalApplicationDataDirectory;
        var recoveryRoot = Normalize(
            Path.GetRelativePath(dataHome, workspace.LockStore.RecoveryStoreRoot));

        foreach (var path in before.Keys.Union(after.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            var previousExists = before.TryGetValue(path, out var previous);
            var currentExists = after.TryGetValue(path, out var current);
            if (previousExists && currentExists && string.Equals(previous, current, StringComparison.Ordinal))
            {
                continue;
            }

            Assert.True(
                IsRecoveryPathOrParent(path, recoveryRoot),
                $"Unexpected isolated data-home mutation: {path}");
            Assert.True(currentExists, $"The isolated data-home entry was removed: {path}");
            Assert.StartsWith("type=directory;", current!, StringComparison.Ordinal);

            if (previousExists)
            {
                Assert.StartsWith("type=directory;", previous!, StringComparison.Ordinal);
                Assert.Equal(WithoutLastWrite(previous!), WithoutLastWrite(current!));
            }

            if (IsRecoveryPath(path, recoveryRoot))
            {
                AssertNoFilesOrLinksBelow(after, path);
            }
        }

        foreach (var pair in after)
        {
            if (IsRecoveryPath(pair.Key, recoveryRoot))
            {
                Assert.StartsWith("type=directory;", pair.Value, StringComparison.Ordinal);
            }
        }
    }

    private static void AssertNoFilesOrLinksBelow(
        IReadOnlyDictionary<string, string> snapshot,
        string relativePath)
    {
        Assert.True(
            snapshot.TryGetValue(relativePath, out var rootDescription),
            $"Recovery infrastructure is missing from its complete snapshot: {relativePath}");
        Assert.StartsWith("type=directory;", rootDescription!, StringComparison.Ordinal);
        foreach (var pair in snapshot)
        {
            if (pair.Key.Equals(relativePath, StringComparison.Ordinal)
                || !pair.Key.StartsWith(relativePath + "/", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.StartsWith("type=directory;", pair.Value, StringComparison.Ordinal);
        }
    }

    private static bool IsRecoveryPathOrParent(string path, string recoveryRoot)
        => IsRecoveryPath(path, recoveryRoot)
            || path == "."
            || recoveryRoot.StartsWith(path + "/", StringComparison.Ordinal);

    private static bool IsRecoveryPath(string path, string recoveryRoot)
        => path.Equals(recoveryRoot, StringComparison.Ordinal)
            || path.StartsWith(recoveryRoot + "/", StringComparison.Ordinal);

    private static string WithoutLastWrite(string description)
        => string.Join(';', description.Split(';').Where(
            field => !field.StartsWith("lastWriteUtcTicks=", StringComparison.Ordinal)));

    private sealed record ByteEntriesRegion(byte[] Prefix, byte[] Body, byte[] Suffix);

    private static void AssertSuccessfulHuman(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static string Normalize(string value)
        => value.Replace('\\', '/');
}
