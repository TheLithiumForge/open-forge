using System.Text;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusLifecycleSectionIsolationIntegrationTests
{
    [Fact(DisplayName = "Framework and Extension lifecycle contributors decode only their supplied snapshot section"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task FrameworkAndExtensionLifecycleContributorsDecodeOnlyTheirSuppliedSnapshotSection()
    {
        using var frameworkWorkspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-framework-section-isolation");
        var installedLifecycle = StatusLifecycleFixture.Read(frameworkWorkspace);
        StatusLifecycleFixture.WriteSections(
            frameworkWorkspace,
            installedLifecycle.Framework,
            StatusLifecycleFixture.MalformedSection());
        var frameworkBefore = frameworkWorkspace.SnapshotHashes();

        var frameworkRun = await RunStatusAsync(frameworkWorkspace);

        Assert.Equal(3, frameworkRun.ExitCode);
        Assert.Equal(string.Empty, frameworkRun.StandardError);
        using var frameworkDocument = StatusIntegrationApplication.ParseJson(frameworkRun);
        var frameworkResult = StatusJsonAssertions.Result(frameworkDocument.RootElement);
        var framework = frameworkResult.GetProperty("lifecycle").GetProperty("framework");
        var malformedExtensions = frameworkResult.GetProperty("lifecycle").GetProperty("extensions");
        Assert.Equal("trusted", framework.GetProperty("state").GetString());
        Assert.NotEmpty(framework.GetProperty("targets").EnumerateArray());
        Assert.Equal("incomplete", malformedExtensions.GetProperty("state").GetString());
        Assert.Empty(malformedExtensions.GetProperty("installed").EnumerateArray());
        Assert.Contains(
            frameworkResult.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-lifecycle-incomplete");
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
                [StatusIntegrationWorkspace.ExtensionTargetPath])],
            [new StatusLifecycleFixture.PathSeed(
                StatusIntegrationWorkspace.ExtensionTargetPath,
                ["isolated-extension"],
                StatusLifecycleFixture.Hash(targetBytes))]);
        StatusLifecycleFixture.WriteSections(
            extensionWorkspace,
            StatusLifecycleFixture.MalformedSection(),
            StatusLifecycleFixture.ExtensionSection(extensions));
        var extensionBefore = extensionWorkspace.SnapshotHashes();

        var extensionRun = await RunStatusAsync(extensionWorkspace);

        Assert.Equal(3, extensionRun.ExitCode);
        Assert.Equal(string.Empty, extensionRun.StandardError);
        using var extensionDocument = StatusIntegrationApplication.ParseJson(extensionRun);
        var extensionResult = StatusJsonAssertions.Result(extensionDocument.RootElement);
        var malformedFramework = extensionResult.GetProperty("lifecycle").GetProperty("framework");
        var extension = extensionResult.GetProperty("lifecycle").GetProperty("extensions");
        Assert.Equal("incomplete", malformedFramework.GetProperty("state").GetString());
        Assert.Empty(malformedFramework.GetProperty("targets").EnumerateArray());
        Assert.Equal("trusted", extension.GetProperty("state").GetString());
        var installed = Assert.Single(extension.GetProperty("installed").EnumerateArray());
        Assert.Equal("isolated-extension", installed.GetProperty("id").GetString());
        var target = Assert.Single(extension.GetProperty("managedFiles").GetProperty("targets").EnumerateArray());
        Assert.Equal(StatusIntegrationWorkspace.ExtensionTargetPath, target.GetProperty("path").GetString());
        Assert.Equal("current", target.GetProperty("state").GetString());
        Assert.Contains(
            extensionResult.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "framework-lifecycle-incomplete");
        Assert.Equal(extensionBefore, extensionWorkspace.SnapshotHashes());
    }

    private static Task<StatusIntegrationRun> RunStatusAsync(StatusIntegrationWorkspace workspace)
        => StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--json");
}
