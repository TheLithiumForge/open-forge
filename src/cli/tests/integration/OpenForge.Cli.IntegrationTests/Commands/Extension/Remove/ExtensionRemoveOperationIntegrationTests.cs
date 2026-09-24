using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove dry-run plans settings exclusion without applying effects"), Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task DryRunPlansExclusionWithoutOwnershipOrEffects()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-operation-no-op");
        var before = workspace.Snapshot();
        var beforeLockInfrastructure = workspace.LockInfrastructureExists;

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--dry-run", "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Null(run.RemainingInput);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = root.GetProperty("data");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        var effects = result.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == ".agents"
            && effect.GetProperty("action").GetString() == "create"
            && effect.GetProperty("outcome").GetString() == "planned");
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == ".agents/open-forge.json"
            && effect.GetProperty("action").GetString() == "record-exclusion"
            && effect.GetProperty("outcome").GetString() == "planned");
        Assert.Equal(["toolkit"], result.GetProperty("packages").EnumerateArray()
            .Select(package => package.GetProperty("id").GetString()));
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
        Assert.False(workspace.LockInfrastructureExists);
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
    }
}
