using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedInstallProcessTests
{
    [Fact(DisplayName = "Published state commands preserve malformed legacy files as unrelated content"), Trait("Feature", "state-retirement"), Trait("Evidence", "EndToEnd")]
    public async Task LegacyFilesAreNeitherReadNorDeleted()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        workspace.SeedLegacyLeftovers();
        string[] legacy = [".agents/open-forge.lifecycle.json", ".agents/open-forge.libraries.json"];
        var before = legacy.Select(path => File.ReadAllBytes(workspace.Combine(path))).ToArray();
        string[][] commands = [["install", "--automatic"], ["update", "--automatic"], ["status", "--format=json"],
            ["doctor", "--format=json"], ["extension", "list", "--format=json"], ["library", "list", "--format=json"]];
        foreach (var command in commands)
        {
            var result = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, command, workspace.ProcessEnvironment);
            Assert.True(result.ExitCode == 0, string.Join(" ", command) + "\n" + result.StandardOutput + result.StandardError);
            for (var index = 0; index < legacy.Length; index++)
                Assert.Equal(before[index], File.ReadAllBytes(workspace.Combine(legacy[index])));
        }
        Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
    }

    [Fact(DisplayName = "Published Install JSON dry run plans effects without writing"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedJsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, workspace.Path, workspace.SnapshotState,
            ["install", "--dry-run", "--format=json"], workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        var root = document.RootElement;
        var plan = root.GetProperty("data");
        Assert.Equal("dry-run", plan.GetProperty("mode").GetString());
        Assert.NotEmpty(root.GetProperty("effects").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published automatic Install applies and converges to a verified no-op"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedApplyConvergesToVerifiedNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        string[] arguments = ["install", "--automatic", "--detail=full"];
        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("All written targets were verified.", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(PublishedInstallWorkspace.EmbeddedPayloadPaths, workspace.InstalledPayloadPaths());
        workspace.AssertPersistentExternalLock();
        var before = workspace.SnapshotState();

        var noOp = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("Open Forge is already installed and current. Nothing to do.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("All written targets were verified.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());
    }

    [Fact(DisplayName = "Published redirected human Install requires automatic without prompting or writing"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRedirectedHumanWriteIsInvalidAndSilent()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["install"],
            workspace.ProcessEnvironment);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.DoesNotContain("Apply this Install plan?", result.StandardError, StringComparison.Ordinal);
        Assert.Contains(
            "Install needs confirmation, and this session cannot ask.",
            result.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("open-forge install --automatic", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

}
