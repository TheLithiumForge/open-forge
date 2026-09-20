using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F17LibraryDestinationJourneyTests
{
    private const string LibraryId = "team";
    private const string SourceRoot = "source";
    private const string DestinationRoot = ".agents/guidance/team";
    private const string TeamEntrypointPath = DestinationRoot + "/_team.md";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string OnePath = "one.md";
    private const string TwoPath = "two.md";
    private const string ThreePath = "three.md";

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

    private const string ThreeSourceBody = """
        ---
        open-forge:
          description: Three library note
          tags: [Guidance]
        ---
        # Three

        Source-owned three bytes.
        """;

    private const string ChangedOneBody = "User-owned one bytes; do not delete.\n";

    private const string TeamEntrypointBody =
        "---\n"
        + "open-forge:\n"
        + "  description: Team guidance\n"
        + "  tags: [Guidance]\n"
        + "---\n"
        + "# Team guidance\n\n"
        + "## Entries\n\n"
        + "- none - No entries - #Empty\n";

    [Fact(DisplayName = "F17 repairs a missing link, preserves a changed destination, and reaches current membership"), Trait("Feature", "library-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F17"), Trait("Scenarios", "C24-03,C27-08,C27-07,X18,C25-01")]
    public async Task RepairsMissingThenChangedDestinationAndInspectsCurrentMembership()
    {
        using var workspace = await CreateAttachedWorkspaceAsync("f17-main");
        var oneSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}"));
        var twoSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}"));
        var oneDestination = workspace.Combine(DestinationPath(OnePath));
        var twoDestination = workspace.Combine(DestinationPath(TwoPath));
        var threeDestination = workspace.Combine(DestinationPath(ThreePath));

        File.Delete(oneDestination);
        var listed = await RunReadOnlyAsync(workspace, "library", "list");
        AssertStdoutResult(listed, 2);
        AssertOutputContains(listed.StandardOutput, DestinationPath(OnePath), "missing", "sync");
        Assert.Null(new FileInfo(oneDestination).LinkTarget);
        Assert.Equal(oneSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));

        var restored = await workspace.RunAsync("library", "sync", LibraryId, "--automatic");
        AssertStdoutResult(restored, 2);
        AssertRestorationReported(restored.StandardOutput, DestinationPath(OnePath));
        AssertRelativeLink(workspace, $"{SourceRoot}/{OnePath}", DestinationPath(OnePath));
        Assert.Equal(oneSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
        Assert.Equal(twoSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));

        File.Delete(oneDestination);
        workspace.WriteBytes(DestinationPath(OnePath), Encoding.UTF8.GetBytes(ChangedOneBody));
        workspace.WriteText($"{SourceRoot}/{ThreePath}", ThreeSourceBody);

        var partiallyCompleted = await workspace.RunAsync("library", "sync", LibraryId, "--automatic");
        AssertStdoutResult(partiallyCompleted, 3);
        AssertOutputContains(partiallyCompleted.StandardOutput, LibraryId, OnePath, ThreePath);
        AssertPartialChangedResult(partiallyCompleted.StandardOutput);
        AssertOrdinaryFileWithBytes(oneDestination, Encoding.UTF8.GetBytes(ChangedOneBody));
        AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(TwoPath));
        AssertRelativeLink(workspace, $"{SourceRoot}/{ThreePath}", DestinationPath(ThreePath));
        Assert.Equal(oneSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
        Assert.Equal(twoSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));
        Assert.Equal(Encoding.UTF8.GetBytes(ThreeSourceBody), File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{ThreePath}")));
        Assert.Equal([OnePath, ThreePath, TwoPath], ReadLibraryPaths(workspace));

        var backupPath = workspace.Combine("backups/one.md");
        Directory.CreateDirectory(workspace.Combine("backups"));
        File.Move(oneDestination, backupPath);
        var backupBytes = File.ReadAllBytes(backupPath);

        var repaired = await workspace.RunAsync("library", "sync", LibraryId, "--automatic");
        AssertStdoutResult(repaired, 2);
        AssertRestorationReported(repaired.StandardOutput, DestinationPath(OnePath));
        Assert.Equal(backupBytes, File.ReadAllBytes(backupPath));
        AssertRelativeLink(workspace, $"{SourceRoot}/{OnePath}", DestinationPath(OnePath));
        AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(TwoPath));
        AssertRelativeLink(workspace, $"{SourceRoot}/{ThreePath}", DestinationPath(ThreePath));
        Assert.Equal([OnePath, ThreePath, TwoPath], ReadLibraryPaths(workspace));

        var inspected = await RunReadOnlyAsync(workspace, "library", "inspect", LibraryId);
        AssertStdoutResult(inspected, 0);
        AssertOutputContains(inspected.StandardOutput, "current");
        Assert.Equal([OnePath, ThreePath, TwoPath], ReadLibraryPaths(workspace));
        Assert.Equal(oneSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
        Assert.Equal(twoSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));
        Assert.Equal(Encoding.UTF8.GetBytes(ThreeSourceBody), File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{ThreePath}")));
        AssertAppliedInfrastructure(workspace);
    }

    [Fact(DisplayName = "F17 detach preserves a changed destination while releasing the other link and registration"), Trait("Feature", "library-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F17"), Trait("Scenarios", "C28-06,X18")]
    public async Task DetachChangedDestinationPreservesUserBytesAndReleasesOtherLinks()
    {
        using var workspace = await CreateAttachedWorkspaceAsync("f17-detach-changed");
        var oneSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}"));
        var twoSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}"));
        var oneDestination = workspace.Combine(DestinationPath(OnePath));
        var twoDestination = workspace.Combine(DestinationPath(TwoPath));

        File.Delete(oneDestination);
        var userBytes = Encoding.UTF8.GetBytes(ChangedOneBody);
        workspace.WriteBytes(DestinationPath(OnePath), userBytes);
        var preview = await RunReadOnlyAsync(
            workspace,
            "library", "detach", LibraryId, "--dry-run", "--detail=standard", "--format=json");
        AssertDryRunDeletesOnly(preview, DestinationPath(TwoPath), DestinationPath(OnePath));
        var detached = await workspace.RunAsync("library", "detach", LibraryId, "--automatic", "--detail=standard");

        AssertStdoutResult(detached, 2);
        AssertOutputContains(detached.StandardOutput, OnePath, TwoPath);
        AssertOutputContains(detached.StandardOutput, "removed");
        AssertOutputContainsAny(detached.StandardOutput, "retained", "preserved", "kept");
        AssertOrdinaryFileWithBytes(oneDestination, userBytes);
        Assert.Null(new FileInfo(twoDestination).LinkTarget);
        AssertNoLibraryRegistration(workspace);
        Assert.Equal(oneSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
        Assert.Equal(twoSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));
        AssertAppliedInfrastructure(workspace);
    }

    [Fact(DisplayName = "F17 detach distinguishes an absent destination while removing the other exact link"), Trait("Feature", "library-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F17"), Trait("Scenarios", "C28-05,X18")]
    public async Task DetachAbsentDestinationPreservesSourceAndReleasesRegistration()
    {
        using var workspace = await CreateAttachedWorkspaceAsync("f17-detach-absent");
        var oneSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}"));
        var twoSourceBytes = File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}"));
        var oneDestination = workspace.Combine(DestinationPath(OnePath));
        var twoDestination = workspace.Combine(DestinationPath(TwoPath));

        File.Delete(oneDestination);
        Assert.Null(new FileInfo(oneDestination).LinkTarget);
        AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(TwoPath));
        var preview = await RunReadOnlyAsync(
            workspace,
            "library", "detach", LibraryId, "--dry-run", "--detail=standard", "--format=json");
        AssertDryRunDeletesOnly(preview, DestinationPath(TwoPath), DestinationPath(OnePath));

        var detached = await workspace.RunAsync("library", "detach", LibraryId, "--automatic", "--detail=standard");

        AssertStdoutResult(detached, 2);
        AssertOutputContains(detached.StandardOutput, OnePath, TwoPath);
        AssertOutputContains(detached.StandardOutput, "removed");
        AssertOutputContainsAny(detached.StandardOutput, "already absent", "absent");
        Assert.Null(new FileInfo(oneDestination).LinkTarget);
        Assert.Null(new FileInfo(twoDestination).LinkTarget);
        AssertNoLibraryRegistration(workspace);
        Assert.Equal(oneSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
        Assert.Equal(twoSourceBytes, File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));
        AssertAppliedInfrastructure(workspace);
    }

    private static async Task<PublishedJourneyWorkspace> CreateAttachedWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(
                $"{SourceRoot}/{OnePath}",
                $"{SourceRoot}/{TwoPath}",
                $"{SourceRoot}/{ThreePath}",
                DestinationPath(OnePath),
                DestinationPath(TwoPath),
                DestinationPath(ThreePath),
                "backups/one.md");
            workspace.WriteText($"{SourceRoot}/{OnePath}", OneSourceBody);
            workspace.WriteText($"{SourceRoot}/{TwoPath}", TwoSourceBody);

            var installed = await workspace.RunAsync("install", "--automatic");
            AssertSetupSuccess(installed, "install");
            workspace.ExpectFiles(TeamEntrypointPath);
            workspace.WriteText(TeamEntrypointPath, TeamEntrypointBody);
            Assert.Equal(
                Encoding.UTF8.GetBytes(TeamEntrypointBody),
                File.ReadAllBytes(workspace.Combine(TeamEntrypointPath)));
            var attached = await workspace.RunAsync(
                "library", "attach", LibraryId, SourceRoot, "--to", DestinationRoot, "--automatic");
            AssertSetupSuccess(attached, "library attach");

            AssertRelativeLink(workspace, $"{SourceRoot}/{OnePath}", DestinationPath(OnePath));
            AssertRelativeLink(workspace, $"{SourceRoot}/{TwoPath}", DestinationPath(TwoPath));
            Assert.Equal([OnePath, TwoPath], ReadLibraryPaths(workspace));
            AssertSourceBytes(workspace, OneSourceBody, TwoSourceBody);
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

    private static string DestinationPath(string sourcePath)
        => $"{DestinationRoot}/{sourcePath}";

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

    private static void AssertOrdinaryFileWithBytes(string path, byte[] expected)
    {
        var attributes = File.GetAttributes(path);
        Assert.False((attributes & FileAttributes.ReparsePoint) != 0);
        Assert.Equal(expected, File.ReadAllBytes(path));
    }

    private static void AssertSourceBytes(
        PublishedJourneyWorkspace workspace,
        string oneBody,
        string twoBody)
    {
        Assert.Equal(Encoding.UTF8.GetBytes(oneBody), File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{OnePath}")));
        Assert.Equal(Encoding.UTF8.GetBytes(twoBody), File.ReadAllBytes(workspace.Combine($"{SourceRoot}/{TwoPath}")));
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

    private static void AssertRestorationReported(string output, string destinationPath)
    {
        AssertOutputContains(output, destinationPath);
        AssertOutputContainsAny(output, "restored", "restoration", "restore");
    }

    private static void AssertPartialChangedResult(string output)
    {
        AssertOutputContainsAny(output, "incomplete", "partial", "could not");
        Assert.DoesNotContain("up to date", output, StringComparison.OrdinalIgnoreCase);
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
