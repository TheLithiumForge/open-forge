using System.Text;
using System.Text.Json;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusWorkspaceIntegrationTests
{
    [Fact(DisplayName = "Complete installed workspace reports fresh producer facts and preserves every external byte"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task CompleteInstalledWorkspaceReportsFreshProducerFactsAndPreservesAllBytes()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-complete-installed");
        workspace.SeedLockBytes([0x13, 0x37, 0x00, 0xff]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--json");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        StatusJsonAssertions.CompleteInstalled(document.RootElement);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }

    [Fact(DisplayName = "Safely uninstalled workspace reports not-applicable status facts and preserves every byte"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task SafelyUninstalledWorkspaceReportsNotApplicableStatusFactsAndPreservesAllBytes()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-uninstalled");
        workspace.SeedLockBytes([0x21, 0x22, 0x23]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--json");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var root = document.RootElement;
        Assert.Equal("complete", root.GetProperty("status").GetString());
        var result = StatusJsonAssertions.Result(root);
        var installation = result.GetProperty("installation");
        Assert.Equal("uninstalled", installation.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, installation.GetProperty("entryPath").ValueKind);
        Assert.Equal(JsonValueKind.Null, installation.GetProperty("loaderPath").ValueKind);

        var context = result.GetProperty("context");
        StatusJsonAssertions.MeasurementAvailable(context.GetProperty("startup").GetProperty("initial"));
        StatusJsonAssertions.MeasurementNotApplicable(context.GetProperty("startup").GetProperty("current"));
        StatusJsonAssertions.MeasurementNotApplicable(context.GetProperty("startup").GetProperty("difference"));
        StatusJsonAssertions.MeasurementAvailable(context.GetProperty("totalAvailable"));
        StatusJsonAssertions.MeasurementNotApplicable(context.GetProperty("continuity"));
        StatusJsonAssertions.NotApplicable(context.GetProperty("startupPercentage"));

        var structure = result.GetProperty("structure");
        StatusJsonAssertions.NotApplicable(structure.GetProperty("rootCategories").GetProperty("count"));
        var generated = structure.GetProperty("generatedNavigation").EnumerateArray().ToArray();
        Assert.NotEmpty(generated);
        Assert.All(generated, item => Assert.Equal("not-applicable", item.GetProperty("state").GetString()));
        var lifecycle = result.GetProperty("lifecycle");
        Assert.Equal("absent", lifecycle.GetProperty("framework").GetProperty("state").GetString());
        Assert.Equal("not-applicable", lifecycle.GetProperty("framework").GetProperty("sourceAvailability").GetString());
        Assert.Empty(lifecycle.GetProperty("framework").GetProperty("targets").EnumerateArray());
        Assert.Equal("absent", lifecycle.GetProperty("extensions").GetProperty("state").GetString());
        Assert.Empty(lifecycle.GetProperty("extensions").GetProperty("installed").EnumerateArray());
        Assert.Empty(lifecycle.GetProperty("extensions").GetProperty("managedFiles").GetProperty("targets").EnumerateArray());
        StatusJsonAssertions.AvailableZero(result.GetProperty("recovery").GetProperty("verifiedFinals"));
        StatusJsonAssertions.AvailableZero(result.GetProperty("recovery").GetProperty("incompleteDrafts"));
        Assert.Empty(result.GetProperty("recovery").GetProperty("candidates").EnumerateArray());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }

    [Fact(DisplayName = "Malformed context and unavailable lifecycle sources retain safe incomplete facts without writes"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task MalformedContextAndSourceUnavailableLifecycleRetainSafeFactsAsIncompleteWithoutWrites()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-incomplete-sources");
        workspace.WriteText(
            ".agents/loader.md",
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Entries = "- [Broken](broken.md) - #Broken",
                Prefix = "# Loader",
            }));
        workspace.WriteBytes(".agents/broken.md", [0xff, 0xfe, 0xfd]);
        var extensionBytes = Encoding.UTF8.GetBytes("# Toolkit\n");
        workspace.WriteBytes(StatusIntegrationWorkspace.ExtensionTargetPath, extensionBytes);
        var missingSource = workspace.Combine("missing-extension-source");
        StatusLifecycleFixture.Write(
            workspace,
            framework: null,
            extensions: StatusLifecycleFixture.Extensions(
                [new StatusLifecycleFixture.ExtensionSeed(
                    "toolkit",
                    "1.0.0",
                    missingSource,
                    [],
                    [StatusIntegrationWorkspace.ExtensionTargetPath])],
                [new StatusLifecycleFixture.PathSeed(
                    StatusIntegrationWorkspace.ExtensionTargetPath,
                    ["toolkit"],
                    StatusLifecycleFixture.Hash(extensionBytes))]));
        workspace.SeedLockBytes([0x41, 0x42, 0x43]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--json");

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var result = StatusJsonAssertions.Result(root);
        var utf8 = result.GetProperty("context").GetProperty("totalAvailable").GetProperty("utf8Bytes");
        Assert.Equal("unavailable", utf8.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, utf8.GetProperty("value").ValueKind);

        var extensions = result.GetProperty("lifecycle").GetProperty("extensions");
        Assert.Equal("trusted", extensions.GetProperty("state").GetString());
        Assert.Equal("unavailable", extensions.GetProperty("sourceAvailability").GetString());
        var installed = Assert.Single(extensions.GetProperty("installed").EnumerateArray());
        Assert.Equal("toolkit", installed.GetProperty("id").GetString());
        Assert.Equal("1.0.0", installed.GetProperty("version").GetString());
        Assert.Equal(missingSource, installed.GetProperty("source").GetString());
        Assert.Equal("unavailable", installed.GetProperty("sourceAvailability").GetString());
        var target = Assert.Single(extensions.GetProperty("managedFiles").GetProperty("targets").EnumerateArray());
        Assert.Equal("current", target.GetProperty("state").GetString());
        Assert.Equal(["toolkit"], target.GetProperty("owners").EnumerateArray().Select(owner => owner.GetString()));
        Assert.Contains(result.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "context-inventory-incomplete");
        Assert.Contains(result.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "extension-source-unavailable");
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }
}
