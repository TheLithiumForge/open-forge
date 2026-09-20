using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveSafetyIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Remove uses lock ownership despite missing or malformed legacy lifecycle"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LegacyLifecycleDoesNotGateRemoval(bool malformed)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-remove-legacy-independent");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-remove-legacy-independent-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        if (malformed)
        {
            workspace.CreateOccupant(ExtensionInstallIntegrationWorkspace.LifecyclePath, "{\n");
        }
        else
        {
            Assert.False(File.Exists(workspace.Combine(ExtensionInstallIntegrationWorkspace.LifecyclePath)));
        }
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(["extension", "remove", "toolkit", "--automatic", "--format", "json"]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(sourceBefore, source.Snapshot());
        if (malformed)
        {
            Assert.Equal("{\n", workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove rejects omitted IDs in automatic JSON mode before mutation"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task AutomaticOmittedSelectionIsInvalidAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-automatic-selection");
        await workspace.SeedFrameworkAsync();
        var beforeWorkspace = workspace.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.selection-required");
        Assert.DoesNotContain(result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("outcome").GetString() is "verified" or "planned");
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove respects the real workspace lock before any effect"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task HeldWorkspaceLockBlocksEveryEffect()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-lock-contention");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-lock-contention-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        using var heldLock = workspace.HoldLock();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.workspace-lock-unavailable");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove dry-run leaves the external recovery boundary untouched"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task DryRunLeavesRecoveryBoundaryUntouched()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-recovery-dry-run");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-recovery-dry-run-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        var recoveryDirectory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(
            workspace.Workspace);
        var beforeRecovery = SnapshotRecovery(recoveryDirectory);
        var beforeWorkspace = workspace.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeRecovery, SnapshotRecovery(recoveryDirectory));
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove blocks a physical target escape and preserves the outside file"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task PhysicalTargetEscapeIsBlockedWithoutWrites()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-target-escape");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-target-escape-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        using var outside = TemporaryWorkspace.Create("extension-remove-target-escape-outside");
        var outsidePath = outside.CreateFile("outside.md", "outside content\n");
        var targetPath = workspace.Combine(".agents/toolkit.md");
        File.Delete(targetPath);
        File.CreateSymbolicLink(targetPath, outsidePath);
        var lifecycleBefore = workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath);
        var sourceBefore = source.Snapshot();

        try
        {
            var run = await workspace.RunAsync(
            [
                "extension", "remove", "toolkit",
                "--automatic", "--format", "json",
            ]);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Blocked, run.Status);
            Assert.Equal(string.Empty, run.StandardError);
            using var document = JsonDocument.Parse(run.StandardOutput);
            Assert.Contains(
                document.RootElement.GetProperty("findings").EnumerateArray(),
                finding => finding.GetProperty("code").GetString()
                    == "extension-remove.target-unsafe");
            Assert.Equal(lifecycleBefore, workspace.ReadText(
                ExtensionInstallIntegrationWorkspace.OwnershipPath));
            Assert.Equal("outside content\n", File.ReadAllText(outsidePath));
            Assert.Equal(sourceBefore, source.Snapshot());
        }
        finally
        {
            File.Delete(targetPath);
        }
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");

    private static Dictionary<string, string> SnapshotRecovery(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["directory"] = "absent",
            };
        }

        return Directory.EnumerateFileSystemEntries(directory)
            .Order(StringComparer.Ordinal)
            .ToDictionary(
                path => Path.GetFileName(path),
                PathState,
                StringComparer.Ordinal);
    }

    private static string PathState(string path)
    {
        if (File.Exists(path))
        {
            return $"file:{Convert.ToBase64String(File.ReadAllBytes(path))}";
        }

        return Directory.Exists(path) ? "directory" : "absent";
    }
}
