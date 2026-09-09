using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInitProcessTests
{
    [Fact(DisplayName = "Published generic Route Init previews applies and converges"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task GenericDryRunApplyAndRepeatNoOpFormOneRealJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateGeneric();
        string[] arguments = ["route", "init", "docs", "--description", "Project documents", "--tag=Documentation", "--json", "--workspace", workspace.Path];
        var preview = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--dry-run"]);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        Assert.Equal("dry-run", previewDocument.RootElement.GetProperty("result").GetProperty("mode").GetString());
        workspace.AssertNoLockInfrastructure();

        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("verified", appliedDocument.RootElement.GetProperty("result").GetProperty("verification").GetString());
        Assert.True(File.Exists(workspace.Combine(PublishedRouteInitWorkspace.GenericTargetPath)));
        var scaffold = await workspace.ReadTextAsync(PublishedRouteInitWorkspace.GenericTargetPath, TestContext.Current.CancellationToken);
        Assert.Contains("Project documents", scaffold, StringComparison.Ordinal);
        Assert.Contains("# docs", scaffold, StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();

        var noOp = await RunWithoutWritesAsync(target, workspace, ["route", "init", "docs", "--json", "--workspace", workspace.Path]);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        Assert.Empty(noOpDocument.RootElement.GetProperty("result").GetProperty("effects").EnumerateArray());
    }

    [Fact(DisplayName = "Published Framework Route Init reuses Install and converges one sparse scope"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task FrameworkDryRunApplyAndRepeatNoOpPreserveOwnership()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateFramework();
        var installed = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
            ["install", "--automatic", "--json", "--workspace", workspace.Path], workspace.ProcessEnvironment);
        Assert.Equal(0, installed.ExitCode);
        Assert.Equal(string.Empty, installed.StandardError);
        string[] arguments = ["route", "init", "memory/Mobile App/crystallized/documents", "--framework", "--json", "--workspace", workspace.Path];
        var preview = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--dry-run"]);
        Assert.Equal(2, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        Assert.Equal("framework", previewDocument.RootElement.GetProperty("result").GetProperty("scaffold").GetString());

        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(2, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("verified", appliedDocument.RootElement.GetProperty("result").GetProperty("verification").GetString());
        Assert.True(File.Exists(workspace.Combine(PublishedRouteInitWorkspace.FrameworkFinalPath)));
        workspace.AssertPersistentExternalLock();

        var noOp = await RunWithoutWritesAsync(target, workspace, arguments);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        var result = noOpDocument.RootElement.GetProperty("result");
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("already-current", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
    }

    [Fact(DisplayName = "Published Framework Route Init refuses a workspace without trusted Install"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task FrameworkWithoutTrustedInstallIsBlocked()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            ["route", "init", "memory/crystallized/documents", "--framework", "--workspace", workspace.Path], workspace.ProcessEnvironment);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: blocked", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("establish a trusted current Framework installation", result.StandardError, StringComparison.OrdinalIgnoreCase);
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteInitWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
