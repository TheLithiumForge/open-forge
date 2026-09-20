using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateSafetyIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update reports no recorded installation independently of legacy lifecycle availability"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData("malformed", 2, CliSemanticStatus.Attention, "extension-update.lifecycle-observation")]
    [InlineData("blocked", 2, CliSemanticStatus.Attention, "extension-update.lifecycle-observation")]
    [InlineData("missing", 2, CliSemanticStatus.Attention, "extension-update.lifecycle-observation")]
    public async Task LifecycleGatePreservesTypedSafetyBoundary(
        string lifecycleState,
        int exitCode,
        object statusValue,
        string findingCode)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-lifecycle-{lifecycleState}");
        if (lifecycleState == "malformed")
        {
            await workspace.SeedFrameworkAsync();
            workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.OwnershipPath, "{");
        }
        else if (lifecycleState == "blocked")
        {
            Directory.CreateDirectory(
                workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath));
        }

        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-lifecycle-{lifecycleState}-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(exitCode, run.ExitCode);
        Assert.Equal(Assert.IsType<CliSemanticStatus>(statusValue), run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == findingCode);
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update rejects explicit IDs combined with all before acquiring mutation authority"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ExplicitIdsAndAllAreRejectedBeforeMutation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-selection-conflict");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-selection-conflict-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.invalid-input");
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update treats missing selected source coverage as incomplete without fallback"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task MissingSelectedSourceCoverageIsIncompleteWithoutFallback()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-source-coverage");
        await workspace.SeedFrameworkAsync();
        using var installedSource = ExtensionInstallCatalogue.Create(
            "extension-update-source-coverage-installed");
        installedSource.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, installedSource, "toolkit");
        using var selectedSource = ExtensionInstallCatalogue.Create(
            "extension-update-source-coverage-selected");
        selectedSource.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = selectedSource.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", selectedSource.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.source-unavailable");
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, selectedSource.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update automatic mode never broadens an omitted selection to all packages"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task AutomaticModeDoesNotBroadenOmittedSelection()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-automatic-selection");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-automatic-selection-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.selection-required");
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source,
        string id)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", id,
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
