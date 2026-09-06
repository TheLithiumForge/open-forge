using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateMutationIntegrationTests
{
    [Fact(DisplayName = "Extension Update force replaces changed and restores missing current targets with verified recovery"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ForceReplacesChangedAndRestoresMissingTargets()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-force-mutation");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-force-mutation-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base v1")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        var frameworkBefore = workspace.ReadFrameworkLifecycle();
        source.ReplacePayload("base", ".agents/base/_base.md", Document("Base v2"));
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("User divergence"));
        File.Delete(workspace.Combine(".agents/base/_base.md"));
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--force", "--automatic", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.True(result.GetProperty("force").GetBoolean());
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/base/_base.md");
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal(Document("Base v2"), workspace.ReadText(".agents/base/_base.md"));
        Assert.Equal(Document("Toolkit v2"), workspace.ReadText(".agents/toolkit/_toolkit.md"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(frameworkBefore.GetRawText()),
            JsonNode.Parse(workspace.ReadFrameworkLifecycle().GetRawText())));
        Assert.Equal("removed", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Fact(DisplayName = "Extension Update prune deletes only an eligible retired target and publishes the reduced lifecycle"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task PruneDeletesEligibleRetiredTargetOnly()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-prune-retired");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-prune-retired-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        File.Delete(Path.Combine(
            source.PackagePath("toolkit"),
            "payload",
            ".agents",
            "toolkit",
            "_toolkit.md"));
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--prune", "--automatic", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.True(result.GetProperty("prune").GetBoolean());
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit/_toolkit.md")));
        Assert.DoesNotContain(
            ".agents/toolkit/_toolkit.md",
            workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath),
            StringComparison.Ordinal);
        Assert.Equal("removed", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Fact(DisplayName = "Extension Update lock contention blocks every effect and preserves source and workspace"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task LockContentionBlocksEveryEffect()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-lock-contention");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-lock-contention-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        using var heldLease = workspace.HoldLock();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--force", "--automatic", "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.workspace-lock-unavailable");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
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
            "--automatic", "--json",
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
