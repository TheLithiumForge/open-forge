using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Remove operation returns a complete no-op when no ownership is recorded"), Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task OperationReturnsNoOpWithoutOwnership()
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
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(beforeLockInfrastructure, workspace.LockInfrastructureExists);
        Assert.False(workspace.LockInfrastructureExists);
    }
}
