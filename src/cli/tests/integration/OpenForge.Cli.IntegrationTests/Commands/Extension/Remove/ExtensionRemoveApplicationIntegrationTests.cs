using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveApplicationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove applies source-independent ownership release and preserves unowned content"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task AppliesRemovalAndPreservesUnownedContent()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-apply-preservation");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-apply-preservation-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        var frameworkBefore = workspace.ReadFrameworkOwnership();
        var sourceBefore = source.Snapshot();
        var unownedBefore = workspace.ReadText("workspace-note.md");

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = root.GetProperty("data");
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.True(result.GetProperty("automatic").GetBoolean());
        Assert.False(result.TryGetProperty("prune", out _));
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md"
                && effect.GetProperty("action").GetString() == "delete"
                && effect.GetProperty("outcome").GetString() == "verified");
        Assert.Equal("verified", result.GetProperty("verification").GetProperty("extensionRecord").GetString());
        Assert.Equal("retained", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.All(
            result.GetProperty("verification").EnumerateObject(),
            verification => Assert.Equal("verified", verification.Value.GetString()));
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(unownedBefore, workspace.ReadText("workspace-note.md"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(frameworkBefore.GetRawText()),
            JsonNode.Parse(workspace.ReadFrameworkOwnership().GetRawText())));
        Assert.Empty(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove releases one shared owner and deletes the unchanged final owner later"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task SharedOwnerReleasePrecedesFinalOwnerDeletion()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-shared-owner-application");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-shared-owner-application-source");
        const string target = ".agents/shared.txt";
        source.AddPackage("alpha", [], (target, "shared package bytes\n"));
        source.AddPackage("beta", [], (target, "shared package bytes\n"));
        await InstallAllAsync(workspace, source);
        var installedOwnership = await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(),
            workspace.Workspace,
            CancellationToken.None);
        Assert.Equal(
            ["alpha", "beta"],
            installedOwnership.Document.Extensions.Select(extension => extension.Id));
        Assert.Equal(["alpha", "beta"], installedOwnership.Document.OwnersOf(target));
        Assert.All(
            installedOwnership.Document.Extensions,
            extension => Assert.Contains(target, extension.Paths));
        var sourceBefore = source.Snapshot();

        var first = await workspace.RunAsync(
        [
            "extension", "remove", "alpha",
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        using var firstDocument = JsonDocument.Parse(first.StandardOutput);
        var firstResult = firstDocument.RootElement.GetProperty("data");
        var shared = Assert.Single(
            firstResult.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == target);
        Assert.Equal("retain-shared", shared.GetProperty("action").GetString());
        Assert.Equal(
            ["alpha"],
            shared.GetProperty("ownersBefore").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.Equal(
            ["beta"],
            shared.GetProperty("ownersAfter").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.True(File.Exists(workspace.Combine(target)));
        var remainingPath = Assert.Single(
            workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal("beta", remainingPath.GetProperty("id").GetString());
        Assert.Equal(target, Assert.Single(remainingPath.GetProperty("paths").EnumerateArray()).GetString());
        var remainingOwnership = await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(),
            workspace.Workspace,
            CancellationToken.None);
        Assert.Equal(["beta"], remainingOwnership.Document.OwnersOf(target));
        Assert.DoesNotContain(
            remainingOwnership.Document.Extensions,
            extension => extension.Id == "alpha");
        Assert.Contains(
            remainingOwnership.Document.Extensions,
            extension => extension.Id == "beta"
                && extension.Paths.Contains(target, StringComparer.Ordinal));

        var cleanup = await workspace.RunAsync(["cleanup", "--format", "json"]);
        Assert.Equal(0, cleanup.ExitCode);

        var second = await workspace.RunAsync(
        [
            "extension", "remove", "beta",
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, second.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        using var secondDocument = JsonDocument.Parse(second.StandardOutput);
        var secondResult = secondDocument.RootElement.GetProperty("data");
        var final = Assert.Single(
            secondResult.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == target);
        Assert.Equal("delete", final.GetProperty("action").GetString());
        Assert.False(File.Exists(workspace.Combine(target)));
        Assert.Empty(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove deletes edited final-owner content and retains its exact recovery bytes"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task EditedFinalOwnerIsDeletedWithRecovery()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-changed-keep-application");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-changed-keep-application-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        var changed = Document("User edit");
        workspace.ReplaceText(".agents/toolkit.md", changed);
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        var path = Assert.Single(
            result.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("delete", path.GetProperty("action").GetString());
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        var bundlePath = Assert.IsType<string>(result.GetProperty("recovery").GetProperty("path").GetString());
        var recovered = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, bundlePath, TestContext.Current.CancellationToken);
        var entry = Assert.Single(Assert.IsType<RecoveryBundleVerifiedRead>(recovered.Verified).Entries,
            value => value.TargetPath == ".agents/toolkit.md");
        using var archive = ZipFile.OpenRead(bundlePath);
        using var prior = new StreamReader(Assert.IsType<ZipArchiveEntry>(archive.GetEntry(entry.PriorPayload!)).Open());
        Assert.Equal(changed, await prior.ReadToEndAsync(TestContext.Current.CancellationToken));
        Assert.Empty(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove projects generated navigation while preserving the package source"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task GeneratedNavigationIsUpdatedAndSourceIsUntouched()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-generated-application");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-generated-application-source");
        const string target = ".agents/toolkit/_toolkit.md";
        source.AddPackage("toolkit", [], (target, Document("Toolkit")));
        await InstallAsync(workspace, source);
        var loaderBefore = workspace.ReadText(".agents/loader.md");
        Assert.Contains("toolkit/_toolkit.md", loaderBefore, StringComparison.Ordinal);
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("action").GetString() == "updated"
                && effect.GetProperty("outcome").GetString() == "changed");
        Assert.False(File.Exists(workspace.Combine(target)));
        Assert.DoesNotContain("toolkit/_toolkit.md", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove converges a trusted repeated request to an exact no-op"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task RepeatedTrustedRequestIsVerifiedNoOp()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-repeated-no-op");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-repeated-no-op-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        var arguments = new[]
        {
            "extension", "remove", "toolkit", "--automatic", "--detail", "full", "--format", "json",
        };
        var applied = await workspace.RunAsync(arguments);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        var afterApply = workspace.Snapshot();

        var repeated = await workspace.RunAsync(arguments);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Equal(string.Empty, repeated.StandardError);
        using var document = JsonDocument.Parse(repeated.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.All(
            result.GetProperty("verification").EnumerateObject(),
            verification => Assert.Equal("verified", verification.Value.GetString()));
        Assert.Equal(afterApply, workspace.Snapshot());
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
