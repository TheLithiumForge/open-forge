using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F01InstallReadinessJourneyTests
{
    private const string AgentsPath = "AGENTS.md";
    private const string ClaudePath = "CLAUDE.md";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string ReadmePath = "README.md";
    private const string ManagedStart = "<!-- open-forge:start -->";
    private const string ManagedEnd = "<!-- open-forge:end -->";
    private const string OccupiedPath = ".agents/guidance/_guidance.md";
    private const string ExpectedGuidancePayloadSha256 = "47f5fe9f6b5d4bd59c7818664181bac5e912f5ed0eb85ae8c34cbf6b5297b664";
    private const string ExpectedManagedRegion =
        "<!-- open-forge:start -->\n\n"
        + "# Open Forge\n\n"
        + "Open Forge provides the working rules and context for this workspace.\n\n"
        + "Before starting a task, read `.agents/loader.md`.\n"
        + "Use it to select every relevant scope, including nested scopes.\n"
        + "Follow the loaded rules throughout the task.\n"
        + "<!-- open-forge:end -->\n";

    private static readonly IReadOnlyList<string> ExpectedStartupSourcePaths =
    [
        AgentsPath,
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

    [Fact(DisplayName = "F01 carries a fresh install through readiness, context, diagnosis, and a no-op repeat"),
     Trait("Feature", "install-readiness"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F01")]
    public async Task FreshInstallCarriesStateThroughReadinessAndRepeat()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f01-main");
        const string readme = "# F01 workspace\n\nKeep this authored file byte-for-byte.\n";
        workspace.WriteText(ReadmePath, readme);
        workspace.ExpectCoreInstall();
        var readmeBytes = File.ReadAllBytes(workspace.Combine(ReadmePath));

        var notInstalledBefore = workspace.SnapshotState();
        var notInstalledExternalBefore = SnapshotExternalStore(workspace);
        var notInstalled = await workspace.RunAsync("status");
        AssertCompleted(notInstalled);
        Assert.Contains("Open Forge is not installed", notInstalled.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(notInstalledBefore, workspace.SnapshotState());
        Assert.Equal(notInstalledExternalBefore, SnapshotExternalStore(workspace));
        workspace.LockStore.AssertNoInfrastructure();

        var dryRunBefore = workspace.SnapshotState();
        var dryRunExternalBefore = SnapshotExternalStore(workspace);
        var dryRun = await workspace.RunAsync("install", "--dry-run");
        AssertCompleted(dryRun);
        Assert.Contains("Would install the Open Forge Framework into", dryRun.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            $"Would create {ExpectedPayloadPaths().Count} files and {ExpectedPayloadDirectories().Count} directories under .agents, plus AGENTS.md and CLAUDE.md.",
            dryRun.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains(OwnershipPath, dryRun.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", dryRun.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(dryRunBefore, workspace.SnapshotState());
        Assert.Equal(dryRunExternalBefore, SnapshotExternalStore(workspace));
        workspace.LockStore.AssertNoInfrastructure();

        var beforeInstall = CaptureEntryKinds(workspace.Path);
        var installed = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(installed);
        Assert.Contains("Installed the Open Forge Framework into", installed.StandardOutput, StringComparison.Ordinal);

        var afterInstall = CaptureEntryKinds(workspace.Path);
        var createdPaths = afterInstall.Keys
            .Except(beforeInstall.Keys, StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var createdFiles = createdPaths
            .Where(path => afterInstall[path] == "file")
            .ToArray();
        var createdDirectories = createdPaths
            .Where(path => afterInstall[path] == "directory")
            .ToArray();
        var installedPayload = createdFiles
            .Where(path => path.StartsWith(".agents/", StringComparison.Ordinal)
                && !string.Equals(path, OwnershipPath, StringComparison.Ordinal))
            .ToArray();
        var createdDirectoriesBelowAgents = createdDirectories
            .Where(path => path.StartsWith(".agents/", StringComparison.Ordinal))
            .ToArray();
        var createdHosts = createdFiles
            .Where(path => path is AgentsPath or ClaudePath)
            .ToArray();
        var createdControlFiles = createdFiles
            .Where(path => string.Equals(path, OwnershipPath, StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(ExpectedPayloadPaths(), installedPayload);
        Assert.Equal([OwnershipPath], createdControlFiles);
        Assert.Contains(".agents", createdDirectories, StringComparer.Ordinal);
        Assert.Equal(ExpectedPayloadDirectories(), createdDirectoriesBelowAgents);
        Assert.Equal([AgentsPath, ClaudePath], createdHosts);
        Assert.Empty(
            createdFiles
                .Except(ExpectedPayloadPaths().Append(OwnershipPath).Append(AgentsPath).Append(ClaudePath),
                    StringComparer.Ordinal));
        Assert.Contains(
            $"Created {installedPayload.Length} files and {createdDirectoriesBelowAgents.Length} directories under .agents",
            installed.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains(OwnershipPath, installed.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Created AGENTS.md and CLAUDE.md with an Open Forge section.", installed.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(readmeBytes, File.ReadAllBytes(workspace.Combine(ReadmePath)));
        Assert.Equal(ExpectedPayloadPaths(), ReadOwnershipPaths(workspace.Combine(OwnershipPath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        var readyBefore = workspace.SnapshotState();
        var readyExternalBefore = SnapshotExternalStore(workspace);
        var ready = await workspace.RunAsync("status");
        AssertCompleted(ready);
        Assert.Contains("Open Forge is installed and current.", ready.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Startup reads", ready.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("routed files", ready.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(readyBefore, workspace.SnapshotState());
        Assert.Equal(readyExternalBefore, SnapshotExternalStore(workspace));

        var contextBefore = workspace.SnapshotState();
        var contextExternalBefore = SnapshotExternalStore(workspace);
        var context = await workspace.RunAsync("context");
        AssertCompleted(context);
        Assert.Contains("=== AGENTS.md ===", context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("=== .agents/loader.md", context.StandardOutput, StringComparison.Ordinal);

        var contextJson = await workspace.RunAsync("context", "--format=json", "--detail=full");
        AssertCompleted(contextJson);
        using (var contextDocument = JsonDocument.Parse(contextJson.StandardOutput))
        {
            var root = contextDocument.RootElement;
            Assert.Equal("context", root.GetProperty("command").GetString());
            Assert.Equal("completed", root.GetProperty("status").GetString());
            var sources = root.GetProperty("data").GetProperty("sources").EnumerateArray().ToArray();
            Assert.Equal(
                ExpectedStartupSourcePaths,
                sources.Select(source => source.GetProperty("path").GetString()));

            var markerPositions = sources
                .Select(source => $"=== {source.GetProperty("path").GetString()}")
                .Select(marker => context.StandardOutput.IndexOf(marker, StringComparison.Ordinal))
                .ToArray();
            Assert.All(markerPositions, position => Assert.True(position >= 0));
            Assert.True(markerPositions.SequenceEqual(markerPositions.OrderBy(position => position)));

            foreach (var source in sources)
            {
                var relativePath = source.GetProperty("path").GetString();
                Assert.NotNull(relativePath);
                Assert.True(File.Exists(workspace.Combine(relativePath!)));
                var bodyParts = source.GetProperty("parts")
                    .EnumerateArray()
                    .Where(part => part.GetProperty("part").GetString() == "body")
                    .ToArray();
                var bodyPart = Assert.Single(bodyParts);
                Assert.Equal(
                    ReadPhysicalBody(workspace.Combine(relativePath!)),
                    bodyPart.GetProperty("text").GetString());
            }
        }

        AssertSourceBodyAppears(context.StandardOutput, workspace.Combine(AgentsPath));
        AssertSourceBodyAppears(context.StandardOutput, workspace.Combine(".agents/loader.md"));
        Assert.Equal(contextBefore, workspace.SnapshotState());
        Assert.Equal(contextExternalBefore, SnapshotExternalStore(workspace));

        var doctorBefore = workspace.SnapshotState();
        var doctorExternalBefore = SnapshotExternalStore(workspace);
        var doctor = await workspace.RunAsync("doctor");
        AssertCompleted(doctor);
        Assert.Contains("No problems found.", doctor.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("No files were changed.", doctor.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(doctorBefore, workspace.SnapshotState());
        Assert.Equal(doctorExternalBefore, SnapshotExternalStore(workspace));

        var repeatBefore = workspace.SnapshotState();
        var repeatExternalBefore = SnapshotExternalStore(workspace);
        var repeat = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(repeat);
        Assert.Contains("Open Forge is already installed and current. Nothing to do.", repeat.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(repeatBefore, workspace.SnapshotState());
        Assert.Equal(repeatExternalBefore, SnapshotExternalStore(workspace));
        Assert.Equal(readmeBytes, File.ReadAllBytes(workspace.Combine(ReadmePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F01 preserves authored AGENTS content while adding only the bounded managed section"),
     Trait("Feature", "install-readiness"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F01")]
    public async Task ExistingAgentsContentIsPreserved()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f01-existing-agents");
        const string readme = "# Existing host workspace\n";
        const string authoredAgents = "# Authored instructions\n\nKeep this paragraph exactly.\n";
        workspace.WriteText(ReadmePath, readme);
        workspace.WriteText(AgentsPath, authoredAgents);
        workspace.ExpectCoreInstall();
        var before = CaptureEntryKinds(workspace.Path);
        var authoredBytes = File.ReadAllBytes(workspace.Combine(AgentsPath));

        var result = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(result);
        Assert.Contains("AGENTS.md", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Open Forge section added; your content was kept", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Created AGENTS.md", result.StandardOutput, StringComparison.Ordinal);

        var after = CaptureEntryKinds(workspace.Path);
        var createdFiles = after.Keys
            .Except(before.Keys, StringComparer.Ordinal)
            .Where(path => after[path] == "file")
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.DoesNotContain(AgentsPath, createdFiles, StringComparer.Ordinal);
        Assert.Contains(ClaudePath, createdFiles, StringComparer.Ordinal);
        Assert.Contains(
            $"Created {ExpectedPayloadPaths().Count} files and {ExpectedPayloadDirectories().Count} directories under .agents",
            result.StandardOutput,
            StringComparison.Ordinal);

        var agents = File.ReadAllText(workspace.Combine(AgentsPath));
        Assert.StartsWith(authoredAgents, agents, StringComparison.Ordinal);
        Assert.Equal(1, Count(agents, ManagedStart));
        Assert.Equal(1, Count(agents, ManagedEnd));
        var managedStart = agents.IndexOf(ManagedStart, StringComparison.Ordinal);
        var managedEnd = agents.IndexOf(ManagedEnd, StringComparison.Ordinal);
        var managedEndExclusive = managedEnd + ManagedEnd.Length;
        Assert.Equal(ExpectedManagedRegion, agents[managedStart..(managedEndExclusive + 1)]);
        Assert.Equal(authoredAgents + "\n", agents[..managedStart]);
        Assert.Equal(string.Empty, agents[(managedEndExclusive + 1)..]);
        Assert.Equal(authoredBytes, Encoding.UTF8.GetBytes(agents[..authoredAgents.Length]));
        Assert.Equal(authoredAgents + "\n" + ExpectedManagedRegion, agents);
        Assert.Equal(readme, File.ReadAllText(workspace.Combine(ReadmePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F01 blocks an eligible occupied target, previews force, then replaces only that target"),
     Trait("Feature", "install-readiness"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F01")]
    public async Task OccupiedTargetRequiresDeliberateForce()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f01-occupied");
        const string readme = "# Occupied target workspace\n";
        const string unrelated = "Do not replace this unrelated authored file.\n";
        const string occupant = """
            ---
            open-forge:
              description: Distinctive occupied guidance
              tags: [Guidance]
            ---
            # Distinctive occupied guidance

            ## Entries

            - [Distinctive occupied entry](occupied-entry.md) - #Guidance
            """;
        workspace.WriteText(ReadmePath, readme);
        workspace.WriteText("notes/unrelated.txt", unrelated);
        workspace.WriteText(OccupiedPath, occupant);
        workspace.ExpectCoreInstall();
        var readmeBytes = File.ReadAllBytes(workspace.Combine(ReadmePath));
        var unrelatedBytes = File.ReadAllBytes(workspace.Combine("notes/unrelated.txt"));
        var beforeBlocked = workspace.SnapshotState();
        var externalBeforeBlocked = SnapshotExternalStore(workspace);

        var blocked = await workspace.RunAsync("install", "--automatic");
        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(string.Empty, blocked.StandardOutput);
        Assert.NotEqual(string.Empty, blocked.StandardError);
        Assert.Contains(OccupiedPath, blocked.StandardError, StringComparison.Ordinal);
        Assert.Contains("open-forge install --force --dry-run", blocked.StandardError, StringComparison.Ordinal);
        Assert.Equal(beforeBlocked, workspace.SnapshotState());
        Assert.Equal(externalBeforeBlocked, SnapshotExternalStore(workspace));
        workspace.LockStore.AssertNoInfrastructure();

        var beforePreview = workspace.SnapshotState();
        var externalBeforePreview = SnapshotExternalStore(workspace);
        var preview = await workspace.RunAsync("install", "--force", "--dry-run");
        AssertCompleted(preview);
        Assert.Contains("Would install the Open Forge Framework into", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("replacing 1 existing file", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(OccupiedPath, preview.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(beforePreview, workspace.SnapshotState());
        Assert.Equal(externalBeforePreview, SnapshotExternalStore(workspace));
        workspace.LockStore.AssertNoInfrastructure();

        var beforeForce = CaptureEntryKinds(workspace.Path);
        var forced = await workspace.RunAsync("install", "--force", "--automatic");
        AssertCompleted(forced);
        Assert.Contains("replacing 1 existing file", forced.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(OccupiedPath, forced.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("replaced", forced.StandardOutput, StringComparison.Ordinal);

        var afterForce = CaptureEntryKinds(workspace.Path);
        var createdFiles = afterForce.Keys
            .Except(beforeForce.Keys, StringComparer.Ordinal)
            .Where(path => afterForce[path] == "file")
            .Order(StringComparer.Ordinal)
            .ToArray();
        var createdPayload = createdFiles
            .Where(path => path.StartsWith(".agents/", StringComparison.Ordinal)
                && !string.Equals(path, OwnershipPath, StringComparison.Ordinal))
            .ToArray();
        var createdDirectories = afterForce.Keys
            .Except(beforeForce.Keys, StringComparer.Ordinal)
            .Where(path => afterForce[path] == "directory"
                && path.StartsWith(".agents/", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            ExpectedPayloadPaths()
                .Where(path => !string.Equals(path, OccupiedPath, StringComparison.Ordinal)),
            createdPayload);
        Assert.Equal(
            ExpectedPayloadDirectories().Where(path => !beforeForce.ContainsKey(path)),
            createdDirectories);
        Assert.Contains(
            $"Created {createdPayload.Length} files and {createdDirectories.Length} directories under .agents",
            forced.StandardOutput,
            StringComparison.Ordinal);
        var occupiedPath = workspace.Combine(OccupiedPath);
        var occupiedAttributes = File.GetAttributes(occupiedPath);
        Assert.Equal((FileAttributes)0, occupiedAttributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device));
        Assert.Equal(
            ExpectedGuidancePayloadSha256,
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(occupiedPath))).ToLowerInvariant());
        Assert.NotEqual(occupant, File.ReadAllText(occupiedPath));
        Assert.Equal(readmeBytes, File.ReadAllBytes(workspace.Combine(ReadmePath)));
        Assert.Equal(unrelatedBytes, File.ReadAllBytes(workspace.Combine("notes/unrelated.txt")));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(DisplayName = "F01 reports unavailable confirmation without prompting, then accepts explicit automatic mode"),
     Trait("Feature", "install-readiness"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F01")]
    public async Task RedirectedInstallRequiresExplicitAutomaticMode()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f01-confirmation");
        const string readme = "# Confirmation workspace\n";
        workspace.WriteText(ReadmePath, readme);
        workspace.ExpectCoreInstall();
        var before = workspace.SnapshotState();
        var externalBefore = SnapshotExternalStore(workspace);

        var unavailable = await workspace.RunAsync("install");
        Assert.Equal(4, unavailable.ExitCode);
        Assert.Equal(string.Empty, unavailable.StandardOutput);
        Assert.Contains("Install needs confirmation, and this session cannot ask.", unavailable.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply these changes?", unavailable.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());
        Assert.Equal(externalBefore, SnapshotExternalStore(workspace));
        workspace.LockStore.AssertNoInfrastructure();

        var applied = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(applied);
        Assert.Contains("Installed the Open Forge Framework into", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(readme, File.ReadAllText(workspace.Combine(ReadmePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static void AssertCompleted(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEqual(string.Empty, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static IReadOnlyList<string> ExpectedPayloadPaths()
        => PublishedInstallWorkspace.EmbeddedPayloadPaths
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> ExpectedPayloadDirectories()
        => PublishedInstallWorkspace.EmbeddedPayloadPaths
            .Select(path => System.IO.Path.GetDirectoryName(path))
            .Where(path => path is not null)
            .Select(path => path!.Replace('\\', '/'))
            .Where(path => path.StartsWith(".agents/", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<string> ReadOwnershipPaths(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(path));
        return document.RootElement
            .GetProperty("framework")
            .GetProperty("paths")
            .EnumerateArray()
            .Select(element => element.GetString()!)
            .Order(StringComparer.Ordinal)
            .ToArray();
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

    private static IReadOnlyDictionary<string, string> CaptureEntryKinds(string rootPath)
    {
        var entries = new SortedDictionary<string, string>(StringComparer.Ordinal);
        CaptureEntry(new DirectoryInfo(rootPath), rootPath, ".", entries);
        return entries;
    }

    private static void CaptureEntry(
        FileSystemInfo entry,
        string rootPath,
        string relativePath,
        IDictionary<string, string> entries)
    {
        entry.Refresh();
        var attributes = entry.Attributes;
        var isDirectory = (attributes & FileAttributes.Directory) != 0;
        var isReparsePoint = (attributes & FileAttributes.ReparsePoint) != 0;
        entries[relativePath] = isReparsePoint
            ? isDirectory ? "directory-reparse" : "file-reparse"
            : isDirectory ? "directory" : "file";

        if (!isDirectory || isReparsePoint)
        {
            return;
        }

        foreach (var child in ((DirectoryInfo)entry)
            .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
            .OrderBy(child => RelativePath(rootPath, child.FullName), StringComparer.Ordinal))
        {
            CaptureEntry(child, rootPath, RelativePath(rootPath, child.FullName), entries);
        }
    }

    private static string RelativePath(string rootPath, string path)
        => System.IO.Path.GetRelativePath(rootPath, path).Replace('\\', '/');

    private static int Count(string value, string token)
    {
        var count = 0;
        var start = 0;
        while ((start = value.IndexOf(token, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += token.Length;
        }

        return count;
    }

    private static void AssertSourceBodyAppears(string output, string path)
    {
        Assert.Contains(ReadPhysicalBody(path), output, StringComparison.Ordinal);
    }

    private static string ReadPhysicalBody(string path)
    {
        var source = Encoding.UTF8.GetString(File.ReadAllBytes(path));
        var frontmatterEnd = source.StartsWith("---\n", StringComparison.Ordinal)
            ? source.IndexOf("\n---\n", 4, StringComparison.Ordinal)
            : -1;
        return frontmatterEnd >= 0
            ? source[(frontmatterEnd + "\n---\n".Length)..]
            : source;
    }
}
