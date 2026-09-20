using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

[Trait("Feature", "extension-update"), Trait("Evidence", "IntegrationSafety")]
public sealed class ExtensionUpdateOwnershipSafetyIntegrationTests
{
    private const string Target = ".agents/toolkit.txt";
    private const string LockPath = ".agents/open-forge.lock.json";

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update does not infer effects from ambiguous ownership")]
    [InlineData("duplicate")]
    [InlineData("cycle")]
    [InlineData("alias")]
    public async Task AmbiguousOwnershipHasNoEffects(string kind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("update-ambiguous-ownership");
        using var source = ExtensionInstallCatalogue.Create("update-ambiguous-source");
        await InstallAsync(workspace, source);
        var document = JsonNode.Parse(workspace.ReadText(LockPath))!;
        var extensions = document["extensions"]!.AsArray();
        if (kind == "cycle") extensions[0]!["dependencies"] = new JsonArray(JsonValue.Create("toolkit"));
        else
        {
            var other = extensions[0]!.DeepClone();
            if (kind == "alias")
            {
                other["id"] = "other";
                other["paths"] = new JsonArray(JsonValue.Create(".agents/TOOLKIT.txt"));
            }
            extensions.Add(other);
        }
        workspace.ReplaceText(LockPath, document.ToJsonString());
        var before = workspace.Snapshot();
        var run = await UpdateAsync(workspace, source);
        Assert.Contains("extension-update.lifecycle-observation", run.StandardOutput, StringComparison.Ordinal);
        using var result = JsonDocument.Parse(run.StandardOutput);
        Assert.Empty(result.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update prune cannot delete a stale claim outside the shared allow list")]
    public async Task OutOfBoundaryClaimIsNotDeleted()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("update-unallowed-claim");
        using var source = ExtensionInstallCatalogue.Create("update-unallowed-source");
        await InstallAsync(workspace, source);
        var document = JsonNode.Parse(workspace.ReadText(LockPath))!;
        document["extensions"]![0]!["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create("workspace-note.md"));
        workspace.ReplaceText(LockPath, document.ToJsonString());
        var before = workspace.Snapshot();
        var run = await UpdateAsync(workspace, source);
        Assert.Equal(5, run.ExitCode);
        Assert.Contains("extension-update.permission-required", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update ignores old state files and stored Framework release metadata")]
    public async Task OldFilesAndFrameworkMetadataDoNotGateUpdate()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("update-old-state");
        using var source = ExtensionInstallCatalogue.Create("update-old-source");
        await InstallAsync(workspace, source);
        workspace.CreateOccupant(".agents/open-forge.lifecycle.json", "old user bytes");
        workspace.CreateOccupant(".agents/open-forge.libraries.json", "{ unrelated malformed bytes");
        var document = JsonNode.Parse(workspace.ReadText(LockPath))!;
        document["framework"]!["source"]!["version"] = "older-release";
        workspace.ReplaceText(LockPath, document.ToJsonString());
        var loader = workspace.ReadText(".agents/loader.md");
        workspace.ReplaceText(".agents/loader.md", loader.Replace("## Entries", "Authored workspace note.\n\n## Entries", StringComparison.Ordinal));
        source.ReplacePayload("toolkit", Target, "updated bytes\n");
        var run = await UpdateAsync(workspace, source);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal("updated bytes\n", workspace.ReadText(Target));
        Assert.Equal("old user bytes", workspace.ReadText(".agents/open-forge.lifecycle.json"));
        Assert.Equal("{ unrelated malformed bytes", workspace.ReadText(".agents/open-forge.libraries.json"));
        Assert.Contains("Authored workspace note.", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
    }

    private static Task<ExtensionInstallRun> UpdateAsync(ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source)
        => workspace.RunAsync(["extension", "update", "toolkit", "--source", source.Path, "--prune", "--automatic", "--format", "json"]);

    private static async Task InstallAsync(ExtensionInstallIntegrationWorkspace workspace, ExtensionInstallCatalogue source)
    {
        await workspace.SeedFrameworkAsync();
        source.AddPackage("toolkit", [], (Target, "installed bytes\n"));
        var run = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, run.ExitCode);
    }
}
