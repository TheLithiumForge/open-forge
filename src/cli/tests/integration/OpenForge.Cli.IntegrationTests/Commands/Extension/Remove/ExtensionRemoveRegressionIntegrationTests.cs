using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveRegressionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove reports unavailable topology for invalid UTF-8 retained sources without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task InvalidUtf8RetainedSourceMakesProjectionIncompleteAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-invalid-utf8-retained-source");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-invalid-utf8-retained-source-source");
        source.AddPackage(
            "alpha",
            [],
            (".agents/selected.md", Document("Selected")));
        source.AddPackage(
            "beta",
            [],
            (".agents/retained.md", Document("Retained")));
        await InstallAllAsync(workspace, source);

        byte[] retainedBytes = [0x23, 0x20, 0x52, 0x65, 0x74, 0x61, 0x69, 0x6E, 0x65, 0x64, 0x20, 0xFF, 0x0A];
        File.WriteAllBytes(workspace.Combine(".agents/retained.md"), retainedBytes);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        var beforeLockInfrastructure = workspace.LockInfrastructureExists;

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "alpha",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.projection-unavailable");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(retainedBytes, File.ReadAllBytes(workspace.Combine(".agents/retained.md")));
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove uses exact-byte fallback for invalid semantic targets with deletion and recovery planning"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task InvalidSemanticTargetUsesExactBytesForDeletion()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-invalid-semantic-target");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-invalid-semantic-target-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);

        byte[] invalidBytes = [0x23, 0x20, 0x54, 0x6F, 0x6F, 0x6C, 0x6B, 0x69, 0x74, 0x20, 0xFF, 0x0A];
        File.WriteAllBytes(workspace.Combine(".agents/toolkit.md"), invalidBytes);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        var beforeLockInfrastructure = workspace.LockInfrastructureExists;

        var prune = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, prune.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, prune.Status);
        Assert.Equal(string.Empty, prune.StandardError);
        using var pruneDocument = JsonDocument.Parse(prune.StandardOutput);
        var pruneResult = pruneDocument.RootElement.GetProperty("data");
        var prunePath = Assert.Single(
            pruneResult.GetProperty("effects").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("delete", prunePath.GetProperty("action").GetString());
        Assert.Contains(
            pruneResult.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md"
                && effect.GetProperty("action").GetString() == "delete"
                && effect.GetProperty("outcome").GetString() == "planned");
        Assert.Empty(pruneDocument.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(invalidBytes, File.ReadAllBytes(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove blocks a malformed affected generated region without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task MalformedAffectedGeneratedRegionBlocksRemovalAndWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-malformed-affected-generated-region");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-malformed-affected-generated-region-source");
        source.AddPackage(
            "host",
            [],
            (".agents/library/_library.md", Document("Library")));
        source.AddPackage(
            "child",
            [],
            (".agents/library/child.md", Document("Child")));
        await InstallAllAsync(workspace, source);

        workspace.ReplaceText(
            ".agents/library/_library.md",
            OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
                "Library",
                ["Extension"],
                "# Library\n\n## Entries\n\n- [Child](child.md) - #Extension\n\n## Entries\n"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        var beforeLockInfrastructure = workspace.LockInfrastructureExists;

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "child",
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
                == "extension-remove.generated-region-unsafe");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove blocks a retained generated route host without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task RetainedGeneratedDescendantBlocksRouteHostRemoval()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-generated-descendant-blocked");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-generated-descendant-blocked-source");
        source.AddPackage(
            "alpha",
            [],
            (".agents/library/_library.md", Document("Library")));
        source.AddPackage(
            "beta",
            [],
            (".agents/library/child.md", Document("Child")));
        await InstallAllAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "alpha",
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
                == "extension-remove.generated-region-unsafe");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove accepts a complete descendant closure in a write-free preview"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task CompleteGeneratedDescendantClosureIsAcceptedByDryRun()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-generated-descendant-dry-run");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-generated-descendant-dry-run-source");
        source.AddPackage(
            "alpha",
            [],
            (".agents/library/_library.md", Document("Library")));
        source.AddPackage(
            "beta",
            [],
            (".agents/library/child.md", Document("Child")));
        await InstallAllAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "alpha", "beta",
            "--automatic", "--dry-run", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("data");
        Assert.Equal(
            ["alpha", "beta"],
            result.GetProperty("packages").EnumerateArray()
                .Select(value => value.GetProperty("id").GetString()));
        Assert.Equal(
            ["alpha", "beta"],
            result.GetProperty("packages").EnumerateArray()
                .Select(value => value.GetProperty("id").GetString()));
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove blocks an Extension claim on a Framework target without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task FrameworkTargetOwnershipConflictBlocksRemoval()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-framework-target-conflict");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-framework-target-conflict-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAsync(workspace, source);

        const string ownershipPath = ".agents/open-forge.lock.json";
        var ownership = JsonNode.Parse(workspace.ReadText(ownershipPath))!;
        ownership["extensions"]![0]!["paths"] = new JsonArray(JsonValue.Create(".agents/loader.md"));
        workspace.ReplaceText(ownershipPath, ownership.ToJsonString());

        var beforeWorkspace = workspace.Snapshot();
        var beforeLifecycle = workspace.ReadText(
            ExtensionInstallIntegrationWorkspace.OwnershipPath);
        var beforeFramework = workspace.ReadFrameworkOwnership().GetRawText();
        var beforeSource = source.Snapshot();

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
                == "extension-remove.ownership-conflict");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeLifecycle, workspace.ReadText(
            ExtensionInstallIntegrationWorkspace.OwnershipPath));
        Assert.Equal(beforeFramework, workspace.ReadFrameworkOwnership().GetRawText());
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
