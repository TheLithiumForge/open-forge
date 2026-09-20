using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F18UnavailableLibrarySourceJourneyTests
{
    private const string LibraryId = "team";
    private const string SourceRoot = "source";
    private const string PreservedSourceRoot = "preserved-source";
    private const string GuidanceDestinationRoot = ".agents/guidance/team";
    private const string TeamEntrypointPath = GuidanceDestinationRoot + "/_team.md";
    private const string ExternalDestinationRoot = "docs";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string SettingsPath = ".agents/open-forge.json";
    private const string OnePath = "one.md";
    private const string TwoPath = "two.md";

    private const string OneSourceBody = """
        ---
        open-forge:
          description: One library note
          tags: [Guidance]
        ---
        # One

        Source-owned one bytes.
        """;

    private const string TwoSourceBody = """
        ---
        open-forge:
          description: Two library note
          tags: [Guidance]
        ---
        # Two

        Source-owned two bytes.
        """;

    private const string TeamEntrypointBody =
        "---\n"
        + "open-forge:\n"
        + "  description: Team guidance\n"
        + "  tags: [Guidance]\n"
        + "---\n"
        + "# Team guidance\n\n"
        + "## Entries\n\n"
        + "- none - No entries - #Empty\n";

    [Fact(DisplayName = "F18 lists, inspects, syncs, and detaches after the source root disappears"), Trait("Feature", "library-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F18"), Trait("Scenarios", "X17,C24-06,C25-07,C27-09")]
    public async Task DetachesKnownDanglingLinksWithoutReadingTheMissingSource()
    {
        using var workspace = await CreateAttachedWorkspaceAsync("f18-main", GuidanceDestinationRoot);
        var oneSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}"));
        var twoSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}"));

        RenameSource(workspace);
        AssertDanglingLinks(workspace, GuidanceDestinationRoot);
        AssertPreservedSourceBytes(workspace, oneSourceBytes, twoSourceBytes);

        var listed = await RunReadOnlyAsync(workspace, "library", "list");
        AssertStdoutResult(listed, 2);
        AssertOutputContains(listed.StandardOutput, LibraryId);
        AssertOutputContainsAny(listed.StandardOutput, "source", "missing", "unavailable", "dangling");
        AssertDanglingLinks(workspace, GuidanceDestinationRoot);

        var inspected = await RunReadOnlyAsync(workspace, "library", "inspect", LibraryId);
        AssertStdoutResult(inspected, 3);
        AssertOutputContains(inspected.StandardOutput, LibraryId);
        AssertOutputContainsAny(inspected.StandardOutput, "comparison", "could not", "unavailable", "incomplete");
        AssertDanglingLinks(workspace, GuidanceDestinationRoot);

        var synchronized = await RunReadOnlyAsync(workspace, "library", "sync", LibraryId, "--automatic");
        AssertStdoutResult(synchronized, 3);
        AssertOutputContains(synchronized.StandardOutput, LibraryId);
        AssertOutputContainsAny(synchronized.StandardOutput, "could not", "source", "incomplete");
        AssertDanglingLinks(workspace, GuidanceDestinationRoot);
        Assert.Equal([OnePath, TwoPath], ReadLibraryPaths(workspace));
        AssertPreservedSourceBytes(workspace, oneSourceBytes, twoSourceBytes);

        var detached = await workspace.RunAsync("library", "detach", LibraryId, "--automatic", "--detail=standard");
        AssertStdoutResult(detached, 0);
        AssertOutputContains(detached.StandardOutput, OnePath, TwoPath, "removed");
        Assert.Null(new FileInfo(workspace.Combine(DestinationPath(GuidanceDestinationRoot, OnePath))).LinkTarget);
        Assert.Null(new FileInfo(workspace.Combine(DestinationPath(GuidanceDestinationRoot, TwoPath))).LinkTarget);
        AssertNoLibraryRegistration(workspace);
        Assert.False(Directory.Exists(workspace.Combine(SourceRoot)));
        AssertPreservedSourceBytes(workspace, oneSourceBytes, twoSourceBytes);
        AssertAppliedInfrastructure(workspace);
    }

    [Fact(DisplayName = "F18 detach distinguishes an absent destination while removing remaining dangling links"), Trait("Feature", "library-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F18"), Trait("Scenarios", "C28-05")]
    public async Task DetachAbsentDestinationAfterSourceDisappearsReleasesRegistration()
    {
        using var workspace = await CreateAttachedWorkspaceAsync("f18-detach-absent", GuidanceDestinationRoot);
        var oneSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}"));
        var twoSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}"));
        var oneDestination = workspace.Combine(DestinationPath(GuidanceDestinationRoot, OnePath));
        var twoDestination = workspace.Combine(DestinationPath(GuidanceDestinationRoot, TwoPath));

        RenameSource(workspace);
        File.Delete(oneDestination);
        Assert.Null(new FileInfo(oneDestination).LinkTarget);
        AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(GuidanceDestinationRoot, TwoPath));
        var preview = await RunReadOnlyAsync(
            workspace,
            "library", "detach", LibraryId, "--dry-run", "--detail=standard", "--format=json");
        AssertDryRunDeletesOnly(
            preview,
            DestinationPath(GuidanceDestinationRoot, TwoPath),
            DestinationPath(GuidanceDestinationRoot, OnePath));

        var detached = await workspace.RunAsync("library", "detach", LibraryId, "--automatic", "--detail=standard");

        AssertStdoutResult(detached, 2);
        AssertOutputContains(detached.StandardOutput, OnePath, TwoPath);
        AssertOutputContains(detached.StandardOutput, "removed");
        AssertOutputContainsAny(detached.StandardOutput, "already absent", "absent");
        Assert.Null(new FileInfo(oneDestination).LinkTarget);
        Assert.Null(new FileInfo(twoDestination).LinkTarget);
        AssertNoLibraryRegistration(workspace);
        Assert.False(Directory.Exists(workspace.Combine(SourceRoot)));
        AssertPreservedSourceBytes(workspace, oneSourceBytes, twoSourceBytes);
        AssertAppliedInfrastructure(workspace);
    }

    [Fact(DisplayName = "F18 revoked external permission blocks automatic detach without effects"), Trait("Feature", "library-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F18"), Trait("Scenarios", "C28-08")]
    public async Task RevokedExternalPermissionBlocksAutomaticDetachWithoutEffects()
    {
        using var workspace = await CreateAttachedWorkspaceAsync("f18-permission", ExternalDestinationRoot, allowExternalPath: true);
        var oneSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}"));
        var twoSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}"));

        workspace.WriteText(SettingsPath, "{\"allowInstallPaths\":[]}");
        RenameSource(workspace);
        var before = SnapshotState(workspace);
        AssertDanglingLinks(workspace, ExternalDestinationRoot);

        var blocked = await RunReadOnlyAsync(workspace, "library", "detach", LibraryId, "--automatic", "--detail=standard");

        AssertStderrResult(blocked, 5);
        AssertOutputContains(blocked.StandardError, ExternalDestinationRoot, "permission");
        Assert.Equal(before, SnapshotState(workspace));
        Assert.Equal("{\"allowInstallPaths\":[]}", File.ReadAllText(workspace.Combine(SettingsPath)));
        AssertDanglingLinks(workspace, ExternalDestinationRoot);
        Assert.Equal([OnePath, TwoPath], ReadLibraryPaths(workspace));
        Assert.False(Directory.Exists(workspace.Combine(SourceRoot)));
        AssertPreservedSourceBytes(workspace, oneSourceBytes, twoSourceBytes);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static async Task<PublishedJourneyWorkspace> CreateAttachedWorkspaceAsync(
        string purpose,
        string destinationRoot,
        bool allowExternalPath = false)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(
                $"{SourceRoot}/{OnePath}",
                $"{SourceRoot}/{TwoPath}",
                $"{PreservedSourceRoot}/{OnePath}",
                $"{PreservedSourceRoot}/{TwoPath}",
                DestinationPath(destinationRoot, OnePath),
                DestinationPath(destinationRoot, TwoPath));
            if (allowExternalPath)
            {
                workspace.ExpectFiles(SettingsPath);
            }

            workspace.WriteText($"{SourceRoot}/{OnePath}", OneSourceBody);
            workspace.WriteText($"{SourceRoot}/{TwoPath}", TwoSourceBody);

            var installed = await workspace.RunAsync("install", "--automatic");
            AssertSetupSuccess(installed, "install");
            if (string.Equals(destinationRoot, GuidanceDestinationRoot, StringComparison.Ordinal))
            {
                workspace.ExpectFiles(TeamEntrypointPath);
                workspace.WriteText(TeamEntrypointPath, TeamEntrypointBody);
                Assert.Equal(
                    Encoding.UTF8.GetBytes(TeamEntrypointBody),
                    File.ReadAllBytes(workspace.Combine(TeamEntrypointPath)));
            }

            var attachArguments = new List<string>
            {
                "library", "attach", LibraryId, SourceRoot, "--to", destinationRoot,
            };
            if (allowExternalPath)
            {
                attachArguments.Add("--allow-path");
                attachArguments.Add(destinationRoot);
            }

            attachArguments.Add("--automatic");
            var attached = await workspace.RunAsync(attachArguments.ToArray());
            AssertSetupSuccess(attached, "library attach");
            AssertRelativeLink(workspace, $"{SourceRoot}/{OnePath}", DestinationPath(destinationRoot, OnePath));
            AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(destinationRoot, TwoPath));
            Assert.Equal([OnePath, TwoPath], ReadLibraryPaths(workspace));
            Assert.Equal(Encoding.UTF8.GetBytes(OneSourceBody), File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
            Assert.Equal(Encoding.UTF8.GetBytes(TwoSourceBody), File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));
            if (allowExternalPath)
            {
                Assert.Contains(destinationRoot, File.ReadAllText(workspace.Combine(SettingsPath)), StringComparison.Ordinal);
            }

            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static Task<ProcessRunResult> RunReadOnlyAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotState(workspace),
            arguments,
            workspace.ProcessEnvironment);

    private static void RenameSource(PublishedJourneyWorkspace workspace)
        => Directory.Move(workspace.Combine(SourceRoot), workspace.Combine(PreservedSourceRoot));

    private static string DestinationPath(string destinationRoot, string sourcePath)
        => $"{destinationRoot}/{sourcePath}";

    private static string ExpectedTarget(
        PublishedJourneyWorkspace workspace,
        string sourcePath,
        string destinationPath)
    {
        var destinationParent = Path.GetDirectoryName(workspace.Combine(destinationPath))
            ?? throw new InvalidOperationException("The destination link has no parent directory.");
        return Path.GetRelativePath(destinationParent, workspace.Combine(sourcePath)).Replace('\\', '/');
    }

    private static void AssertRelativeLink(
        PublishedJourneyWorkspace workspace,
        string sourcePath,
        string destinationPath)
    {
        var actual = new FileInfo(workspace.Combine(destinationPath)).LinkTarget;
        Assert.NotNull(actual);
        Assert.Equal(ExpectedTarget(workspace, sourcePath, destinationPath), actual);
    }

    private static void AssertDanglingLinks(
        PublishedJourneyWorkspace workspace,
        string destinationRoot)
    {
        AssertRelativeLink(workspace, $"{SourceRoot}/{OnePath}", DestinationPath(destinationRoot, OnePath));
        AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(destinationRoot, TwoPath));
        Assert.False(Directory.Exists(workspace.Combine(SourceRoot)));
    }

    private static void AssertPreservedSourceBytes(
        PublishedJourneyWorkspace workspace,
        byte[] oneBytes,
        byte[] twoBytes)
    {
        Assert.Equal(oneBytes, File.ReadAllBytes(workspace.Combine($"{PreservedSourceRoot}/{OnePath}")));
        Assert.Equal(twoBytes, File.ReadAllBytes(workspace.Combine($"{PreservedSourceRoot}/{TwoPath}")));
    }

    private static void AssertNoLibraryRegistration(PublishedJourneyWorkspace workspace)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        Assert.Empty(document.RootElement.GetProperty("libraries").EnumerateArray());
    }

    private static string[] ReadLibraryPaths(PublishedJourneyWorkspace workspace)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var library = Assert.Single(document.RootElement.GetProperty("libraries").EnumerateArray());
        Assert.Equal(LibraryId, library.GetProperty("id").GetString());
        return library.GetProperty("paths")
            .EnumerateArray()
            .Select(path => path.GetString() ?? throw new InvalidOperationException("A Library path was null."))
            .ToArray();
    }

    private static void AssertSetupSuccess(ProcessRunResult result, string operation)
    {
        Assert.True(result.ExitCode == 0,
            $"{operation} failed with exit {result.ExitCode}.\nstdout:\n{result.StandardOutput}\nstderr:\n{result.StandardError}");
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertStdoutResult(ProcessRunResult result, int exitCode)
    {
        Assert.Equal(exitCode, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
    }

    private static void AssertStderrResult(ProcessRunResult result, int exitCode)
    {
        Assert.Equal(exitCode, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
    }

    private static void AssertOutputContains(string output, params string[] values)
    {
        foreach (var value in values)
        {
            Assert.Contains(value, output, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void AssertOutputContainsAny(string output, params string[] values)
    {
        Assert.True(
            values.Any(value => output.Contains(value, StringComparison.OrdinalIgnoreCase)),
            $"Expected one of [{string.Join(", ", values)}] in output:\n{output}");
    }

    private static void AssertDryRunDeletesOnly(
        ProcessRunResult result,
        string deletedPath,
        params string[] nonDeletedPaths)
    {
        Assert.Equal(string.Empty, result.StandardError);
        Assert.NotEmpty(result.StandardOutput);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var effects = document.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(
            effects,
            effect => IsDeleteEffect(effect, deletedPath));
        foreach (var nonDeletedPath in nonDeletedPaths)
        {
            Assert.DoesNotContain(
                effects,
                effect => IsDeleteEffect(effect, nonDeletedPath));
        }
    }

    private static bool IsDeleteEffect(JsonElement effect, string path)
        => effect.TryGetProperty("path", out var effectPath)
            && effectPath.GetString() == path
            && effect.TryGetProperty("action", out var action)
            && action.GetString() == "delete";

    private static IReadOnlyDictionary<string, string> SnapshotState(PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in workspace.SnapshotState())
        {
            state[$"workspace/{entry.Key}"] = entry.Value;
        }

        AddDirectorySnapshot(state, "data-home", workspace.LockStore.LocalApplicationDataDirectory);
        AddDirectorySnapshot(state, "recovery-workspace", workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));
        return new ReadOnlyDictionary<string, string>(state);
    }

    private static void AddDirectorySnapshot(
        IDictionary<string, string> state,
        string prefix,
        string path)
    {
        if (!Directory.Exists(path))
        {
            state[prefix] = File.Exists(path)
                ? FileState(path)
                : "absent";
            return;
        }

        foreach (var entry in PublishedWorkspaceTreeSnapshot.Capture(path))
        {
            state[$"{prefix}/{entry.Key}"] = entry.Value;
        }
    }

    private static string FileState(string path)
        => $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}";

    private static void AssertAppliedInfrastructure(PublishedJourneyWorkspace workspace)
    {
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }
}
