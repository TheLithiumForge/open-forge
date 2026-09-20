using System.Diagnostics;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F20WorkspaceLockJourneyTests
{
    private const string Feature = "workspace-lock-journey";
    private const string GuidanceEntrypointPath = ".agents/guidance/_guidance.md";
    private const string ANotePath = ".agents/guidance/f20-a.md";
    private const string BNotePath = ".agents/guidance/f20-b.md";
    private const string AChangedNotePath = ".agents/guidance/f20-a-after-block.md";
    private const string MarkerPath = "first-writer.txt";

    [Fact(DisplayName = "F20 X21 C05-09 a live OS writer blocks A, leaves B independent, and permits a real retry")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F20")]
    public async Task LiveWriterBlocksOnlyItsWorkspaceAndReleasePermitsRetry()
    {
        SkipUnlessWindows();
        using var workspaceA = PublishedJourneyWorkspace.Create("e2e-f20-live-lock-a");
        using var workspaceB = PublishedJourneyWorkspace.Create("e2e-f20-live-lock-b");
        await InstallPairAsync(workspaceA, workspaceB);
        SeedNotes(workspaceA, workspaceB);
        var aNoteBefore = File.ReadAllBytes(workspaceA.Combine(ANotePath));
        var bNoteBefore = File.ReadAllBytes(workspaceB.Combine(BNotePath));

        var lockPath = workspaceA.LockStore.Track(workspaceA.Path);
        workspaceA.LockStore.AssertPersistentZeroByteLock(workspaceA.Path);
        var markerPath = ReserveMarker(workspaceA);
        AssertWindowsPowerShellExists();
        using var holder = await PublishedLockHolder.StartAsync(lockPath, markerPath);
        AssertHolderIsAlive(holder);
        AssertExclusiveOpenIsDenied(lockPath);
        Assert.Contains("first-writer", File.ReadAllText(markerPath), StringComparison.Ordinal);

        var blockedWorkspace = workspaceA.SnapshotState();
        var blockedOwnership = File.ReadAllBytes(workspaceA.Combine(".agents/open-forge.lock.json"));
        var blockedRecovery = CaptureRecoveryInventory(workspaceA);
        var blocked = await workspaceA.RunAsync("index");
        AssertBlocked(blocked, "A index while the live writer holds the lock");
        AssertSnapshotEqual(blockedWorkspace, workspaceA.SnapshotState());
        Assert.Equal(blockedOwnership, File.ReadAllBytes(workspaceA.Combine(".agents/open-forge.lock.json")));
        AssertRecoveryInventoryEqual(blockedRecovery, CaptureRecoveryInventory(workspaceA));

        var bIndex = await RunOnADataHomeAsync(workspaceA, workspaceB, "index");
        AssertCompleted(bIndex, "B index with A's data home");
        Assert.NotEmpty(bIndex.StandardOutput);
        Assert.Contains(
            Path.GetFileName(BNotePath),
            File.ReadAllText(workspaceB.Combine(GuidanceEntrypointPath)),
            StringComparison.Ordinal);

        await holder.ReleaseAsync();
        Assert.True(File.Exists(lockPath));
        Assert.Equal(0, new FileInfo(lockPath).Length);

        var retry = await workspaceA.RunAsync("index");
        AssertCompleted(retry, "A retry after release");
        Assert.NotEmpty(retry.StandardOutput);
        Assert.Contains(
            Path.GetFileName(ANotePath),
            File.ReadAllText(workspaceA.Combine(GuidanceEntrypointPath)),
            StringComparison.Ordinal);
        Assert.Contains(
            "first-writer",
            File.ReadAllText(markerPath),
            StringComparison.Ordinal);
        Assert.Equal(aNoteBefore, File.ReadAllBytes(workspaceA.Combine(ANotePath)));
        Assert.Equal(bNoteBefore, File.ReadAllBytes(workspaceB.Combine(BNotePath)));
        workspaceA.LockStore.AssertPersistentZeroByteLock(workspaceA.Path);
        workspaceA.LockStore.AssertNoRecoveryArtifacts(workspaceA.Path);
    }

    [Fact(DisplayName = "F20 X09 a crashed live writer leaves the persistent pathname and a fresh retry succeeds")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F20")]
    public async Task CrashedWriterLeavesPersistentPathnameForFreshRetry()
    {
        SkipUnlessWindows();
        using var workspaceA = PublishedJourneyWorkspace.Create("e2e-f20-crash-a");
        using var workspaceB = PublishedJourneyWorkspace.Create("e2e-f20-crash-b");
        await InstallPairAsync(workspaceA, workspaceB);
        SeedNotes(workspaceA, workspaceB);
        var aNoteBefore = File.ReadAllBytes(workspaceA.Combine(ANotePath));
        var bNoteBefore = File.ReadAllBytes(workspaceB.Combine(BNotePath));

        var lockPath = workspaceA.LockStore.Track(workspaceA.Path);
        var markerPath = ReserveMarker(workspaceA);
        AssertWindowsPowerShellExists();
        using var holder = await PublishedLockHolder.StartAsync(lockPath, markerPath);
        AssertHolderIsAlive(holder);

        var blocked = await workspaceA.RunAsync("index");
        AssertBlocked(blocked, "A index before the holder crash");
        await holder.CrashAsync();

        Assert.True(File.Exists(lockPath));
        Assert.Equal(0, new FileInfo(lockPath).Length);
        Assert.Contains("first-writer", File.ReadAllText(markerPath), StringComparison.Ordinal);

        var retry = await workspaceA.RunAsync("index");
        AssertCompleted(retry, "A retry after holder crash");
        Assert.NotEmpty(retry.StandardOutput);
        Assert.Contains(
            Path.GetFileName(ANotePath),
            File.ReadAllText(workspaceA.Combine(GuidanceEntrypointPath)),
            StringComparison.Ordinal);
        Assert.Equal(aNoteBefore, File.ReadAllBytes(workspaceA.Combine(ANotePath)));
        Assert.Equal(bNoteBefore, File.ReadAllBytes(workspaceB.Combine(BNotePath)));
        workspaceA.LockStore.AssertPersistentZeroByteLock(workspaceA.Path);
        workspaceA.LockStore.AssertNoRecoveryArtifacts(workspaceA.Path);
    }

    [Fact(DisplayName = "F20 X09 a changed A input is revalidated after live contention before retry projection")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F20")]
    public async Task ChangedInputIsRevalidatedAfterContention()
    {
        SkipUnlessWindows();
        using var workspaceA = PublishedJourneyWorkspace.Create("e2e-f20-changed-a");
        using var workspaceB = PublishedJourneyWorkspace.Create("e2e-f20-changed-b");
        await InstallPairAsync(workspaceA, workspaceB);
        SeedNotes(workspaceA, workspaceB);
        var aNoteBefore = File.ReadAllBytes(workspaceA.Combine(ANotePath));
        var bNoteBefore = File.ReadAllBytes(workspaceB.Combine(BNotePath));

        var lockPath = workspaceA.LockStore.Track(workspaceA.Path);
        var markerPath = ReserveMarker(workspaceA);
        AssertWindowsPowerShellExists();
        using var holder = await PublishedLockHolder.StartAsync(lockPath, markerPath);
        AssertHolderIsAlive(holder);

        var blocked = await workspaceA.RunAsync("index");
        AssertBlocked(blocked, "A index before changed-input retry");
        workspaceA.WriteText(
            AChangedNotePath,
            OpenForgeDocumentSeed.Metadata(
                description: "F20 A note after contention",
                tags: ["Guidance", "F20"],
                body: "\n# F20 A Note After Contention\n\nThis note was authored after the blocked call.\n"));
        var changedNoteBytes = File.ReadAllBytes(workspaceA.Combine(AChangedNotePath));
        AssertHolderIsAlive(holder);

        await holder.ReleaseAsync();
        Assert.True(File.Exists(lockPath));
        Assert.Equal(0, new FileInfo(lockPath).Length);

        var retry = await workspaceA.RunAsync("index");
        AssertCompleted(retry, "A retry with changed input");
        Assert.NotEmpty(retry.StandardOutput);
        Assert.Contains(
            Path.GetFileName(AChangedNotePath),
            File.ReadAllText(workspaceA.Combine(GuidanceEntrypointPath)),
            StringComparison.Ordinal);
        Assert.Contains("first-writer", File.ReadAllText(markerPath), StringComparison.Ordinal);
        Assert.Equal(aNoteBefore, File.ReadAllBytes(workspaceA.Combine(ANotePath)));
        Assert.Equal(bNoteBefore, File.ReadAllBytes(workspaceB.Combine(BNotePath)));
        Assert.Equal(changedNoteBytes, File.ReadAllBytes(workspaceA.Combine(AChangedNotePath)));
        workspaceA.LockStore.AssertPersistentZeroByteLock(workspaceA.Path);
        workspaceA.LockStore.AssertNoRecoveryArtifacts(workspaceA.Path);
    }

    private static async Task InstallPairAsync(
        PublishedJourneyWorkspace workspaceA,
        PublishedJourneyWorkspace workspaceB)
    {
        workspaceA.ExpectCoreInstall();
        workspaceB.ExpectCoreInstall();
        _ = workspaceA.LockStore.Track(workspaceB.Path);

        var installA = await workspaceA.RunAsync("install", "--automatic");
        AssertCompleted(installA, "A install");
        var installB = await RunOnADataHomeAsync(workspaceA, workspaceB, "install", "--automatic");
        AssertCompleted(installB, "B install using A's data home");
    }

    private static void SeedNotes(
        PublishedJourneyWorkspace workspaceA,
        PublishedJourneyWorkspace workspaceB)
    {
        workspaceA.WriteText(
            ANotePath,
            OpenForgeDocumentSeed.Metadata(
                description: "F20 A note",
                tags: ["Guidance", "F20"],
                body: "\n# F20 A Note\n\nA valid note for workspace A.\n"));
        workspaceB.WriteText(
            BNotePath,
            OpenForgeDocumentSeed.Metadata(
                description: "F20 B note",
                tags: ["Guidance", "F20"],
                body: "\n# F20 B Note\n\nA valid note for workspace B.\n"));
    }

    private static string ReserveMarker(PublishedJourneyWorkspace workspace)
    {
        workspace.ExpectFiles(MarkerPath);
        return workspace.Combine(MarkerPath);
    }

    private static Task<ProcessRunResult> RunOnADataHomeAsync(
        PublishedJourneyWorkspace dataHomeOwner,
        PublishedJourneyWorkspace targetWorkspace,
        params string[] arguments)
        => PublishedJourneyProcess.RunAsync(
            dataHomeOwner.Target,
            targetWorkspace.Path,
            arguments,
            dataHomeOwner.ProcessEnvironment);

    private static void AssertHolderIsAlive(PublishedLockHolder holder)
    {
        using var process = Process.GetProcessById(holder.ProcessId);
        Assert.False(process.HasExited);
    }

    private static void AssertExclusiveOpenIsDenied(string lockPath)
    {
        var denied = false;
        try
        {
            using var ignored = new FileStream(
                lockPath,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None);
        }
        catch (IOException)
        {
            denied = true;
        }
        catch (UnauthorizedAccessException)
        {
            denied = true;
        }

        Assert.True(denied, "An independent exclusive open unexpectedly bypassed the live writer.");
    }

    private static void AssertCompleted(ProcessRunResult result, string operation)
    {
        Assert.True(
            result.ExitCode == 0 && result.StandardError.Length == 0,
            $"{operation} was not completed. Exit: {result.ExitCode}; stdout: {result.StandardOutput}; stderr: {result.StandardError}");
    }

    private static void AssertBlocked(ProcessRunResult result, string operation)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardError), $"{operation} did not report stderr.");
        Assert.False(
            result.StandardError.TrimStart().StartsWith("{", StringComparison.Ordinal),
            $"{operation} did not report a human-readable blocked diagnostic: {result.StandardError}");
    }

    private static void AssertWindowsPowerShellExists()
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");
        Assert.True(File.Exists(path), $"The OS-provided Windows PowerShell executable is missing: {path}");
    }

    private static IReadOnlyDictionary<string, string> CaptureRecoveryInventory(
        PublishedJourneyWorkspace workspace)
    {
        var recoveryDirectory = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        if (!Directory.Exists(recoveryDirectory))
        {
            return new Dictionary<string, string> { ["<root>"] = "absent" };
        }

        return new SortedDictionary<string, string>(
            PublishedWorkspaceTreeSnapshot.Capture(recoveryDirectory).ToDictionary(pair => pair.Key, pair => pair.Value),
            StringComparer.Ordinal);
    }

    private static void AssertSnapshotEqual(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(
            expected.OrderBy(pair => pair.Key),
            actual.OrderBy(pair => pair.Key));
    }

    private static void AssertRecoveryInventoryEqual(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual)
    {
        AssertSnapshotEqual(expected, actual);
    }

    private static void SkipUnlessWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F20 requires the Windows PowerShell and Windows file-sharing capabilities.");
        }
    }
}
