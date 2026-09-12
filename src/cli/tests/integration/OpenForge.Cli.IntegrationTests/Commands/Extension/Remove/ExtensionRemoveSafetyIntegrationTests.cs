using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveSafetyIntegrationTests
{
    [Fact(
        DisplayName = "Extension Remove treats a missing lifecycle document as incomplete without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task MissingLifecycleIsIncompleteAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-missing-lifecycle");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-missing-lifecycle-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        File.Delete(workspace.Combine(ExtensionInstallIntegrationWorkspace.LifecyclePath));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.lifecycle-unavailable");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Fact(
        DisplayName = "Extension Remove treats a malformed lifecycle document as blocked without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task MalformedLifecycleIsBlockedAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-malformed-lifecycle");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-malformed-lifecycle-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        workspace.ReplaceText(
            ExtensionInstallIntegrationWorkspace.LifecyclePath,
            "{\n");
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.lifecycle-blocked");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

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
            "--automatic", "--json",
        ]);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.selection-required");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

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
            "--automatic", "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.workspace-lock-unavailable");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

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
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Equal(
            "not-created",
            document.RootElement.GetProperty("result")
                .GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeRecovery, SnapshotRecovery(recoveryDirectory));
    }

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
        var lifecycleBefore = workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath);
        var sourceBefore = source.Snapshot();

        try
        {
            var run = await workspace.RunAsync(
            [
                "extension", "remove", "toolkit",
                "--prune", "--automatic", "--json",
            ]);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Blocked, run.Status);
            Assert.Equal(string.Empty, run.StandardError);
            using var document = JsonDocument.Parse(run.StandardOutput);
            Assert.Contains(
                document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
                finding => finding.GetProperty("code").GetString()
                    == "extension-remove.target-unsafe");
            Assert.Equal(lifecycleBefore, workspace.ReadText(
                ExtensionInstallIntegrationWorkspace.LifecyclePath));
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
            "--automatic", "--json",
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
