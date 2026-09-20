using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F13DeliberateReplacementJourneyTests
{
    private const string Feature = "replacement-journey";
    private const string ToolkitTarget = ".agents/guidance/toolkit.md";
    private const string BackupPath = "backups/toolkit.md";
    private const string UnrelatedPath = "unrelated-user.md";
    private const string SettingsPath = ".agents/open-forge.json";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string LibraryId = "protected";
    private const string LibrarySourceRoot = "shared/protected";
    private const string LibrarySourcePath = "shared/protected/source.md";
    private const string LibraryDestinationRoot = ".agents/guidance/protected";
    private const string LibraryDestinationPath = ".agents/guidance/protected/source.md";

    private const string ToolkitV1 = """
        ---
        open-forge:
          description: F13 toolkit fixture
          tags: [Extension]
        ---
        # F13 Toolkit

        Toolkit source bytes version 1.
        """;

    private const string UserBytes = "F13 user bytes at the occupied target.\n";
    private const string BackupBytes = "F13 deliberate backup bytes.\n";
    private const string UnrelatedBytes = "F13 unrelated user bytes.\n";
    private const string OtherOwnerBytes = """
        ---
        open-forge:
          description: F13 other owner fixture
          tags: [Extension]
        ---
        # F13 Other Owner

        Distinct other-owner source bytes.
        """;

    private const string ProtectedLibraryBytes = """
        ---
        open-forge:
          description: F13 protected Library source
          tags: [Guidance]
        ---
        # F13 Protected Source

        Protected Library source bytes.
        """;

    private const string ProtectedToolkitBytes = """
        ---
        open-forge:
          description: F13 blocked external source
          tags: [Extension]
        ---
        # F13 Blocked Source

        External package source bytes.
        """;

    [Fact(DisplayName = "F13 blocks, previews, deliberately replaces, and preserves an occupied target"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F13"),
     Trait("Scenarios", "C21-09,C21-14,C21-10,C21-11,C21-12")]
    public async Task MainReplacementJourneyCarriesStateThroughNoOpAndUserEdit()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f13-replacement-main-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f13-replacement-main-catalogue");
        WritePackage(catalogue, "toolkit", "1.0.0", ToolkitTarget, Encoding.UTF8.GetBytes(ToolkitV1));

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ToolkitTarget, BackupPath, UnrelatedPath);
        await InstallFrameworkAsync(consumer);

        consumer.WriteText(BackupPath, BackupBytes);
        consumer.WriteText(UnrelatedPath, UnrelatedBytes);
        consumer.WriteText(ToolkitTarget, UserBytes);

        var sourceBefore = catalogue.SnapshotState();
        var blockedBefore = consumer.SnapshotState();
        var blocked = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertExistingTargetBlocked(blocked, ToolkitTarget);
        Assert.Equal(blockedBefore, consumer.SnapshotState());
        AssertSourceUnchanged(catalogue, sourceBefore);

        var previewBefore = consumer.SnapshotState();
        var preview = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--force", "--dry-run");
        AssertCompleted(preview);
        Assert.Contains(ToolkitTarget, preview.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(previewBefore, consumer.SnapshotState());
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);

        var applied = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--force", "--automatic");
        AssertCompleted(applied);
        Assert.Contains("toolkit", applied.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("replac", applied.StandardOutput, StringComparison.OrdinalIgnoreCase);
        AssertFileBytes(consumer, ToolkitTarget, File.ReadAllBytes(catalogue.Combine($"toolkit/content/{ToolkitTarget}")));
        AssertFileBytes(consumer, BackupPath, Encoding.UTF8.GetBytes(BackupBytes));
        AssertFileBytes(consumer, UnrelatedPath, Encoding.UTF8.GetBytes(UnrelatedBytes));
        AssertExtensionRecord(consumer, "toolkit", "1.0.0", ToolkitTarget);
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);

        var beforeNoOp = consumer.SnapshotState();
        var noOp = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertCompleted(noOp);
        Assert.Contains("toolkit", noOp.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeNoOp, consumer.SnapshotState());
        AssertExtensionRecord(consumer, "toolkit", "1.0.0", ToolkitTarget);
        AssertSourceUnchanged(catalogue, sourceBefore);

        consumer.WriteText(ToolkitTarget, UserBytes);
        var changedBefore = consumer.SnapshotState();
        var changed = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertBlocked(changed, "toolkit", "update");
        Assert.Contains("update", changed.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(changedBefore, consumer.SnapshotState());
        AssertFileBytes(consumer, ToolkitTarget, Encoding.UTF8.GetBytes(UserBytes));
        AssertExtensionRecord(consumer, "toolkit", "1.0.0", ToolkitTarget);
        AssertSourceUnchanged(catalogue, sourceBefore);
    }

    [Fact(DisplayName = "F13 force preserves a different extension owner's claim"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F13"),
     Trait("Scenarios", "X27")]
    public async Task ForceDoesNotReplaceAnotherOwnersDistinctContent()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f13-replacement-other-owner-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f13-replacement-other-owner-catalogue");
        WritePackage(catalogue, "other", "1.0.0", ToolkitTarget, Encoding.UTF8.GetBytes(OtherOwnerBytes));
        WritePackage(catalogue, "toolkit", "1.0.0", ToolkitTarget, Encoding.UTF8.GetBytes(ToolkitV1));

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ToolkitTarget);
        await InstallFrameworkAsync(consumer);
        var sourceBefore = catalogue.SnapshotState();

        var otherInstall = await consumer.RunAsync(
            "extension", "install", "other", "--source", catalogue.Path, "--automatic");
        AssertCompleted(otherInstall);
        AssertFileBytes(consumer, ToolkitTarget, Encoding.UTF8.GetBytes(OtherOwnerBytes));
        AssertExtensionRecord(consumer, "other", "1.0.0", ToolkitTarget);

        var before = consumer.SnapshotState();
        var blocked = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--force", "--automatic");
        AssertOwnerConflictBlocked(blocked);
        Assert.Equal(before, consumer.SnapshotState());
        AssertFileBytes(consumer, ToolkitTarget, Encoding.UTF8.GetBytes(OtherOwnerBytes));
        AssertExtensionRecord(consumer, "other", "1.0.0", ToolkitTarget);
        AssertNoExtensionRecord(consumer, "toolkit");
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F13 force preserves a Library source tree and its links"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F13"),
     Trait("Scenarios", "X27")]
    public async Task ForceDoesNotReplaceARegisteredLibrarySource()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f13-replacement-library-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f13-replacement-library-catalogue");
        WritePackage(
            catalogue,
            "toolkit",
            "1.0.0",
            LibrarySourcePath,
            Encoding.UTF8.GetBytes(ProtectedToolkitBytes));

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(LibrarySourcePath, LibraryDestinationPath, SettingsPath);
        consumer.WriteText(LibrarySourcePath, ProtectedLibraryBytes);
        await InstallFrameworkAsync(consumer);

        var attached = await consumer.RunAsync(
            "library", "attach", LibraryId, LibrarySourceRoot,
            "--to", LibraryDestinationRoot, "--automatic");
        AssertCompleted(attached);
        AssertRelativeLink(consumer, LibrarySourcePath, LibraryDestinationPath);
        AssertLibraryRegistration(
            consumer, LibraryId, LibrarySourceRoot, LibraryDestinationRoot, "source.md");
        var sourceBytesBefore = File.ReadAllBytes(consumer.Combine(LibrarySourcePath));
        var ownershipBefore = File.ReadAllBytes(consumer.Combine(OwnershipPath));
        var before = consumer.SnapshotState();
        var sourceBefore = catalogue.SnapshotState();

        var blocked = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path,
            "--allow-path", LibrarySourceRoot, "--force", "--automatic");
        AssertBlocked(blocked, LibrarySourcePath, "source");
        Assert.Equal(before, consumer.SnapshotState());
        Assert.Equal(sourceBytesBefore, File.ReadAllBytes(consumer.Combine(LibrarySourcePath)));
        Assert.Equal(ownershipBefore, File.ReadAllBytes(consumer.Combine(OwnershipPath)));
        AssertRelativeLink(consumer, LibrarySourcePath, LibraryDestinationPath);
        AssertLibraryRegistration(
            consumer, LibraryId, LibrarySourceRoot, LibraryDestinationRoot, "source.md");
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F13 terminal cancellation leaves an eligible occupied target unchanged"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F13"),
     Trait("Scenarios", "C21-18")]
    public async Task HumanDeclineCancelsForceWithoutEffects()
    {
        SkipUnlessWindows();
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f13-replacement-terminal-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f13-replacement-terminal-catalogue");
        WritePackage(catalogue, "toolkit", "1.0.0", ToolkitTarget, Encoding.UTF8.GetBytes(ToolkitV1));

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ToolkitTarget);
        await InstallFrameworkAsync(consumer);
        consumer.WriteText(ToolkitTarget, UserBytes);
        var before = consumer.SnapshotState();
        var sourceBefore = catalogue.SnapshotState();

        var terminal = await PublishedWindowsTerminal.RunAsync(
            consumer.Target,
            consumer.Path,
            ["extension", "install", "toolkit", "--source", catalogue.Path, "--force"],
            TerminalEnvironment(consumer),
            "n\r");

        Assert.Equal(130, terminal.ExitCode);
        Assert.Contains("apply", terminal.Transcript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cancel", terminal.Transcript, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, consumer.SnapshotState());
        AssertFileBytes(consumer, ToolkitTarget, Encoding.UTF8.GetBytes(UserBytes));
        AssertNoExtensionRecord(consumer, "toolkit");
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    private static async Task InstallFrameworkAsync(PublishedJourneyWorkspace workspace)
    {
        var result = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(result);
        AssertFileExists(workspace, OwnershipPath);
    }

    private static void WritePackage(
        PublishedJourneyWorkspace catalogue,
        string id,
        string version,
        string target,
        byte[] bytes)
    {
        catalogue.WriteText($"{id}/extension.json", Manifest(id, version));
        catalogue.WriteBytes($"{id}/content/{target}", bytes);
    }

    private static string Manifest(string id, string version)
        => $$"""
            {
              "id": "{{id}}",
              "name": "{{id}}",
              "description": "F13 fixture package {{id}}.",
              "version": "{{version}}",
              "dependencies": []
            }
            """;

    private static IReadOnlyDictionary<string, string> TerminalEnvironment(
        PublishedJourneyWorkspace workspace)
    {
        var environment = workspace.ProcessEnvironment.ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.OrdinalIgnoreCase);
        environment["TERM"] = "dumb";
        return environment;
    }

    private static void AssertCompleted(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
    }

    private static void AssertBlocked(ProcessRunResult result, string target, string reason)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains(target, result.StandardError, StringComparison.Ordinal);
        Assert.Contains(reason, result.StandardError, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertExistingTargetBlocked(ProcessRunResult result, string target)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains(target, result.StandardError, StringComparison.Ordinal);
        Assert.True(
            result.StandardError.Contains("existing", StringComparison.OrdinalIgnoreCase)
                || result.StandardError.Contains("already exists", StringComparison.OrdinalIgnoreCase),
            $"Expected an existing-file refusal in stderr:{Environment.NewLine}{result.StandardError}");
    }

    private static void AssertOwnerConflictBlocked(ProcessRunResult result)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains("Extension", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.True(
            result.StandardError.Contains("owner", StringComparison.OrdinalIgnoreCase)
                || result.StandardError.Contains("owned", StringComparison.OrdinalIgnoreCase),
            $"Expected an Extension owner conflict in stderr:{Environment.NewLine}{result.StandardError}");
    }

    private static void AssertSourceUnchanged(
        PublishedJourneyWorkspace catalogue,
        IReadOnlyDictionary<string, string> expected)
        => Assert.Equal(expected, catalogue.SnapshotState());

    private static void AssertFileExists(PublishedJourneyWorkspace workspace, string relativePath)
    {
        var path = workspace.Combine(relativePath);
        Assert.True(File.Exists(path), $"Expected file '{relativePath}'.");
        Assert.False((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0);
    }

    private static void AssertFileBytes(
        PublishedJourneyWorkspace workspace,
        string relativePath,
        byte[] expected)
    {
        AssertFileExists(workspace, relativePath);
        Assert.Equal(expected, File.ReadAllBytes(workspace.Combine(relativePath)));
    }

    private static void AssertExtensionRecord(
        PublishedJourneyWorkspace workspace,
        string id,
        string version,
        params string[] expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var extension = Assert.Single(
            document.RootElement.GetProperty("extensions").EnumerateArray(),
            item => item.GetProperty("id").GetString() == id);
        Assert.Equal(version, extension.GetProperty("version").GetString());
        var actualPaths = extension.GetProperty("paths").EnumerateArray()
            .Select(path => Normalize(path.GetString()!))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedPaths.Select(Normalize).OrderBy(path => path, StringComparer.Ordinal).ToArray(),
            actualPaths);
    }

    private static void AssertNoExtensionRecord(PublishedJourneyWorkspace workspace, string id)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        Assert.DoesNotContain(
            document.RootElement.GetProperty("extensions").EnumerateArray(),
            item => item.GetProperty("id").GetString() == id);
    }

    private static void AssertLibraryRegistration(
        PublishedJourneyWorkspace workspace,
        string id,
        string sourceRoot,
        string destinationRoot,
        params string[] expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(OwnershipPath)));
        var library = Assert.Single(
            document.RootElement.GetProperty("libraries").EnumerateArray(),
            item => item.GetProperty("id").GetString() == id);
        Assert.Equal(sourceRoot, library.GetProperty("sourceRoot").GetString());
        Assert.Equal(destinationRoot, library.GetProperty("destinationRoot").GetString());
        var actualPaths = library.GetProperty("paths").EnumerateArray()
            .Select(path => Normalize(path.GetString()!))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            expectedPaths.Select(Normalize).OrderBy(path => path, StringComparer.Ordinal).ToArray(),
            actualPaths);
    }

    private static void AssertRelativeLink(
        PublishedJourneyWorkspace workspace,
        string sourcePath,
        string destinationPath)
    {
        var destination = workspace.Combine(destinationPath);
        var actual = new FileInfo(destination).LinkTarget;
        Assert.NotNull(actual);
        var parent = Path.GetDirectoryName(destination)
            ?? throw new InvalidOperationException("A Library link has no parent directory.");
        Assert.Equal(
            Path.GetRelativePath(parent, workspace.Combine(sourcePath)).Replace('\\', '/'),
            actual);
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    private static void SkipUnlessWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F13 terminal cancellation requires the accepted Windows ConPTY transport.");
        }
    }
}
