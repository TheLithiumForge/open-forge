using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateMutationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update replaces changed and restores missing current targets with retained recovery"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ReplacesChangedAndRestoresMissingTargets(bool force, bool git)
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
        var frameworkBefore = workspace.ReadFrameworkOwnership();
        source.ReplacePayload("base", ".agents/base/_base.md", Document("Base v2"));
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("User divergence"));
        File.Delete(workspace.Combine(".agents/base/_base.md"));
        var sourceBefore = source.Snapshot();

        if (git) workspace.CreateDirectory(".git");
        string[] arguments = ["extension", "update", "toolkit", "--source", source.Path,
            "--automatic", "--format", "json", .. force ? new[] { "--force" } : []];
        var run = await workspace.RunAsync(arguments);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        var result = root.GetProperty("data");
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal(force, result.GetProperty("force").GetBoolean());
        Assert.Contains(
            root.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/base/_base.md");
        Assert.Contains(
            root.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal(Document("Base v2"), workspace.ReadText(".agents/base/_base.md"));
        Assert.Equal(Document("Toolkit v2"), workspace.ReadText(".agents/toolkit/_toolkit.md"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(frameworkBefore.GetRawText()),
            JsonNode.Parse(workspace.ReadFrameworkOwnership().GetRawText())));
        var recovery = root.GetProperty("recovery");
        Assert.Equal("retained", recovery.GetProperty("disposition").GetString());
        var bundlePath = Assert.IsType<string>(recovery.GetProperty("path").GetString());
        await AssertPriorAsync(workspace, bundlePath, ".agents/toolkit/_toolkit.md", Document("User divergence"));
        Assert.Equal(git ? "git diff" : "open-forge doctor", document.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Contains(bundlePath, document.RootElement.GetProperty("next").GetProperty("reason").GetString(), StringComparison.Ordinal);
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update prune deletes edited retired files and releases absent ownership without deletion"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PruneDeletesEligibleRetiredTargetOnly(bool missing)
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
            "content",
            ".agents",
            "toolkit",
            "_toolkit.md"));
        if (missing) File.Delete(workspace.Combine(".agents/toolkit/_toolkit.md"));
        else workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("Edited retired bytes"));
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--prune", "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        var result = root.GetProperty("data");
        Assert.True(result.GetProperty("prune").GetBoolean());
        Assert.Equal(!missing, root.GetProperty("effects").EnumerateArray().Any(
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md"));
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit/_toolkit.md")));
        Assert.DoesNotContain(
            ".agents/toolkit/_toolkit.md",
            workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath),
            StringComparison.Ordinal);
        if (!missing)
        {
            var recovery = root.GetProperty("recovery");
            Assert.Equal("retained", recovery.GetProperty("disposition").GetString());
            var bundlePath = Assert.IsType<string>(recovery.GetProperty("path").GetString());
            await AssertPriorAsync(workspace, bundlePath, ".agents/toolkit/_toolkit.md", Document("Edited retired bytes"));
        }
        else
        {
            Assert.Equal("retained", root.GetProperty("recovery").GetProperty("disposition").GetString());
        }
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
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
            "--force", "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.workspace-lock-unavailable");
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").ValueKind);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static async Task AssertPriorAsync(ExtensionInstallIntegrationWorkspace workspace,
        string bundlePath, string target, string expected)
    {
        var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, bundlePath, TestContext.Current.CancellationToken);
        var entry = Assert.Single(Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified).Entries, value => value.TargetPath == target);
        using var archive = ZipFile.OpenRead(bundlePath);
        using var prior = new StreamReader(Assert.IsType<ZipArchiveEntry>(archive.GetEntry(entry.PriorPayload!)).Open());
        Assert.Equal(expected, await prior.ReadToEndAsync(TestContext.Current.CancellationToken));
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
