using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveRegressionIntegrationTests
{
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
                == "extension-remove.projection-unavailable");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(retainedBytes, File.ReadAllBytes(workspace.Combine(".agents/retained.md")));
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
    }

    [Fact(
        DisplayName = "Extension Remove uses exact-byte fallback for invalid semantic targets with Keep and same-request prune"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task InvalidSemanticTargetUsesExactBytesForKeepAndPrune()
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

        var keep = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(2, keep.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, keep.Status);
        Assert.Equal(string.Empty, keep.StandardError);
        using var keepDocument = JsonDocument.Parse(keep.StandardOutput);
        var keepResult = keepDocument.RootElement.GetProperty("result");
        var keepPath = Assert.Single(
            keepResult.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("changed-final-owner", keepPath.GetProperty("classification").GetString());
        Assert.Equal("keep-as-unmanaged", keepPath.GetProperty("action").GetString());
        Assert.Contains(
            keepResult.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.managed-divergence");
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(invalidBytes, File.ReadAllBytes(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);

        var prune = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--prune", "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, prune.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, prune.Status);
        Assert.Equal(string.Empty, prune.StandardError);
        using var pruneDocument = JsonDocument.Parse(prune.StandardOutput);
        var pruneResult = pruneDocument.RootElement.GetProperty("result");
        var prunePath = Assert.Single(
            pruneResult.GetProperty("paths").EnumerateArray(),
            value => value.GetProperty("path").GetString() == ".agents/toolkit.md");
        Assert.Equal("changed-final-owner", prunePath.GetProperty("classification").GetString());
        Assert.Equal("delete", prunePath.GetProperty("action").GetString());
        Assert.Contains(
            pruneResult.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit.md"
                && effect.GetProperty("action").GetString() == "delete"
                && effect.GetProperty("outcome").GetString() == "planned");
        Assert.Empty(pruneResult.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(invalidBytes, File.ReadAllBytes(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
    }

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
            "# Library\n\n## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "- [Child](child.md) - #Extension\n"
            + "<!-- open-forge:generated-index:end -->\n"
            + "<!-- open-forge:generated-index:end -->\n");
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        var beforeLockInfrastructure = workspace.LockInfrastructureExists;

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "child",
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
                == "extension-remove.generated-region-unsafe");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
    }

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
                == "extension-remove.generated-region-unsafe");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

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
            "--automatic", "--dry-run", "--json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal(
            ["alpha", "beta"],
            result.GetProperty("selection").GetProperty("ids")
                .EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(
            ["alpha", "beta"],
            result.GetProperty("dependencies").GetProperty("packages")
                .EnumerateArray().Select(value => value.GetProperty("id").GetString()));
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Fact(
        DisplayName = "Extension Remove blocks an Extension claim on a Framework target without effects"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task FrameworkTargetOwnershipConflictBlocksPrune()
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

        var lifecycle = JsonNode.Parse(
                workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath))
            ?.AsObject()
            ?? throw new InvalidOperationException("The lifecycle fixture must be an object.");
        var extensions = lifecycle["extensions"]?.AsObject()
            ?? throw new InvalidOperationException("The lifecycle fixture must contain extensions.");
        var package = extensions["packages"]?.AsArray()?.Single()?.AsObject()
            ?? throw new InvalidOperationException("The toolkit lifecycle package is missing.");
        var packagePaths = package["paths"]?.AsArray()
            ?? throw new InvalidOperationException("The toolkit lifecycle paths are missing.");
        packagePaths.Clear();
        packagePaths.Add((JsonNode?)JsonValue.Create(".agents/loader.md"));

        var extensionPath = extensions["paths"]?.AsArray()?.Single()?.AsObject()
            ?? throw new InvalidOperationException("The toolkit lifecycle path is missing.");
        extensionPath["path"] = ".agents/loader.md";
        extensionPath["fingerprintKind"] = "exact-bytes";
        extensionPath["baselineFingerprint"] = Convert.ToHexString(
                SHA256.HashData(File.ReadAllBytes(
                    workspace.Combine(".agents/loader.md"))))
            .ToLowerInvariant();
        workspace.ReplaceText(
            ExtensionInstallIntegrationWorkspace.LifecyclePath,
            lifecycle.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var beforeWorkspace = workspace.Snapshot();
        var beforeLifecycle = workspace.ReadText(
            ExtensionInstallIntegrationWorkspace.LifecyclePath);
        var beforeFramework = workspace.ReadFrameworkLifecycle().GetRawText();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--prune", "--automatic", "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(
            result.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.ownership-conflict");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeLifecycle, workspace.ReadText(
            ExtensionInstallIntegrationWorkspace.LifecyclePath));
        Assert.Equal(beforeFramework, workspace.ReadFrameworkLifecycle().GetRawText());
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
