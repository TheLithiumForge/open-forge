using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdatePlanningIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update dry-run resolves the selected dependency closure in order without effects"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task DryRunResolvesDependencyClosureInOrderWithoutEffects()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-dry-run-closure");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-dry-run-closure-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base v1")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("base", ".agents/base/_base.md", Document("Base v2"));
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension update", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = root.GetProperty("data");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.NotEmpty(root.GetProperty("effects").EnumerateArray());
        var packagePaths = root.GetProperty("effects")
            .EnumerateArray()
            .Select(effect => effect.GetProperty("path").GetString())
            .Where(path => path is ".agents/base/_base.md" or ".agents/toolkit/_toolkit.md")
            .ToArray();
        Assert.Equal([".agents/base/_base.md", ".agents/toolkit/_toolkit.md"], packagePaths);
        Assert.All(
            root.GetProperty("effects").EnumerateArray(),
            effect => Assert.Equal("planned", effect.GetProperty("outcome").GetString()));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update normal dry-run plans changed current content without applying it"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task NormalDryRunPlansChangedCurrentContentWithoutMutation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-divergence");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-divergence-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("User divergence"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.DoesNotContain(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.managed-divergence");
        Assert.NotEmpty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(Document("User divergence"), workspace.ReadText(".agents/toolkit/_toolkit.md"));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
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
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
