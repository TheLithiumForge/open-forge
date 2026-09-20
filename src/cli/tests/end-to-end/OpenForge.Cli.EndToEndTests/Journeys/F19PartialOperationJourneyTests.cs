using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F19PartialOperationJourneyTests
{
    private const string Feature = "partial-operation-journey";
    private const string SettingsPath = ".agents/open-forge.json";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string AlphaPath = ".agents/guidance/alpha/_alpha.md";
    private const string AlphaChildPath = ".agents/guidance/alpha/seed.md";
    private const string AlphaNewChildPath = ".agents/guidance/alpha/new.md";
    private const string BetaPath = ".agents/guidance/beta/_beta.md";
    private const string BetaChildPath = ".agents/guidance/beta/seed.md";
    private const string BetaNewChildPath = ".agents/guidance/beta/new.md";
    private const string AlphaNewEntry = "- [Alpha New](new.md) - #Alpha";
    private const string BetaNewEntry = "- [Beta New](new.md) - #Beta";

    [Fact(DisplayName = "F19 X09 C02-05 X16 C07-03 partial failure is diagnosed, manually continued, and dry-run cleaned")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F19")]
    public async Task PartialFailureIsDiagnosedManuallyContinuedAndRetainedEvidenceIsPreviewed()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F19 requires the Windows file-sharing capability used by the deterministic replacement failure.");
        }

        using var workspace = PublishedJourneyWorkspace.Create("e2e-f19-partial-operation-journey");
        workspace.ExpectCoreInstall();
        workspace.WriteText(SettingsPath, "{\"allowInstallPaths\":[]}\n");

        var install = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(install, "install");

        SeedCategory(workspace, "alpha", "Alpha", "Alpha Seed", "Alpha", AlphaChildPath);
        SeedCategory(workspace, "beta", "Beta", "Beta Seed", "Beta", BetaChildPath);

        var initialIndex = await workspace.RunAsync("index", AlphaPath, BetaPath);
        AssertCompleted(initialIndex, "initial index");
        Assert.NotEmpty(initialIndex.StandardOutput);
        Assert.Contains(
            "seed.md",
            File.ReadAllText(workspace.Combine(AlphaPath)),
            StringComparison.Ordinal);
        Assert.Contains(
            "seed.md",
            File.ReadAllText(workspace.Combine(BetaPath)),
            StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        workspace.WriteText(
            AlphaNewChildPath,
            OpenForgeDocumentSeed.Metadata(
                description: "Alpha New",
                tags: ["Alpha"],
                body: "\n# Alpha New\n\nA newly authored Alpha child.\n"));
        workspace.WriteText(
            BetaNewChildPath,
            OpenForgeDocumentSeed.Metadata(
                description: "Beta New",
                tags: ["Beta"],
                body: "\n# Beta New\n\nA newly authored Beta child.\n"));

        var alphaBefore = File.ReadAllBytes(workspace.Combine(AlphaPath));
        var betaBefore = File.ReadAllBytes(workspace.Combine(BetaPath));
        var alphaChildBefore = File.ReadAllBytes(workspace.Combine(AlphaChildPath));
        var betaChildBefore = File.ReadAllBytes(workspace.Combine(BetaChildPath));
        var alphaNewChildBefore = File.ReadAllBytes(workspace.Combine(AlphaNewChildPath));
        var betaNewChildBefore = File.ReadAllBytes(workspace.Combine(BetaNewChildPath));
        var settingsBefore = File.ReadAllBytes(workspace.Combine(SettingsPath));
        var ownershipBefore = File.ReadAllBytes(workspace.Combine(OwnershipPath));
        var preState = workspace.SnapshotState();
        var preRecovery = CaptureRecoveryInventory(workspace);

        RecoveryEvidence failureEvidence;
        byte[] alphaAfterFailure;
        using (var heldBeta = new FileStream(
                   workspace.Combine(BetaPath),
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.Read))
        {
            var failed = await workspace.RunAsync("index", AlphaPath, BetaPath);

            Assert.Equal(1, failed.ExitCode);
            Assert.Empty(failed.StandardOutput);
            AssertHumanPrimaryError(failed.StandardError, "partial index failure");

            alphaAfterFailure = File.ReadAllBytes(workspace.Combine(AlphaPath));
            Assert.False(alphaBefore.SequenceEqual(alphaAfterFailure));
            Assert.Contains(
                AlphaNewEntry,
                File.ReadAllText(workspace.Combine(AlphaPath)),
                StringComparison.Ordinal);
            Assert.Equal(betaBefore, File.ReadAllBytes(workspace.Combine(BetaPath)));
            Assert.Equal(alphaChildBefore, File.ReadAllBytes(workspace.Combine(AlphaChildPath)));
            Assert.Equal(betaChildBefore, File.ReadAllBytes(workspace.Combine(BetaChildPath)));
            Assert.Equal(alphaNewChildBefore, File.ReadAllBytes(workspace.Combine(AlphaNewChildPath)));
            Assert.Equal(betaNewChildBefore, File.ReadAllBytes(workspace.Combine(BetaNewChildPath)));
            Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Combine(SettingsPath)));
            Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
            var postState = workspace.SnapshotState();
            PublishedJourneyAssertions.AssertOnlyFileMutations(preState, postState, AlphaPath, BetaPath);
            Assert.NotEqual(preState[AlphaPath], postState[AlphaPath]);
            Assert.Equal(preState[BetaPath], postState[BetaPath]);

            failureEvidence = InspectRetainedRecovery(workspace, preRecovery);
        }

        var doctorStateBefore = workspace.SnapshotState();
        var doctorRecoveryBefore = CaptureRecoveryInventory(workspace);
        var humanDoctor = await workspace.RunAsync("doctor");
        Assert.Equal(2, humanDoctor.ExitCode);
        Assert.Empty(humanDoctor.StandardError);
        Assert.NotEmpty(humanDoctor.StandardOutput);
        var doctor = await workspace.RunAsync("doctor", "--format=json", "--detail=full");
        Assert.Equal(2, doctor.ExitCode);
        Assert.Empty(doctor.StandardError);
        Assert.NotEmpty(doctor.StandardOutput);
        using (var doctorDocument = JsonDocument.Parse(doctor.StandardOutput))
        {
            Assert.Equal("completed-with-warnings", doctorDocument.RootElement.GetProperty("status").GetString());
            var routeFindings = Findings(doctorDocument, "Routes and Entries");
            Assert.Contains(
                routeFindings,
                finding => finding.GetProperty("code").GetString() == "route.generated-region-stale");
            var recoveryFindings = Findings(doctorDocument, "Recovery data");
            Assert.NotEmpty(recoveryFindings);
            Assert.Contains(
                recoveryFindings,
                finding => (finding.GetProperty("code").GetString() ?? string.Empty)
                    .Contains("recovery", StringComparison.OrdinalIgnoreCase));
        }
        AssertSnapshotEqual(doctorStateBefore, workspace.SnapshotState());
        AssertRecoveryInventoryEqual(doctorRecoveryBefore, CaptureRecoveryInventory(workspace));
        Assert.Equal(alphaAfterFailure, File.ReadAllBytes(workspace.Combine(AlphaPath)));
        Assert.Equal(betaBefore, File.ReadAllBytes(workspace.Combine(BetaPath)));

        var currentAlpha = File.ReadAllBytes(workspace.Combine(AlphaPath));
        var currentBeta = File.ReadAllBytes(workspace.Combine(BetaPath));
        Assert.Equal(alphaAfterFailure, currentAlpha);
        Assert.Equal(betaBefore, currentBeta);
        File.WriteAllBytes(workspace.Combine(AlphaPath), alphaBefore);
        Assert.Equal(alphaBefore, File.ReadAllBytes(workspace.Combine(AlphaPath)));
        Assert.Equal(failureEvidence.Sha256, HashFile(failureEvidence.Path));

        var retry = await workspace.RunAsync("index", AlphaPath, BetaPath);
        AssertCompleted(retry, "manual continuation index");
        Assert.NotEmpty(retry.StandardOutput);
        Assert.Contains(
            AlphaNewEntry,
            File.ReadAllText(workspace.Combine(AlphaPath)),
            StringComparison.Ordinal);
        Assert.Contains(
            BetaNewEntry,
            File.ReadAllText(workspace.Combine(BetaPath)),
            StringComparison.Ordinal);
        Assert.Equal(alphaChildBefore, File.ReadAllBytes(workspace.Combine(AlphaChildPath)));
        Assert.Equal(betaChildBefore, File.ReadAllBytes(workspace.Combine(BetaChildPath)));
        Assert.Equal(alphaNewChildBefore, File.ReadAllBytes(workspace.Combine(AlphaNewChildPath)));
        Assert.Equal(betaNewChildBefore, File.ReadAllBytes(workspace.Combine(BetaNewChildPath)));
        Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Combine(SettingsPath)));
        Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        Assert.True(File.Exists(failureEvidence.Path));
        Assert.Equal(failureEvidence.Sha256, HashFile(failureEvidence.Path));

        var beforeCleanupSource = workspace.SnapshotState();
        var beforeCleanupRecovery = CaptureRecoveryInventory(workspace);
        var humanCleanup = await workspace.RunAsync("cleanup", "--dry-run");
        AssertCompleted(humanCleanup, "retained recovery preview");
        Assert.NotEmpty(humanCleanup.StandardOutput);
        var cleanup = await workspace.RunAsync("cleanup", "--dry-run", "--format=json");
        Assert.Equal(0, cleanup.ExitCode);
        Assert.Empty(cleanup.StandardError);
        using (var cleanupDocument = JsonDocument.Parse(cleanup.StandardOutput))
        {
            Assert.Equal("completed", cleanupDocument.RootElement.GetProperty("status").GetString());
            var items = cleanupDocument.RootElement
                .GetProperty("data")
                .GetProperty("items")
                .EnumerateArray()
                .ToArray();
            Assert.Contains(
                items,
                item => item.GetProperty("path").GetString() == failureEvidence.Path
                    && item.GetProperty("kind").GetString() == "bundle"
                    && item.GetProperty("outcome").GetString() == "would-be-removed");
        }
        AssertSnapshotEqual(beforeCleanupSource, workspace.SnapshotState());
        AssertRecoveryInventoryEqual(beforeCleanupRecovery, CaptureRecoveryInventory(workspace));
        Assert.Equal(alphaNewChildBefore, File.ReadAllBytes(workspace.Combine(AlphaNewChildPath)));
        Assert.Equal(betaNewChildBefore, File.ReadAllBytes(workspace.Combine(BetaNewChildPath)));
        Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Combine(SettingsPath)));
        Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        Assert.Equal(failureEvidence.Sha256, HashFile(failureEvidence.Path));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    private static void SeedCategory(
        PublishedJourneyWorkspace workspace,
        string category,
        string title,
        string childDescription,
        string childTag,
        string childPath)
    {
        var entrypointPath = $".agents/guidance/{category}/_{category}.md";
        var entry = $"- [{childDescription}]({Path.GetFileName(childPath)}) - #{childTag}";
        var generatedEntries = OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = entry,
            Prefix = $"# {title}",
        });
        workspace.WriteText(
            entrypointPath,
            OpenForgeDocumentSeed.Metadata(
                description: $"F19 {title} entrypoint",
                tags: ["Guidance", childTag],
                body: $"\n{generatedEntries}"));
        workspace.WriteText(
            childPath,
            OpenForgeDocumentSeed.Metadata(
                description: childDescription,
                tags: [childTag],
                body: $"\n# {childDescription}\n\nAn authored {title} child.\n"));
    }

    private static RecoveryEvidence InspectRetainedRecovery(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, string> preRecovery)
    {
        var recoveryDirectory = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        Assert.True(Directory.Exists(recoveryDirectory));
        var zipPaths = Directory.EnumerateFiles(recoveryDirectory, "*.zip", SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var path = Assert.Single(zipPaths);
        var relative = Path.GetFileName(path)!;
        Assert.DoesNotContain(
            preRecovery.Keys,
            key => key.EndsWith(relative, StringComparison.Ordinal));

        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false))
        {
            var manifestEntry = archive.GetEntry("manifest.json");
            Assert.NotNull(manifestEntry);
            using var manifestReader = new StreamReader(manifestEntry!.Open(), Encoding.UTF8);
            using var manifest = JsonDocument.Parse(manifestReader.ReadToEnd());
            var root = manifest.RootElement;
            Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal(workspace.Path, root.GetProperty("workspacePath").GetString());
            Assert.Equal(
                PublishedWorkspaceLockStore.RecoveryWorkspaceKey(workspace.Path),
                root.GetProperty("workspaceKey").GetString());
            Assert.Equal(
                PublishedWorkspaceLockStore.RecoveryWorkspaceKey(workspace.Path),
                root.GetProperty("attribution")
                    .GetProperty("subject")
                    .GetProperty("identity")
                    .GetString());
            var entries = root.GetProperty("entries").EnumerateArray().ToArray();
            Assert.NotEmpty(entries);
            foreach (var entry in entries)
            {
                var payload = entry.GetProperty("priorPayload").GetString();
                Assert.False(string.IsNullOrWhiteSpace(payload));
                Assert.NotNull(archive.GetEntry(payload!));
            }
        }

        return new RecoveryEvidence(path, HashFile(path));
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

    private static JsonElement[] Findings(JsonDocument document, string category)
        => document.RootElement.GetProperty("findings")
            .EnumerateArray()
            .Where(finding => finding.GetProperty("category").GetString() == category)
            .ToArray();

    private static string HashFile(string path)
        => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));

    private static void AssertCompleted(ProcessRunResult result, string operation)
    {
        Assert.True(
            result.ExitCode == 0 && result.StandardError.Length == 0,
            $"{operation} was not completed. Exit: {result.ExitCode}; stdout: {result.StandardOutput}; stderr: {result.StandardError}");
    }

    private static void AssertHumanPrimaryError(string error, string operation)
    {
        Assert.False(string.IsNullOrWhiteSpace(error), $"{operation} did not produce primary stderr.");
        Assert.False(
            error.TrimStart().StartsWith("{", StringComparison.Ordinal),
            $"{operation} primary stderr was not human-readable: {error}");
    }

    private static void AssertSnapshotEqual(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual)
    {
        Assert.Equal(
            expected.OrderBy(pair => pair.Key),
            actual.OrderBy(pair => pair.Key));
    }

    private static void AssertOnlyWorkspaceChange(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        string expectedChangedPath)
    {
        PublishedJourneyAssertions.AssertOnlyFileMutations(before, after, expectedChangedPath);
        Assert.NotEqual(before[expectedChangedPath], after[expectedChangedPath]);
    }

    private static void AssertRecoveryInventoryEqual(
        IReadOnlyDictionary<string, string> expected,
        IReadOnlyDictionary<string, string> actual,
        bool allowAdditionalRetainedEvidence = false)
    {
        if (!allowAdditionalRetainedEvidence)
        {
            AssertSnapshotEqual(expected, actual);
            return;
        }

        foreach (var pair in expected)
        {
            if (pair.Key == "<root>" && pair.Value == "absent")
            {
                continue;
            }

            Assert.True(actual.TryGetValue(pair.Key, out var actualValue));
            Assert.Equal(pair.Value, actualValue);
        }
    }

    private sealed record RecoveryEvidence(string Path, string Sha256);
}
