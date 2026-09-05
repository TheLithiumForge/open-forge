using System.Text;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusFreshnessIntegrationTests
{
    [Fact(DisplayName = "Repeated invocations use fresh facts and each invocation remains internally consistent"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task RepeatedInvocationsUseFreshFactsAndEachInvocationIsInternallyConsistent()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-repeated-facts");
        workspace.SeedLockBytes([0x81, 0x82]);
        var firstRun = await RunStatusAsync(workspace);
        Assert.Equal(0, firstRun.ExitCode);
        Assert.Equal(string.Empty, firstRun.StandardError);
        using var first = StatusIntegrationApplication.ParseJson(firstRun);
        var firstResult = StatusJsonAssertions.Result(first.RootElement);
        var firstExtensions = firstResult.GetProperty("lifecycle").GetProperty("extensions");
        var firstTotal = firstResult.GetProperty("context").GetProperty("totalAvailable");
        var firstBytes = StatusJsonAssertions.Available(firstTotal.GetProperty("utf8Bytes"));
        var firstFiles = StatusJsonAssertions.Available(firstTotal.GetProperty("files"));
        Assert.Equal("trusted", firstExtensions.GetProperty("state").GetString());
        Assert.Empty(firstExtensions.GetProperty("installed").EnumerateArray());
        StatusJsonAssertions.AssertDifference(firstResult.GetProperty("context").GetProperty("startup"));

        workspace.WritePostInstallAgentText(
            ".agents/status-new.md",
            "# New context discovered between invocations\n" + new string('x', 48));
        var lifecycle = StatusLifecycleFixture.Read(workspace);
        var targetBytes = Encoding.UTF8.GetBytes("# Lifecycle bytes observed by the next invocation\n");
        workspace.WritePostInstallAgentBytes(
            StatusIntegrationWorkspace.ExtensionTargetPath,
            targetBytes);
        var extensions = StatusLifecycleFixture.Extensions(
            [new StatusLifecycleFixture.ExtensionSeed(
                "fresh-extension",
                "1.0.0",
                workspace.Combine("missing-fresh-extension-source"),
                [],
                [StatusIntegrationWorkspace.ExtensionTargetPath])],
            [new StatusLifecycleFixture.PathSeed(
                StatusIntegrationWorkspace.ExtensionTargetPath,
                ["fresh-extension"],
                StatusLifecycleFixture.Hash(targetBytes))]);
        StatusLifecycleFixture.WriteSections(
            workspace,
            lifecycle.Framework,
            StatusLifecycleFixture.ExtensionSection(extensions));
        var afterMutation = workspace.SnapshotHashes();
        var lockAfterMutation = workspace.SnapshotLockBytes();
        var recoveryAfterMutation = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var secondRun = await RunStatusAsync(workspace);

        Assert.Equal(3, secondRun.ExitCode);
        Assert.Equal(string.Empty, secondRun.StandardError);
        using var second = StatusIntegrationApplication.ParseJson(secondRun);
        Assert.Equal("incomplete", second.RootElement.GetProperty("status").GetString());
        var secondResult = StatusJsonAssertions.Result(second.RootElement);
        var secondTotal = secondResult.GetProperty("context").GetProperty("totalAvailable");
        Assert.True(StatusJsonAssertions.Available(secondTotal.GetProperty("utf8Bytes")) > firstBytes);
        Assert.True(StatusJsonAssertions.Available(secondTotal.GetProperty("files")) > firstFiles);
        var secondExtensions = secondResult.GetProperty("lifecycle").GetProperty("extensions");
        Assert.Equal("trusted", secondExtensions.GetProperty("state").GetString());
        var installed = Assert.Single(secondExtensions.GetProperty("installed").EnumerateArray());
        Assert.Equal("fresh-extension", installed.GetProperty("id").GetString());
        var target = Assert.Single(
            secondExtensions.GetProperty("managedFiles").GetProperty("targets").EnumerateArray());
        Assert.Equal(StatusIntegrationWorkspace.ExtensionTargetPath, target.GetProperty("path").GetString());
        Assert.Equal("current", target.GetProperty("state").GetString());
        StatusJsonAssertions.AssertDifference(secondResult.GetProperty("context").GetProperty("startup"));
        Assert.Equal(afterMutation, workspace.SnapshotHashes());
        Assert.Equal(lockAfterMutation, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryAfterMutation, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }

    private static Task<StatusIntegrationRun> RunStatusAsync(StatusIntegrationWorkspace workspace)
        => StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--json");
}
