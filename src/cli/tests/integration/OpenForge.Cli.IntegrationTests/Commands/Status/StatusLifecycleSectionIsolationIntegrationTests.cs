using System.Text;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusLifecycleSectionIsolationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Framework and Extension ignore malformed legacy sections and read current lock claims"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task FrameworkAndExtensionIgnoreMalformedLegacySections()
    {
        using var frameworkWorkspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-framework-section-isolation");
        frameworkWorkspace.WritePostInstallAgentText(StatusIntegrationWorkspace.LifecyclePath, "{ malformed legacy extensions }");
        var frameworkBefore = frameworkWorkspace.SnapshotHashes();

        var frameworkRun = await RunStatusAsync(frameworkWorkspace);

        Assert.Equal(0, frameworkRun.ExitCode);
        Assert.Equal(string.Empty, frameworkRun.StandardError);
        using var frameworkDocument = StatusIntegrationApplication.ParseJson(frameworkRun);
        var frameworkResult = StatusJsonAssertions.Result(frameworkDocument.RootElement);
        var framework = frameworkResult.GetProperty("frameworkFiles");
        var malformedExtensions = frameworkResult.GetProperty("extensions");
        Assert.NotEmpty(framework.EnumerateArray());
        Assert.Empty(malformedExtensions.EnumerateArray());
        Assert.DoesNotContain(
            frameworkDocument.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "status.extension-lifecycle-incomplete");
        Assert.Equal(frameworkBefore, frameworkWorkspace.SnapshotHashes());

        using var extensionWorkspace = StatusIntegrationWorkspace.Create(
            "status-extension-section-isolation");
        var targetBytes = Encoding.UTF8.GetBytes("# Extension section remains independently readable\n");
        extensionWorkspace.WriteBytes(StatusIntegrationWorkspace.ExtensionTargetPath, targetBytes);
        var extensions = StatusLifecycleFixture.Extensions(
            [new StatusLifecycleFixture.ExtensionSeed(
                "isolated-extension",
                "2.0.0",
                extensionWorkspace.Combine("missing-isolated-extension-source"),
                [],
                [StatusIntegrationWorkspace.ExtensionTargetPath])]);
        StatusLifecycleFixture.Write(extensionWorkspace, null, extensions);
        extensionWorkspace.WriteText(StatusIntegrationWorkspace.LifecyclePath, "{ malformed legacy framework }");
        var extensionBefore = extensionWorkspace.SnapshotHashes();

        var extensionRun = await RunStatusAsync(extensionWorkspace);

        Assert.Equal(3, extensionRun.ExitCode);
        Assert.Equal(string.Empty, extensionRun.StandardError);
        using var extensionDocument = StatusIntegrationApplication.ParseJson(extensionRun);
        var extensionResult = StatusJsonAssertions.Result(extensionDocument.RootElement);
        var malformedFramework = extensionResult.GetProperty("frameworkFiles");
        var extension = extensionResult.GetProperty("extensions");
        Assert.Empty(malformedFramework.EnumerateArray());
        var installed = Assert.Single(extension.EnumerateArray());
        Assert.Equal("isolated-extension", installed.GetProperty("id").GetString());
        var target = Assert.Single(installed.GetProperty("files").EnumerateArray());
        Assert.Equal(StatusIntegrationWorkspace.ExtensionTargetPath, target.GetProperty("path").GetString());
        Assert.Equal("unavailable", target.GetProperty("state").GetString());
        Assert.Contains(
            extensionDocument.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "status.framework-ownership-observation");
        Assert.Equal(extensionBefore, extensionWorkspace.SnapshotHashes());
    }

    private static Task<StatusIntegrationRun> RunStatusAsync(StatusIntegrationWorkspace workspace)
        => StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--format", "json",
            "--detail", "full");
}
