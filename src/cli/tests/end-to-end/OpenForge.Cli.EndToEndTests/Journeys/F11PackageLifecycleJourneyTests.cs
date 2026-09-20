using System.IO.Compression;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F11PackageLifecycleJourneyTests
{
    private const string BaseTarget = ".agents/guidance/base.md";
    private const string ToolkitTarget = ".agents/guidance/toolkit.md";
    private const string RetiredTarget = ".agents/guidance/retired.md";
    private const string AddedTarget = ".agents/guidance/added.md";
    private const string SupportTarget = ".agents/guidance/toolkit-support.bin";
    private const string SharedTarget = ".agents/guidance/shared.md";
    private const string OwnershipTarget = ".agents/open-forge.lock.json";

    [Fact(DisplayName = "F11 installs, updates, and removes a dependent package while preserving source and ownership"),
     Trait("Feature", "package lifecycle with dependencies"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F11")]
    public async Task MainPackageLifecycleCarriesDependencyOwnershipAndRecovery()
    {
        using var consumer = PublishedJourneyWorkspace.Create("journey-f11-main-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("journey-f11-main-catalogue");
        WriteLifecycleCatalogue(catalogue, versionTwo: false);

        consumer.ExpectCoreInstall();
        ReserveLifecycleTargets(consumer);
        await InstallFrameworkAsync(consumer);
        consumer.WriteText("unrelated-user.md", "owned by the consumer, not by either package\n");
        var unrelatedBytes = File.ReadAllBytes(consumer.Combine("unrelated-user.md"));
        var sourceOne = catalogue.SnapshotState();

        var list = await RunAsync(consumer, "extension", "list", "--source", catalogue.Path);
        AssertSuccessful(list);
        Assert.Contains("base", list.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("toolkit", list.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertSourceUnchanged(catalogue, sourceOne);

        var inspect = await RunAsync(consumer, "extension", "inspect", "toolkit", "--source", catalogue.Path);
        AssertSuccessful(inspect);
        Assert.Contains("toolkit", inspect.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not installed", inspect.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertSourceUnchanged(catalogue, sourceOne);

        var install = await RunAsync(
            consumer, "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertSuccessful(install);
        Assert.Contains("toolkit", install.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("base", install.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertInstalledLifecycleConsumer(consumer, catalogue, includeRetired: true, version: "1.0.0");
        var priorToolkitBytes = File.ReadAllBytes(consumer.Combine(ToolkitTarget));
        AssertSourceUnchanged(catalogue, sourceOne);

        var beforeBlockedRemoval = consumer.SnapshotState();
        var blocked = await RunAsync(consumer, "extension", "remove", "base", "--automatic");
        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(string.Empty, blocked.StandardOutput);
        Assert.Contains("toolkit", blocked.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeBlockedRemoval, consumer.SnapshotState());
        AssertSourceUnchanged(catalogue, sourceOne);

        var retiredBytes = File.ReadAllBytes(catalogue.Combine($"toolkit/content/{RetiredTarget}"));
        WriteLifecycleCatalogue(catalogue, versionTwo: true);
        File.Delete(catalogue.Combine($"toolkit/content/{RetiredTarget}"));
        var sourceTwo = catalogue.SnapshotState();

        var previewBefore = consumer.SnapshotState();
        var preview = await RunAsync(
            consumer,
            "extension", "update", "toolkit", "--source", catalogue.Path, "--dry-run");
        AssertCompletedWithWarnings(preview);
        Assert.Contains("retired.md", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("toolkit", preview.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(previewBefore, consumer.SnapshotState());
        AssertSourceUnchanged(catalogue, sourceTwo);

        var update = await RunAsync(
            consumer,
            "extension", "update", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertCompletedWithWarnings(update);
        Assert.Contains("toolkit", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2.0.0", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertUpdatedLifecycleConsumer(consumer, retiredBytes, catalogue, keepRetired: true);
        AssertRetainedRecoveryEvidence(consumer, priorToolkitBytes);
        Assert.Equal(unrelatedBytes, File.ReadAllBytes(consumer.Combine("unrelated-user.md")));
        AssertSourceUnchanged(catalogue, sourceTwo);

        var removeToolkit = await RunAsync(consumer, "extension", "remove", "toolkit", "--automatic");
        AssertCompletedWithWarnings(removeToolkit);
        Assert.Contains("toolkit", removeToolkit.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertMissingFile(consumer, ToolkitTarget);
        AssertMissingFile(consumer, RetiredTarget);
        AssertMissingFile(consumer, AddedTarget);
        AssertMissingFile(consumer, SupportTarget);
        AssertFileBytes(consumer, BaseTarget, File.ReadAllBytes(catalogue.Combine($"base/content/{BaseTarget}")));
        AssertExtensionRecord(consumer, "base", "1.0.0", catalogue.Path, [], BaseTarget);
        AssertNoExtensionRecord(consumer, "toolkit");
        AssertSourceUnchanged(catalogue, sourceTwo);

        var removeBase = await RunAsync(consumer, "extension", "remove", "base", "--automatic");
        AssertSuccessful(removeBase);
        Assert.Contains("base", removeBase.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertMissingFile(consumer, BaseTarget);
        AssertNoExtensionRecord(consumer, "base");
        Assert.Equal(unrelatedBytes, File.ReadAllBytes(consumer.Combine("unrelated-user.md")));
        AssertRetainedRecoveryEvidence(consumer, priorToolkitBytes);
        AssertSourceUnchanged(catalogue, sourceTwo);
    }

    [Fact(DisplayName = "F11 ordinary update keeps retired bytes and ownership"),
     Trait("Feature", "package lifecycle with dependencies"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F11")]
    public async Task OrdinaryUpdateKeepsRetiredContent()
    {
        using var consumer = PublishedJourneyWorkspace.Create("journey-f11-retirement-keep-consumer");
        using var versionOne = PublishedJourneyWorkspace.Create("journey-f11-retirement-keep-v1");
        using var versionTwo = PublishedJourneyWorkspace.Create("journey-f11-retirement-keep-v2");
        await SeedLifecycleConsumerAsync(consumer, versionOne, versionTwo);
        var priorToolkitBytes = File.ReadAllBytes(consumer.Combine(ToolkitTarget));
        var retiredBytes = File.ReadAllBytes(versionOne.Combine($"toolkit/content/{RetiredTarget}"));
        var sourceBefore = versionTwo.SnapshotState();

        var update = await RunAsync(
            consumer,
            "extension", "update", "toolkit", "--source", versionTwo.Path, "--automatic");

        AssertCompletedWithWarnings(update);
        AssertFileBytes(consumer, RetiredTarget, retiredBytes);
        AssertFileBytes(consumer, AddedTarget, File.ReadAllBytes(versionTwo.Combine($"toolkit/content/{AddedTarget}")));
        AssertExtensionRecord(
            consumer,
            "toolkit",
            "2.0.0",
            versionTwo.Path,
            ["base"],
            ToolkitTarget,
            RetiredTarget,
            AddedTarget,
            SupportTarget);
        AssertExtensionRecord(consumer, "base", "1.0.0", versionTwo.Path, [], BaseTarget);
        AssertSourceUnchanged(versionTwo, sourceBefore);
        AssertRetainedRecoveryEvidence(consumer, priorToolkitBytes);
    }

    [Fact(DisplayName = "F11 prune update removes retired content and ownership"),
     Trait("Feature", "package lifecycle with dependencies"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F11")]
    public async Task PruneUpdateRemovesRetiredContent()
    {
        using var consumer = PublishedJourneyWorkspace.Create("journey-f11-retirement-prune-consumer");
        using var versionOne = PublishedJourneyWorkspace.Create("journey-f11-retirement-prune-v1");
        using var versionTwo = PublishedJourneyWorkspace.Create("journey-f11-retirement-prune-v2");
        await SeedLifecycleConsumerAsync(consumer, versionOne, versionTwo);
        var priorToolkitBytes = File.ReadAllBytes(consumer.Combine(ToolkitTarget));
        var priorRetiredBytes = File.ReadAllBytes(versionOne.Combine($"toolkit/content/{RetiredTarget}"));
        var sourceBefore = versionTwo.SnapshotState();

        var update = await RunAsync(
            consumer,
            "extension", "update", "toolkit", "--source", versionTwo.Path, "--prune", "--automatic");

        AssertSuccessful(update);
        AssertMissingFile(consumer, RetiredTarget);
        AssertFileBytes(consumer, AddedTarget, File.ReadAllBytes(versionTwo.Combine($"toolkit/content/{AddedTarget}")));
        AssertExtensionRecord(
            consumer,
            "toolkit",
            "2.0.0",
            versionTwo.Path,
            ["base"],
            ToolkitTarget,
            AddedTarget,
            SupportTarget);
        AssertExtensionRecord(consumer, "base", "1.0.0", versionTwo.Path, [], BaseTarget);
        AssertSourceUnchanged(versionTwo, sourceBefore);
        AssertRetainedRecoveryEvidence(consumer, priorToolkitBytes, priorRetiredBytes);
    }

    [Fact(DisplayName = "F11 shared ownership preserves a file after removing one owner"),
     Trait("Feature", "package lifecycle with dependencies"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F11")]
    public async Task SharedOwnerRemovalPreservesOtherOwner()
    {
        using var consumer = PublishedJourneyWorkspace.Create("journey-f11-shared-owner-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("journey-f11-shared-owner-catalogue");
        WriteSharedOwnerCatalogue(catalogue);
        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(SharedTarget);
        consumer.WriteText("neighbor.md", "unrelated neighbor\n");
        var sourceBefore = catalogue.SnapshotState();

        await InstallFrameworkAsync(consumer);
        AssertSuccessful(await RunAsync(
            consumer, "extension", "install", "first", "--source", catalogue.Path, "--automatic"));
        AssertSuccessful(await RunAsync(
            consumer, "extension", "install", "second", "--source", catalogue.Path, "--automatic"));
        var sharedBytes = File.ReadAllBytes(catalogue.Combine($"first/content/{SharedTarget}"));
        AssertFileBytes(consumer, SharedTarget, sharedBytes);
        AssertExtensionRecord(consumer, "first", "1.0.0", catalogue.Path, [], SharedTarget);
        AssertExtensionRecord(consumer, "second", "1.0.0", catalogue.Path, [], SharedTarget);

        var removeFirst = await RunAsync(consumer, "extension", "remove", "first", "--automatic");
        AssertSuccessful(removeFirst);
        AssertFileBytes(consumer, SharedTarget, sharedBytes);
        AssertNoExtensionRecord(consumer, "first");
        AssertExtensionRecord(consumer, "second", "1.0.0", catalogue.Path, [], SharedTarget);
        Assert.Equal("unrelated neighbor\n", File.ReadAllText(consumer.Combine("neighbor.md")));
        Assert.Equal(sourceBefore, catalogue.SnapshotState());
        AssertPersistentLifecycleEvidence(consumer);
    }

    [Fact(DisplayName = "F11 source, version, and target comparisons use independent real package copies"),
     Trait("Feature", "package lifecycle with dependencies"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F11")]
    public async Task X30ComparisonsUseIndependentPackageCopies()
    {
        await RunComparisonCaseAsync("source-only", ComparisonMutation.SourceOnly);
        await RunComparisonCaseAsync("version-only", ComparisonMutation.VersionOnly);
        await RunComparisonCaseAsync("target-only", ComparisonMutation.TargetOnly);
    }

    private static async Task RunComparisonCaseAsync(string label, ComparisonMutation mutation)
    {
        using var consumer = PublishedJourneyWorkspace.Create($"journey-f11-x30-{label}-consumer");
        using var source = PublishedJourneyWorkspace.Create($"journey-f11-x30-{label}-source");
        WriteLifecycleCatalogue(source, versionTwo: false);
        consumer.ExpectCoreInstall();
        ReserveLifecycleTargets(consumer);
        consumer.WriteText("neighbor.md", "unrelated comparison neighbor\n");
        await InstallFrameworkAsync(consumer);
        AssertSuccessful(await RunAsync(
            consumer, "extension", "install", "toolkit", "--source", source.Path, "--automatic"));
        switch (mutation)
        {
            case ComparisonMutation.SourceOnly:
                source.WriteText(
                    $"toolkit/content/{ToolkitTarget}",
                    OpenForgeDocumentSeed.Metadata("toolkit source-only revision", ["Extension"], "# Source-only revision\n"));
                break;
            case ComparisonMutation.VersionOnly:
                source.WriteText(
                    "toolkit/extension.json",
                    Manifest("toolkit", "Toolkit", "2.0.0", "base"));
                break;
            case ComparisonMutation.TargetOnly:
                consumer.WriteText(
                    ToolkitTarget,
                    OpenForgeDocumentSeed.Metadata("toolkit target-only revision", ["Extension"], "# Target-only revision\n"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
        }

        var before = consumer.SnapshotState();
        var sourceBefore = source.SnapshotState();

        var status = await RunReadOnlyComparisonAsync(consumer, source, "status");
        AssertComparisonObservation(status, mutation);
        var list = await RunReadOnlyComparisonAsync(consumer, source, "extension", "list", "--source", source.Path);
        AssertComparisonObservation(list, mutation);
        var inspect = await RunReadOnlyComparisonAsync(
            consumer, source, "extension", "inspect", "toolkit", "--source", source.Path);
        AssertComparisonObservation(inspect, mutation);
        var preview = await RunReadOnlyComparisonAsync(
            consumer, source, "extension", "update", "toolkit", "--source", source.Path, "--dry-run");
        AssertSuccessful(preview);
        Assert.Equal(before, consumer.SnapshotState());
        Assert.Equal(sourceBefore, source.SnapshotState());
        Assert.Contains("toolkit", string.Join("\n", status.StandardOutput, list.StandardOutput, inspect.StandardOutput, preview.StandardOutput), StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<ProcessRunResult> RunReadOnlyComparisonAsync(
        PublishedJourneyWorkspace consumer,
        PublishedJourneyWorkspace source,
        params string[] arguments)
    {
        var consumerBefore = consumer.SnapshotState();
        var sourceBefore = source.SnapshotState();
        var result = await RunAsync(consumer, arguments);
        Assert.Equal(consumerBefore, consumer.SnapshotState());
        Assert.Equal(sourceBefore, source.SnapshotState());
        return result;
    }

    private static void AssertComparisonObservation(
        ProcessRunResult result,
        ComparisonMutation mutation)
    {
        Assert.Equal(mutation == ComparisonMutation.VersionOnly ? 0 : 2, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static async Task SeedLifecycleConsumerAsync(
        PublishedJourneyWorkspace consumer,
        PublishedJourneyWorkspace versionOne,
        PublishedJourneyWorkspace versionTwo)
    {
        WriteLifecycleCatalogue(versionOne, versionTwo: false);
        WriteLifecycleCatalogue(versionTwo, versionTwo: true);
        consumer.ExpectCoreInstall();
        ReserveLifecycleTargets(consumer);
        consumer.WriteText("unrelated-user.md", "unrelated user content\n");
        await InstallFrameworkAsync(consumer);
        var install = await RunAsync(
            consumer, "extension", "install", "toolkit", "--source", versionOne.Path, "--automatic");
        AssertSuccessful(install);
        AssertInstalledLifecycleConsumer(consumer, versionOne, includeRetired: true, version: "1.0.0");
    }

    private static async Task<ProcessRunResult> InstallFrameworkAsync(PublishedJourneyWorkspace consumer)
    {
        var result = await RunAsync(consumer, "install", "--automatic");
        AssertSuccessful(result);
        AssertFileExists(consumer, OwnershipTarget);
        return result;
    }

    private static Task<ProcessRunResult> RunAsync(PublishedJourneyWorkspace workspace, params string[] arguments)
        => workspace.RunAsync(arguments);

    private static void WriteLifecycleCatalogue(PublishedJourneyWorkspace catalogue, bool versionTwo)
    {
        if (!versionTwo || !File.Exists(catalogue.Combine("base/extension.json")))
        {
            catalogue.WriteText(
                "base/extension.json",
                Manifest("base", "Base", "1.0.0"));
            catalogue.WriteText(
                $"base/content/{BaseTarget}",
                OpenForgeDocumentSeed.Metadata("base package", ["Extension"], "# Base guidance\nbase-v1\n"));
        }

        catalogue.WriteText(
            "toolkit/extension.json",
            Manifest("toolkit", "Toolkit", versionTwo ? "2.0.0" : "1.0.0", "base"));
        catalogue.WriteText(
            $"toolkit/content/{ToolkitTarget}",
            OpenForgeDocumentSeed.Metadata(
                versionTwo ? "toolkit package v2" : "toolkit package v1",
                ["Extension"],
                versionTwo ? "# Toolkit guidance\ntoolkit-v2\n" : "# Toolkit guidance\ntoolkit-v1\n"));
        if (!versionTwo || !File.Exists(catalogue.Combine($"toolkit/content/{SupportTarget}")))
        {
            catalogue.WriteBytes(
                $"toolkit/content/{SupportTarget}",
                Encoding.UTF8.GetBytes("opaque toolkit support bytes\0\u0001\n"));
        }
        if (versionTwo)
        {
            catalogue.WriteText(
                $"toolkit/content/{AddedTarget}",
                OpenForgeDocumentSeed.Metadata("toolkit added file", ["Extension"], "# Added guidance\n"));
        }
        else
        {
            catalogue.WriteText(
                $"toolkit/content/{RetiredTarget}",
                OpenForgeDocumentSeed.Metadata("toolkit retired file", ["Extension"], "# Retired guidance\nretired-v1\n"));
        }
    }

    private static void WriteSharedOwnerCatalogue(PublishedJourneyWorkspace catalogue)
    {
        var shared = Encoding.UTF8.GetBytes(
            OpenForgeDocumentSeed.Metadata("shared owner", ["Extension"], "shared byte-identical owner content\n"));
        foreach (var id in new[] { "first", "second" })
        {
            catalogue.WriteText(
                $"{id}/extension.json",
                Manifest(id, id, "1.0.0"));
            catalogue.WriteBytes($"{id}/content/{SharedTarget}", shared);
        }
    }

    private static string Manifest(string id, string name, string version, params string[] dependencies)
    {
        var dependencyJson = string.Join(", ", dependencies.Select(dependency => $"\"{dependency}\""));
        return $$"""
                {
                  "id": "{{id}}",
                  "name": "{{name}}",
                  "description": "Journey fixture {{id}}.",
                  "version": "{{version}}",
                  "dependencies": [{{dependencyJson}}]
                }
                """;
    }

    private static void ReserveLifecycleTargets(PublishedJourneyWorkspace consumer)
        => consumer.ExpectFiles(BaseTarget, ToolkitTarget, RetiredTarget, AddedTarget, SupportTarget);

    private static void AssertInstalledLifecycleConsumer(
        PublishedJourneyWorkspace consumer,
        PublishedJourneyWorkspace source,
        bool includeRetired,
        string version)
    {
        AssertFileBytes(consumer, BaseTarget, File.ReadAllBytes(source.Combine($"base/content/{BaseTarget}")));
        AssertFileBytes(consumer, ToolkitTarget, File.ReadAllBytes(source.Combine($"toolkit/content/{ToolkitTarget}")));
        AssertFileBytes(consumer, SupportTarget, File.ReadAllBytes(source.Combine($"toolkit/content/{SupportTarget}")));
        if (includeRetired)
        {
            AssertFileBytes(consumer, RetiredTarget, File.ReadAllBytes(source.Combine($"toolkit/content/{RetiredTarget}")));
        }
        AssertExtensionRecord(consumer, "base", "1.0.0", source.Path, [], BaseTarget);
        AssertExtensionRecord(
            consumer,
            "toolkit",
            version,
            source.Path,
            ["base"],
            includeRetired
                ? new[] { ToolkitTarget, RetiredTarget, SupportTarget }
                : new[] { ToolkitTarget, SupportTarget });
        AssertInitialInstallRecoveryEvidence(consumer);
    }

    private static void AssertUpdatedLifecycleConsumer(
        PublishedJourneyWorkspace consumer,
        byte[] retiredBytes,
        PublishedJourneyWorkspace versionTwo,
        bool keepRetired)
    {
        AssertFileBytes(consumer, BaseTarget, File.ReadAllBytes(versionTwo.Combine($"base/content/{BaseTarget}")));
        AssertFileBytes(consumer, ToolkitTarget, File.ReadAllBytes(versionTwo.Combine($"toolkit/content/{ToolkitTarget}")));
        AssertFileBytes(consumer, AddedTarget, File.ReadAllBytes(versionTwo.Combine($"toolkit/content/{AddedTarget}")));
        AssertFileBytes(consumer, SupportTarget, File.ReadAllBytes(versionTwo.Combine($"toolkit/content/{SupportTarget}")));
        if (keepRetired)
        {
            AssertFileBytes(consumer, RetiredTarget, retiredBytes);
            AssertExtensionRecord(
                consumer,
                "toolkit",
                "2.0.0",
                versionTwo.Path,
                ["base"],
                ToolkitTarget,
                RetiredTarget,
                AddedTarget,
                SupportTarget);
        }
        else
        {
            AssertMissingFile(consumer, RetiredTarget);
            AssertExtensionRecord(
                consumer,
                "toolkit",
                "2.0.0",
                versionTwo.Path,
                ["base"],
                ToolkitTarget,
                AddedTarget,
                SupportTarget);
        }
        AssertExtensionRecord(consumer, "base", "1.0.0", versionTwo.Path, [], BaseTarget);
    }

    private static void AssertExtensionRecord(
        PublishedJourneyWorkspace consumer,
        string id,
        string version,
        string sourcePath,
        IReadOnlyList<string> expectedDependencies,
        params string[] expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(consumer.Combine(OwnershipTarget)));
        var extension = FindExtension(document.RootElement, id);
        Assert.Equal(id, extension.GetProperty("id").GetString());
        Assert.Equal(version, extension.GetProperty("version").GetString());
        Assert.Equal(sourcePath, extension.GetProperty("source").GetString());
        var actualPaths = extension.GetProperty("paths").EnumerateArray()
            .Select(path => Normalize(path.GetString()!))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedPaths.Select(Normalize).OrderBy(path => path, StringComparer.Ordinal).ToArray(),
            actualPaths);
        Assert.Equal(
            expectedDependencies,
            extension.GetProperty("dependencies").EnumerateArray()
                .Select(dependency => Assert.IsType<string>(dependency.GetString()))
                .ToArray());
        Assert.Empty(extension.GetProperty("regions").EnumerateArray());
    }

    private static void AssertNoExtensionRecord(PublishedJourneyWorkspace consumer, string id)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(consumer.Combine(OwnershipTarget)));
        var extensions = document.RootElement.GetProperty("extensions").EnumerateArray();
        Assert.DoesNotContain(extensions, extension => extension.GetProperty("id").GetString() == id);
    }

    private static JsonElement FindExtension(JsonElement root, string id)
        => Assert.Single(
            root.GetProperty("extensions").EnumerateArray(),
            extension => extension.GetProperty("id").GetString() == id);

    private static void AssertPersistentLifecycleEvidence(PublishedJourneyWorkspace workspace)
    {
        AssertFileExists(workspace, OwnershipTarget);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    private static void AssertInitialInstallRecoveryEvidence(PublishedJourneyWorkspace workspace)
    {
        AssertPersistentLifecycleEvidence(workspace);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static void AssertRetainedRecoveryEvidence(
        PublishedJourneyWorkspace workspace,
        byte[] priorToolkitBytes,
        byte[]? priorRetiredBytes = null)
    {
        AssertPersistentLifecycleEvidence(workspace);

        var recoveryDirectory = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        Assert.True(Directory.Exists(recoveryDirectory));
        var artifacts = Directory.EnumerateFileSystemEntries(
                recoveryDirectory,
                "*",
                SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.NotEmpty(artifacts);

        var expectedPreimages = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [ToolkitTarget] = priorToolkitBytes,
        };
        if (priorRetiredBytes is not null)
        {
            expectedPreimages[RetiredTarget] = priorRetiredBytes;
        }

        var matchedPreimages = new HashSet<string>(StringComparer.Ordinal);
        var workspaceKey = PublishedWorkspaceLockStore.RecoveryWorkspaceKey(workspace.Path);
        foreach (var artifact in artifacts)
        {
            var attributes = File.GetAttributes(artifact);
            Assert.True(
                (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0,
                $"Recovery artifact '{artifact}' must be an ordinary file.");
            Assert.True(
                string.Equals(Path.GetExtension(artifact), ".zip", StringComparison.OrdinalIgnoreCase),
                $"Recovery artifact '{artifact}' must be a ZIP file.");

            using var stream = new FileStream(artifact, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
            var manifestEntry = archive.GetEntry("manifest.json");
            Assert.NotNull(manifestEntry);
            using var manifestStream = manifestEntry!.Open();
            using var manifest = JsonDocument.Parse(manifestStream);
            var root = manifest.RootElement;
            Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
            Assert.Equal(workspace.Path, root.GetProperty("workspacePath").GetString());
            Assert.Equal(workspaceKey, root.GetProperty("workspaceKey").GetString());

            var entries = root.GetProperty("entries").EnumerateArray().ToArray();
            Assert.NotEmpty(entries);
            foreach (var entry in entries)
            {
                var logicalPath = entry.GetProperty("logicalPath").GetString();
                Assert.False(string.IsNullOrWhiteSpace(logicalPath));
                var priorPayload = entry.GetProperty("priorPayload").GetString();
                Assert.False(string.IsNullOrWhiteSpace(priorPayload));
                var payloadEntry = archive.GetEntry(priorPayload!);
                Assert.NotNull(payloadEntry);

                if (!expectedPreimages.TryGetValue(logicalPath!, out var expectedBytes))
                {
                    continue;
                }

                using var payloadStream = payloadEntry!.Open();
                using var payload = new MemoryStream();
                payloadStream.CopyTo(payload);
                if (expectedBytes.SequenceEqual(payload.ToArray()))
                {
                    matchedPreimages.Add(logicalPath!);
                }
            }
        }

        Assert.Contains(ToolkitTarget, matchedPreimages);
        if (priorRetiredBytes is not null)
        {
            Assert.Contains(RetiredTarget, matchedPreimages);
        }
    }

    private static void AssertSourceUnchanged(
        PublishedJourneyWorkspace source,
        IReadOnlyDictionary<string, string> expected)
        => Assert.Equal(expected, source.SnapshotState());

    private static void AssertSuccessful(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == 0, $"Expected exit 0, got {result.ExitCode}. Stdout: {result.StandardOutput} Stderr: {result.StandardError}");
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertCompletedWithWarnings(ProcessRunResult result)
    {
        Assert.Equal(2, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertFileExists(PublishedJourneyWorkspace workspace, string relativePath)
    {
        var path = workspace.Combine(relativePath);
        Assert.True(File.Exists(path), $"Expected file '{relativePath}'.");
        Assert.False((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0);
    }

    private static void AssertMissingFile(PublishedJourneyWorkspace workspace, string relativePath)
        => Assert.False(File.Exists(workspace.Combine(relativePath)), $"Did not expect file '{relativePath}'.");

    private static void AssertFileBytes(
        PublishedJourneyWorkspace workspace,
        string relativePath,
        byte[] expected)
    {
        AssertFileExists(workspace, relativePath);
        Assert.Equal(expected, File.ReadAllBytes(workspace.Combine(relativePath)));
    }

    private static string Normalize(string path)
        => path.Replace('\\', '/');

    private enum ComparisonMutation
    {
        SourceOnly,
        VersionOnly,
        TargetOnly,
    }
}
