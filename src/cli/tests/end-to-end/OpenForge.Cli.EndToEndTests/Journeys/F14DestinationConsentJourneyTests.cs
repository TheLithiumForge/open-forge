using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F14DestinationConsentJourneyTests
{
    private const string Feature = "destination-consent-journey";
    private const string ExternalTarget = "docs/team.md";
    private const string InternalTarget = ".agents/guidance/internal.md";
    private const string SettingsPath = ".agents/open-forge.json";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string MalformedSettings = "{ this is not valid JSON\n";
    private const string EmptyGrant = "{\"allowInstallPaths\":[]}\n";

    private const string ToolkitV1 = """
        ---
        open-forge:
          description: F14 toolkit fixture version 1
          tags: [Extension]
        ---
        # F14 Toolkit

        Toolkit external bytes version 1.
        """;

    private const string ToolkitV2 = """
        ---
        open-forge:
          description: F14 toolkit fixture version 2
          tags: [Extension]
        ---
        # F14 Toolkit v2

        Toolkit external bytes version 2.
        """;

    private const string InternalBytes = """
        ---
        open-forge:
          description: F14 implicit .agents package
          tags: [Guidance]
        ---
        # F14 Internal

        Implicit .agents package bytes.
        """;

    [Fact(DisplayName = "F14 blocks an external install without destination consent"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "C21-05")]
    public async Task MissingExternalConsentBlocksAutomaticInstallWithoutEffects()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-missing-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-missing-catalogue");
        WriteToolkitPackage(catalogue, "1.0.0", ToolkitV1);

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ExternalTarget, SettingsPath);
        await InstallFrameworkAsync(consumer);
        var sourceBefore = catalogue.SnapshotState();
        var before = consumer.SnapshotState();

        var blocked = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertBlocked(blocked, ExternalTarget);
        Assert.Equal(before, consumer.SnapshotState());
        AssertMissingFile(consumer, ExternalTarget);
        AssertMissingFile(consumer, SettingsPath);
        AssertNoExtensionRecord(consumer, "toolkit");
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F14 once consent installs the exact external file without saving a grant"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "C21-07")]
    public async Task TerminalOnceConsentInstallsWithoutPersistingPermission()
    {
        SkipUnlessWindows();
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-once-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-once-catalogue");
        WriteToolkitPackage(catalogue, "1.0.0", ToolkitV1);

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ExternalTarget, SettingsPath);
        await InstallFrameworkAsync(consumer);
        var failedState = await PrepareGrantFreeExternalStateAsync(consumer, catalogue);
        AssertBlocked(failedState, ExternalTarget);
        var sourceBefore = catalogue.SnapshotState();

        var terminal = await PublishedWindowsTerminal.RunAsync(
            consumer.Target,
            consumer.Path,
            ["extension", "install", "toolkit", "--source", catalogue.Path],
            TerminalEnvironment(consumer),
            "once\ryes\r");

        Assert.Equal(0, terminal.ExitCode);
        Assert.Contains("once", terminal.Transcript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("apply", terminal.Transcript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(ExternalTarget, terminal.Transcript, StringComparison.Ordinal);
        AssertFileBytes(consumer, ExternalTarget, File.ReadAllBytes(catalogue.Combine($"toolkit/content/{ExternalTarget}")));
        AssertMissingFile(consumer, SettingsPath);
        AssertExtensionRecord(consumer, "toolkit", "1.0.0", ExternalTarget);
        Assert.False(File.Exists(consumer.Combine("docs/team")));
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F14 explicit file consent persists and revocation blocks a later update"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "X14,C22-09")]
    public async Task ExplicitFileGrantPersistsUntilRevokedAndThenProtectsUpdate()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-persistent-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-persistent-catalogue");
        WriteToolkitPackage(catalogue, "1.0.0", ToolkitV1);

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ExternalTarget, SettingsPath);
        await InstallFrameworkAsync(consumer);
        var failedState = await PrepareGrantFreeExternalStateAsync(consumer, catalogue);
        AssertBlocked(failedState, ExternalTarget);

        var installed = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path,
            "--allow-path", ExternalTarget, "--automatic");
        AssertCompleted(installed);
        AssertGrant(consumer, ExternalTarget);
        var v1Bytes = File.ReadAllBytes(catalogue.Combine($"toolkit/content/{ExternalTarget}"));
        Assert.Equal(v1Bytes, File.ReadAllBytes(consumer.Combine(ExternalTarget)));
        AssertExtensionRecord(consumer, "toolkit", "1.0.0", ExternalTarget);

        WriteToolkitPackage(catalogue, "2.0.0", ToolkitV2);
        var sourceAfterV2 = catalogue.SnapshotState();
        var ownershipBeforeRevoke = File.ReadAllBytes(consumer.Combine(OwnershipPath));
        consumer.WriteText(SettingsPath, EmptyGrant);
        var beforeUpdate = consumer.SnapshotState();

        var blocked = await consumer.RunAsync(
            "extension", "update", "toolkit", "--source", catalogue.Path, "--automatic");
        AssertBlocked(blocked, "toolkit");
        Assert.Equal(beforeUpdate, consumer.SnapshotState());
        Assert.Equal(v1Bytes, File.ReadAllBytes(consumer.Combine(ExternalTarget)));
        Assert.Equal(ownershipBeforeRevoke, File.ReadAllBytes(consumer.Combine(OwnershipPath)));
        AssertGrant(consumer);
        AssertSourceUnchanged(catalogue, sourceAfterV2);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F14 dry-run previews an exact external grant without saving or installing"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "C21-14")]
    public async Task DryRunAllowPathDoesNotPersistConsentOrContent()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-dry-run-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-dry-run-catalogue");
        WriteToolkitPackage(catalogue, "1.0.0", ToolkitV1);

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ExternalTarget, SettingsPath);
        await InstallFrameworkAsync(consumer);
        var failedState = await PrepareGrantFreeExternalStateAsync(consumer, catalogue);
        AssertBlocked(failedState, ExternalTarget);
        var sourceBefore = catalogue.SnapshotState();
        var before = consumer.SnapshotState();

        var preview = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path,
            "--allow-path", ExternalTarget, "--dry-run");
        AssertCompleted(preview);
        Assert.Contains(ExternalTarget, preview.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, consumer.SnapshotState());
        AssertMissingFile(consumer, ExternalTarget);
        AssertMissingFile(consumer, SettingsPath);
        AssertNoExtensionRecord(consumer, "toolkit");
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F14 keeps a saved grant visible when a locked replacement fails"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "X26")]
    public async Task SavedGrantSurvivesAWindowsSharingFailureBeforeReplacement()
    {
        SkipUnlessWindows();
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-sharing-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-sharing-catalogue");
        WriteToolkitPackage(catalogue, "1.0.0", ToolkitV1);

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ExternalTarget, SettingsPath);
        await InstallFrameworkAsync(consumer);
        var failedState = await PrepareGrantFreeExternalStateAsync(consumer, catalogue);
        AssertBlocked(failedState, ExternalTarget);
        var installed = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path,
            "--allow-path", ExternalTarget, "--automatic");
        AssertCompleted(installed);
        AssertGrant(consumer, ExternalTarget);

        WriteToolkitPackage(catalogue, "2.0.0", ToolkitV2);
        var sourceAfterV2 = catalogue.SnapshotState();
        consumer.WriteText(SettingsPath, EmptyGrant);
        var v1Bytes = File.ReadAllBytes(consumer.Combine(ExternalTarget));
        var ownershipBefore = File.ReadAllBytes(consumer.Combine(OwnershipPath));

        using (var heldTarget = new FileStream(
                   consumer.Combine(ExternalTarget),
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.Read))
        {
            var failed = await consumer.RunAsync(
                "extension", "update", "toolkit", "--source", catalogue.Path,
                "--allow-path", ExternalTarget, "--automatic");
            Assert.Equal(1, failed.ExitCode);
            Assert.Empty(failed.StandardOutput);
            Assert.NotEmpty(failed.StandardError);
            var report = failed.StandardError + failed.StandardOutput;
            Assert.Contains("grant", report, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("No files were changed", report, StringComparison.OrdinalIgnoreCase);
            AssertGrant(consumer, ExternalTarget);
            Assert.Equal(v1Bytes, File.ReadAllBytes(consumer.Combine(ExternalTarget)));
            Assert.Equal(ownershipBefore, File.ReadAllBytes(consumer.Combine(OwnershipPath)));
            AssertRecoveryEvidence(consumer);
        }

        AssertSourceUnchanged(catalogue, sourceAfterV2);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
    }

    [Fact(DisplayName = "F14 preserves malformed settings during an external grant attempt"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "X14")]
    public async Task MalformedSettingsAreNotOverwrittenForAnExternalGrant()
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-malformed-external-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-malformed-external-catalogue");
        WriteToolkitPackage(catalogue, "1.0.0", ToolkitV1);

        consumer.ExpectCoreInstall();
        consumer.ExpectFiles(ExternalTarget, SettingsPath);
        await InstallFrameworkAsync(consumer);
        consumer.WriteText(SettingsPath, MalformedSettings);
        var sourceBefore = catalogue.SnapshotState();
        var before = consumer.SnapshotState();

        var blocked = await consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path,
            "--allow-path", ExternalTarget, "--automatic");
        Assert.Equal(5, blocked.ExitCode);
        Assert.Empty(blocked.StandardOutput);
        Assert.Contains(SettingsPath, blocked.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, consumer.SnapshotState());
        Assert.Equal(MalformedSettings, File.ReadAllText(consumer.Combine(SettingsPath)));
        AssertMissingFile(consumer, ExternalTarget);
        AssertNoExtensionRecord(consumer, "toolkit");
        AssertSourceUnchanged(catalogue, sourceBefore);
        consumer.LockStore.AssertPersistentZeroByteLock(consumer.Path);
        consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
    }

    [Fact(DisplayName = "F14 implicitly admits internal targets with valid settings and blocks malformed settings without effects"),
     Trait("Feature", Feature), Trait("Evidence", "EndToEnd"), Trait("Journey", "F14"),
     Trait("Scenarios", "X14")]
    public async Task InternalOnlyPackageUsesImplicitAdmissionAndMalformedSettingsBlockWithoutMutation()
    {
        using var admittedConsumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-internal-admitted-consumer");
        using var malformedConsumer = PublishedJourneyWorkspace.Create("e2e-f14-consent-malformed-internal-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-f14-consent-internal-catalogue");
        WritePackage(catalogue, "internal", "1.0.0", InternalTarget, Encoding.UTF8.GetBytes(InternalBytes));

        admittedConsumer.ExpectCoreInstall();
        admittedConsumer.ExpectFiles(InternalTarget, SettingsPath);
        await InstallFrameworkAsync(admittedConsumer);
        admittedConsumer.WriteText(SettingsPath, EmptyGrant);
        var sourceBefore = catalogue.SnapshotState();

        var installed = await admittedConsumer.RunAsync(
            "extension", "install", "internal", "--source", catalogue.Path, "--automatic");
        AssertCompleted(installed);
        AssertFileBytes(admittedConsumer, InternalTarget, Encoding.UTF8.GetBytes(InternalBytes));
        Assert.Equal(EmptyGrant, File.ReadAllText(admittedConsumer.Combine(SettingsPath)));
        AssertExtensionRecord(admittedConsumer, "internal", "1.0.0", InternalTarget);
        AssertSourceUnchanged(catalogue, sourceBefore);
        admittedConsumer.LockStore.AssertPersistentZeroByteLock(admittedConsumer.Path);
        admittedConsumer.LockStore.AssertNoRecoveryArtifacts(admittedConsumer.Path);

        malformedConsumer.ExpectCoreInstall();
        malformedConsumer.ExpectFiles(InternalTarget, SettingsPath);
        await InstallFrameworkAsync(malformedConsumer);
        malformedConsumer.WriteText(SettingsPath, MalformedSettings);
        var malformedState = malformedConsumer.SnapshotState();

        var blocked = await malformedConsumer.RunAsync(
            "extension", "install", "internal", "--source", catalogue.Path, "--automatic");
        Assert.Equal(5, blocked.ExitCode);
        Assert.Empty(blocked.StandardOutput);
        Assert.Contains(SettingsPath, blocked.StandardError, StringComparison.Ordinal);
        Assert.Equal(malformedState, malformedConsumer.SnapshotState());
        Assert.Equal(MalformedSettings, File.ReadAllText(malformedConsumer.Combine(SettingsPath)));
        AssertMissingFile(malformedConsumer, InternalTarget);
        AssertNoExtensionRecord(malformedConsumer, "internal");
        AssertSourceUnchanged(catalogue, sourceBefore);
        malformedConsumer.LockStore.AssertPersistentZeroByteLock(malformedConsumer.Path);
        malformedConsumer.LockStore.AssertNoRecoveryArtifacts(malformedConsumer.Path);
    }

    private static async Task InstallFrameworkAsync(PublishedJourneyWorkspace workspace)
    {
        var result = await workspace.RunAsync("install", "--automatic");
        AssertCompleted(result);
        AssertFileExists(workspace, OwnershipPath);
    }

    private static Task<ProcessRunResult> PrepareGrantFreeExternalStateAsync(
        PublishedJourneyWorkspace consumer,
        PublishedJourneyWorkspace catalogue)
        => consumer.RunAsync(
            "extension", "install", "toolkit", "--source", catalogue.Path, "--automatic");

    private static void WriteToolkitPackage(
        PublishedJourneyWorkspace catalogue,
        string version,
        string document)
    {
        catalogue.WriteText("toolkit/extension.json", Manifest("toolkit", version));
        catalogue.WriteBytes(
            $"toolkit/content/{ExternalTarget}",
            Encoding.UTF8.GetBytes(document));
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
              "description": "F14 fixture package {{id}}.",
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

    private static void AssertBlocked(ProcessRunResult result, string target)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains(target, result.StandardError, StringComparison.Ordinal);
        Assert.True(
            result.StandardError.Contains("no grant", StringComparison.OrdinalIgnoreCase)
                || result.StandardError.Contains("permission", StringComparison.OrdinalIgnoreCase)
                || result.StandardError.Contains("consent", StringComparison.OrdinalIgnoreCase),
            $"Expected a missing-grant or consent refusal in stderr:{Environment.NewLine}{result.StandardError}");
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

    private static void AssertMissingFile(PublishedJourneyWorkspace workspace, string relativePath)
        => Assert.False(File.Exists(workspace.Combine(relativePath)), $"Did not expect '{relativePath}'.");

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

    private static void AssertGrant(
        PublishedJourneyWorkspace workspace,
        params string[] expectedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(workspace.Combine(SettingsPath)));
        var actualPaths = document.RootElement.GetProperty("allowInstallPaths")
            .EnumerateArray()
            .Select(path => path.GetString() ?? throw new InvalidOperationException("A grant path was null."))
            .ToArray();
        Assert.Equal(expectedPaths, actualPaths);
    }

    private static void AssertRecoveryEvidence(PublishedJourneyWorkspace workspace)
    {
        var recoveryDirectory = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        Assert.True(Directory.Exists(recoveryDirectory));
        Assert.NotEmpty(Directory.EnumerateFiles(recoveryDirectory, "*", SearchOption.TopDirectoryOnly));
    }

    private static string Normalize(string path) => path.Replace('\\', '/');

    private static void SkipUnlessWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("F14 terminal and sharing cases require the accepted Windows transport/filesystem capability.");
        }
    }
}
