using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePlanningIntegrationTests
{
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
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.False(result.GetProperty("prune").GetBoolean());
        Assert.True(result.GetProperty("automatic").GetBoolean());
        Assert.Equal("explicit-ids", result.GetProperty("selection").GetProperty("selectedBy").GetString());
        Assert.Equal(
            "toolkit",
            Assert.Single(result.GetProperty("selection").GetProperty("ids").EnumerateArray()).GetString());
        var package = Assert.Single(result.GetProperty("dependencies").GetProperty("packages").EnumerateArray());
        Assert.Equal("toolkit", package.GetProperty("id").GetString());
        Assert.True(package.GetProperty("selectedForRemoval").GetBoolean());
        Assert.Empty(package.GetProperty("dependencies").EnumerateArray());
        Assert.Equal(
            ["toolkit"],
            result.GetProperty("dependencies").GetProperty("removalOrder")
                .EnumerateArray().Select(value => value.GetString()));
        var path = Assert.Single(
            result.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("unchanged-final-owner", path.GetProperty("classification").GetString());
        Assert.Equal("delete", path.GetProperty("action").GetString());
        Assert.Equal(
            ["toolkit"],
            path.GetProperty("selectedOwnerIds").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.Empty(path.GetProperty("remainingOwnerIds").EnumerateArray());
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md"
                && effect.GetProperty("action").GetString() == "delete"
                && effect.GetProperty("outcome").GetString() == "planned");
        Assert.Equal("trusted", result.GetProperty("lifecycle").GetProperty("trust").GetString());
        Assert.Equal("complete", result.GetProperty("lifecycle").GetProperty("coverage").GetString());
        Assert.Equal("publish", result.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("planned", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("planned", result.GetProperty("verification").GetProperty("targets").GetString());
        Assert.Equal("planned", result.GetProperty("verification").GetProperty("topology").GetString());
        Assert.Equal(
            "planned",
            result.GetProperty("verification").GetProperty("extensionsLifecycle").GetString());
        Assert.True(result.GetProperty("packageSourceUnchanged").GetBoolean());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

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
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(
            ["base", "toolkit"],
            result.GetProperty("selection").GetProperty("ids")
                .EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            ["base", "toolkit"],
            result.GetProperty("dependencies").GetProperty("packages")
                .EnumerateArray().Select(value => value.GetProperty("id").GetString()));
        Assert.Equal(
            ["toolkit", "base"],
            result.GetProperty("dependencies").GetProperty("removalOrder")
                .EnumerateArray().Select(value => value.GetString()));
        Assert.Empty(result.GetProperty("dependencies").GetProperty("retainedDependentBlockers").EnumerateArray());
        Assert.Empty(result.GetProperty("dependencies").GetProperty("retainedOrphanDependencyIds").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

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
            "--automatic", "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.dependency-blocked");
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Fact(
        DisplayName = "Extension Remove classifies changed final-owner content as Keep-as-unmanaged by default"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ChangedFinalOwnerDefaultsToKeepAsUnmanaged()
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
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        var path = Assert.Single(
            result.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("changed-final-owner", path.GetProperty("classification").GetString());
        Assert.Equal("keep-as-unmanaged", path.GetProperty("action").GetString());
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-remove.managed-divergence");
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

    [Fact(
        DisplayName = "Extension Remove selects changed-content Delete only with same-request prune"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task SameRequestPruneSelectsChangedContentDelete()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-changed-prune-plan");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-changed-prune-plan-source");
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
            "--prune", "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.True(result.GetProperty("prune").GetBoolean());
        var path = Assert.Single(
            result.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("changed-final-owner", path.GetProperty("classification").GetString());
        Assert.Equal("delete", path.GetProperty("action").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
    }

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
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        var path = Assert.Single(
            result.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("missing", path.GetProperty("classification").GetString());
        Assert.Equal("release-ownership", path.GetProperty("action").GetString());
        Assert.DoesNotContain(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md");
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
            "--automatic", "--json",
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
}
