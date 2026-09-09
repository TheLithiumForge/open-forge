using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedInstallProcessTests
{
    [Fact(DisplayName = "Published Install JSON dry run plans effects without writing"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedJsonDryRunIsReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, workspace.Path, workspace.SnapshotState,
            ["install", "--dry-run", "--json"], workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var plan = document.RootElement.GetProperty("result");
        Assert.Equal("dry-run", plan.GetProperty("mode").GetString());
        Assert.NotEmpty(plan.GetProperty("effects").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published automatic Install applies and converges to a verified no-op"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedApplyConvergesToVerifiedNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        string[] arguments = ["install", "--automatic"];
        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Verification: verified", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(PublishedInstallWorkspace.EmbeddedPayloadPaths, workspace.InstalledPayloadPaths());
        workspace.AssertPersistentExternalLock();
        var before = workspace.SnapshotState();

        var noOp = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("Classification: trusted-exact", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Effects: 0", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Verification: verified", noOp.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("Status: invalid", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("open-forge install --automatic", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

}
