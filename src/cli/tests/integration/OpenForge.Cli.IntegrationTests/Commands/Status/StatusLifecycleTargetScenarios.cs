using System.Text;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusLifecycleTargetScenarios
{
    internal static async Task<string> ReadGeneratedNavigationStateAsync(string scenario)
    {
        if (scenario == "not-applicable")
        {
            using var uninstalled = StatusIntegrationWorkspace.Create("status-generated-not-applicable");
            var notApplicableRun = await RunStatusAsync(uninstalled);
            Assert.Equal(0, notApplicableRun.ExitCode);
            Assert.Equal(string.Empty, notApplicableRun.StandardError);
            using var notApplicableDocument = StatusIntegrationApplication.ParseJson(notApplicableRun);
            var notApplicableGenerated = StatusJsonAssertions.Result(notApplicableDocument.RootElement)
                .GetProperty("structure").GetProperty("generatedNavigation").EnumerateArray();
            var notApplicableTarget = Assert.Single(
                notApplicableGenerated,
                item => item.GetProperty("path").GetString() == StatusIntegrationWorkspace.GeneratedTargetPath);
            Assert.Equal("not-applicable", notApplicableTarget.GetProperty("state").GetString());
            return Assert.IsType<string>(notApplicableTarget.GetProperty("state").GetString());
        }

        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync($"status-generated-{scenario}");
        using var outside = TemporaryWorkspace.Create($"status-generated-outside-{scenario}");
        switch (scenario)
        {
            case "current":
                break;
            case "changed":
                workspace.OverwriteInstalledText(
                    StatusIntegrationWorkspace.GeneratedTargetPath,
                    GeneratedDocument("- [Stale](stale.md) - #Stale"));
                break;
            case "missing":
                workspace.DeleteInstalledTarget(StatusIntegrationWorkspace.GeneratedTargetPath);
                break;
            case "unavailable":
                workspace.OverwriteInstalledBytes(
                    StatusIntegrationWorkspace.GeneratedTargetPath,
                    [0xff, 0xfe, 0xfd]);
                break;
            case "blocked":
                outside.WriteText("outside.md", "# Outside\n");
                workspace.ReplaceInstalledTargetWithLink(
                    StatusIntegrationWorkspace.GeneratedTargetPath,
                    outside.Combine("outside.md"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario);
        }

        var before = workspace.SnapshotHashes();
        workspace.SeedLockBytes([0x31, 0x32, 0x33]);
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());
        var run = await RunStatusAsync(workspace);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var generated = StatusJsonAssertions.Result(document.RootElement)
            .GetProperty("structure").GetProperty("generatedNavigation").EnumerateArray();
        var target = Assert.Single(generated, item => item.GetProperty("path").GetString() == StatusIntegrationWorkspace.GeneratedTargetPath);
        var state = Assert.IsType<string>(target.GetProperty("state").GetString());
        Assert.Equal(scenario, state);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
        return state;
    }

    internal static async Task<string> ReadFrameworkTargetStateAsync(string scenario)
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync($"status-framework-target-{scenario}");
        using var outside = TemporaryWorkspace.Create($"status-framework-outside-{scenario}");
        switch (scenario)
        {
            case "current":
                break;
            case "changed":
                workspace.OverwriteInstalledText(
                    StatusIntegrationWorkspace.GeneratedTargetPath,
                    GeneratedDocument("- [Changed](changed.md) - #Changed"));
                break;
            case "missing":
                workspace.DeleteInstalledTarget(StatusIntegrationWorkspace.GeneratedTargetPath);
                break;
            case "unavailable":
                workspace.ReplaceInstalledTargetWithDirectory(
                    StatusIntegrationWorkspace.GeneratedTargetPath);
                break;
            case "blocked":
                outside.WriteText("outside.md", "# Outside\n");
                workspace.ReplaceInstalledTargetWithLink(
                    StatusIntegrationWorkspace.GeneratedTargetPath,
                    outside.Combine("outside.md"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario);
        }

        var before = workspace.SnapshotHashes();
        workspace.SeedLockBytes([0x61, 0x62, 0x63]);
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());
        var run = await RunStatusAsync(workspace);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var targets = StatusJsonAssertions.Result(document.RootElement)
            .GetProperty("lifecycle").GetProperty("framework").GetProperty("targets").EnumerateArray();
        var target = Assert.Single(
            targets,
            item => item.GetProperty("path").GetString() == StatusIntegrationWorkspace.GeneratedTargetPath
                && item.GetProperty("kind").GetString() == "file");
        var state = Assert.IsType<string>(target.GetProperty("state").GetString());
        Assert.Equal(scenario, state);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
        return state;
    }

    internal static async Task<string> ReadExtensionTargetStateAsync(string scenario)
    {
        using var workspace = StatusIntegrationWorkspace.Create($"status-extension-target-{scenario}");
        using var outside = TemporaryWorkspace.Create($"status-extension-outside-{scenario}");
        var bytes = Encoding.UTF8.GetBytes("# Extension target\n");
        workspace.WriteBytes(StatusIntegrationWorkspace.ExtensionTargetPath, bytes);
        StatusLifecycleFixture.Write(
            workspace,
            framework: null,
            extensions: StatusLifecycleFixture.Extensions(
                [new StatusLifecycleFixture.ExtensionSeed(
                    "toolkit", "1.0.0", workspace.Combine("missing-extension-source"), [],
                    [StatusIntegrationWorkspace.ExtensionTargetPath])],
                [new StatusLifecycleFixture.PathSeed(
                    StatusIntegrationWorkspace.ExtensionTargetPath,
                    ["toolkit"],
                    StatusLifecycleFixture.Hash(bytes))]));
        switch (scenario)
        {
            case "current":
                break;
            case "changed":
                workspace.ReplaceText(StatusIntegrationWorkspace.ExtensionTargetPath, "# Changed extension target\n");
                break;
            case "missing":
                workspace.DeleteTarget(StatusIntegrationWorkspace.ExtensionTargetPath);
                break;
            case "unavailable":
                workspace.DeleteOrReplaceWithDirectory(StatusIntegrationWorkspace.ExtensionTargetPath);
                break;
            case "blocked":
                outside.WriteText("outside.md", "# Outside\n");
                workspace.DeleteOrReplaceWithLink(StatusIntegrationWorkspace.ExtensionTargetPath, outside.Combine("outside.md"));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario);
        }

        var before = workspace.SnapshotHashes();
        workspace.SeedLockBytes([0x71, 0x72, 0x73]);
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());
        var run = await RunStatusAsync(workspace);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var targets = StatusJsonAssertions.Result(document.RootElement)
            .GetProperty("lifecycle").GetProperty("extensions").GetProperty("managedFiles").GetProperty("targets").EnumerateArray();
        var target = Assert.Single(targets, item => item.GetProperty("path").GetString() == StatusIntegrationWorkspace.ExtensionTargetPath);
        var state = Assert.IsType<string>(target.GetProperty("state").GetString());
        Assert.Equal(scenario, state);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
        return state;
    }

    private static string GeneratedDocument(string entries)
        => OpenForgeDocumentSeed.Metadata(
            "Memory",
            ["Memory"],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = entries,
                Prefix = "# Memory",
            }));

    private static Task<StatusIntegrationRun> RunStatusAsync(StatusIntegrationWorkspace workspace)
        => StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--json");
}
