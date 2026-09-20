using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePlanningIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove dry-run forms one complete source-independent plan without writes"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task DryRunFormsCompletePlanWithoutWrites()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-dry-run-plan");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-dry-run-plan-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--dry-run", "--detail", "standard", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = root.GetProperty("data");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.False(result.TryGetProperty("prune", out _));
        Assert.True(result.GetProperty("automatic").GetBoolean());
        Assert.Equal(
            "toolkit",
            Assert.Single(result.GetProperty("packages").EnumerateArray()).GetProperty("id").GetString());
        var package = Assert.Single(result.GetProperty("packages").EnumerateArray());
        Assert.Equal("toolkit", package.GetProperty("id").GetString());
        Assert.Equal(
            ["toolkit"],
            result.GetProperty("removalOrder").EnumerateArray().Select(value => value.GetString()));
        var effect = Assert.Single(
            result.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("delete", effect.GetProperty("action").GetString());
        Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        Assert.Equal(
            [],
            effect.GetProperty("keptFor").EnumerateArray().Select(value => value.GetString()));
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove plans multiple selected packages in dependency-first order"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task MultipleSelectedPackagesUseDependencyFirstOrder()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-multiple-plan");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-multiple-plan-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "base", "toolkit",
            "--automatic", "--dry-run", "--detail", "standard", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Equal(
            ["base", "toolkit"],
            result.GetProperty("packages")
                .EnumerateArray().Select(value => value.GetProperty("id").GetString()));
        Assert.Equal(
            ["base", "toolkit"],
            result.GetProperty("packages").EnumerateArray().Select(value => value.GetProperty("id").GetString()));
        Assert.Equal(
            ["toolkit", "base"],
            result.GetProperty("removalOrder").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove blocks a retained dependent before forming any effect"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task RetainedDependentBlocksCompletePlan()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-retained-dependent");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-retained-dependent-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "base",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.dependency-blocked");
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove plans deletion of edited final-owner content without writes"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task EditedFinalOwnerPlansDeletion()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-changed-keep-plan");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-changed-keep-plan-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        workspace.ReplaceText(".agents/toolkit.md", Document("User edit"));
        var beforeWorkspace = workspace.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        var effect = Assert.Single(
            result.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("delete", effect.GetProperty("action").GetString());
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove releases missing managed targets without requiring package source"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task MissingManagedTargetCanBeReleasedSourceIndependently()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-missing-target-plan");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-missing-target-plan-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        File.Delete(workspace.Combine(".agents/toolkit.md"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        var effect = Assert.Single(
            result.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("release-ownership", effect.GetProperty("action").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
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

    private static async Task InstallAllAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "--all",
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
}
