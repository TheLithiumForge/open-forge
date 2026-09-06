using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveApplicationIntegrationTests
{
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
        var frameworkBefore = workspace.ReadFrameworkLifecycle();
        var sourceBefore = source.Snapshot();
        var unownedBefore = workspace.ReadText("workspace-note.md");

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = root.GetProperty("result");
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.True(result.GetProperty("automatic").GetBoolean());
        Assert.False(result.GetProperty("prune").GetBoolean());
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md"
                && effect.GetProperty("action").GetString() == "delete"
                && effect.GetProperty("outcome").GetString() == "verified");
        Assert.Equal("publish", result.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("verified", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("removed", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.All(
            result.GetProperty("verification").EnumerateObject(),
            verification => Assert.Equal("verified", verification.Value.GetString()));
        Assert.True(result.GetProperty("packageSourceUnchanged").GetBoolean());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(unownedBefore, workspace.ReadText("workspace-note.md"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(frameworkBefore.GetRawText()),
            JsonNode.Parse(workspace.ReadFrameworkLifecycle().GetRawText())));
        Assert.Empty(workspace.ReadExtensionsLifecycle().GetProperty("packages").EnumerateArray());
        Assert.Empty(workspace.ReadExtensionsLifecycle().GetProperty("paths").EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

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
        var sourceBefore = source.Snapshot();

        var first = await workspace.RunAsync(
        [
            "extension", "remove", "alpha",
            "--automatic", "--json",
        ]);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        using var firstDocument = JsonDocument.Parse(first.StandardOutput);
        var firstResult = firstDocument.RootElement.GetProperty("result");
        var shared = Assert.Single(
            firstResult.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == target);
        Assert.Equal("shared", shared.GetProperty("classification").GetString());
        Assert.Equal("retain-shared", shared.GetProperty("action").GetString());
        Assert.Equal(
            ["alpha"],
            shared.GetProperty("selectedOwnerIds").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.Equal(
            ["beta"],
            shared.GetProperty("remainingOwnerIds").EnumerateArray()
                .Select(value => value.GetString()));
        Assert.True(File.Exists(workspace.Combine(target)));
        var remainingPath = Assert.Single(
            workspace.ReadExtensionsLifecycle().GetProperty("paths").EnumerateArray());
        Assert.Equal(["beta"], remainingPath.GetProperty("owners").EnumerateArray()
            .Select(value => value.GetString()));

        var second = await workspace.RunAsync(
        [
            "extension", "remove", "beta",
            "--automatic", "--json",
        ]);

        Assert.Equal(0, second.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        using var secondDocument = JsonDocument.Parse(second.StandardOutput);
        var secondResult = secondDocument.RootElement.GetProperty("result");
        var final = Assert.Single(
            secondResult.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == target);
        Assert.Equal("unchanged-final-owner", final.GetProperty("classification").GetString());
        Assert.Equal("delete", final.GetProperty("action").GetString());
        Assert.False(File.Exists(workspace.Combine(target)));
        Assert.Empty(workspace.ReadExtensionsLifecycle().GetProperty("paths").EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Fact(
        DisplayName = "Extension Remove keeps changed final-owner content unmanaged without prune"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ChangedFinalOwnerIsKeptAsUnmanagedByDefault()
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
            "--automatic", "--json",
        ]);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        var path = Assert.Single(
            result.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("changed-final-owner", path.GetProperty("classification").GetString());
        Assert.Equal("keep-as-unmanaged", path.GetProperty("action").GetString());
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.managed-divergence");
        Assert.Equal(changed, workspace.ReadText(".agents/toolkit.md"));
        Assert.Empty(workspace.ReadExtensionsLifecycle().GetProperty("paths").EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Fact(
        DisplayName = "Extension Remove deletes changed final-owner content only with same-request prune"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task SameRequestPruneDeletesChangedFinalOwner()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-changed-prune-application");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-changed-prune-application-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);
        workspace.ReplaceText(".agents/toolkit.md", Document("User edit"));
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--prune", "--automatic", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.True(result.GetProperty("prune").GetBoolean());
        Assert.Contains(
            result.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md"
                && effect.GetProperty("action").GetString() == "delete"
                && effect.GetProperty("outcome").GetString() == "verified");
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Empty(workspace.ReadExtensionsLifecycle().GetProperty("paths").EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

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
            "--automatic", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray(),
            region => region.GetProperty("state").GetString() == "changed");
        Assert.False(File.Exists(workspace.Combine(target)));
        Assert.DoesNotContain("toolkit/_toolkit.md", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
        Assert.Equal(sourceBefore, source.Snapshot());
    }

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
            "extension", "remove", "toolkit", "--automatic", "--json",
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
        var result = document.RootElement.GetProperty("result");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", result.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal(
            "already-current",
            result.GetProperty("lifecycle").GetProperty("outcome").GetString());
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
