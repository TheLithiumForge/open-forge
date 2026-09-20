using System.Text;
using System.Text.Json;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusWorkspaceIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Complete installed workspace reports fresh producer facts and preserves every external byte"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task CompleteInstalledWorkspaceReportsFreshProducerFactsAndPreservesAllBytes()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync("status-complete-installed");
        workspace.SeedLockBytes([0x13, 0x37, 0x00, 0xff]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--format", "json", "--detail", "full");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        StatusJsonAssertions.CompleteInstalled(document.RootElement);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Safely uninstalled workspace reports not-applicable status facts and preserves every byte"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task SafelyUninstalledWorkspaceReportsNotApplicableStatusFactsAndPreservesAllBytes()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-uninstalled");
        workspace.SeedLockBytes([0x21, 0x22, 0x23]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--format", "json", "--detail", "full");

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var result = StatusJsonAssertions.Result(root);
        var installation = result.GetProperty("installation");
        Assert.Equal("uninstalled", installation.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, installation.GetProperty("entryPath").ValueKind);
        Assert.Equal(JsonValueKind.Null, installation.GetProperty("loaderPath").ValueKind);

        var context = result.GetProperty("context");
        StatusJsonAssertions.MeasurementAvailable(context.GetProperty("startup").GetProperty("shipped"));
        StatusJsonAssertions.MeasurementNullable(context.GetProperty("startup").GetProperty("current"));
        StatusJsonAssertions.MeasurementNullable(context.GetProperty("startup").GetProperty("difference"));
        StatusJsonAssertions.MeasurementAvailable(context.GetProperty("allRouted"));
        StatusJsonAssertions.MeasurementNullable(context.GetProperty("startup").GetProperty("mayLoadAgain"));
        Assert.Equal(JsonValueKind.Null, context.GetProperty("startupShare").ValueKind);

        var structure = result.GetProperty("structure");
        Assert.Equal(JsonValueKind.Null, structure.GetProperty("rootCategories").GetProperty("count").ValueKind);
        var generated = result.GetProperty("entriesSections").EnumerateArray().ToArray();
        Assert.NotEmpty(generated);
        Assert.All(generated, item => Assert.Equal("not-applicable", item.GetProperty("state").GetString()));
        Assert.Empty(result.GetProperty("frameworkFiles").EnumerateArray());
        Assert.Empty(result.GetProperty("extensions").EnumerateArray());
        Assert.Empty(result.GetProperty("libraries").EnumerateArray());
        Assert.Empty(result.GetProperty("recovery").GetProperty("candidates").EnumerateArray());
        Assert.All(root.GetProperty("findings").EnumerateArray(),
            finding => Assert.EndsWith("ownership-observation", finding.GetProperty("code").GetString(), StringComparison.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }

    [Trait("Boundary", "Host")]
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
                    [StatusIntegrationWorkspace.ExtensionTargetPath])]));
        workspace.SeedLockBytes([0x41, 0x42, 0x43]);
        var before = workspace.SnapshotHashes();
        var lockBefore = workspace.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(workspace, "status", "--workspace", workspace.Path, "--format", "json", "--detail", "full");

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var root = document.RootElement;
        Assert.Equal("incomplete", root.GetProperty("status").GetString());
        var result = StatusJsonAssertions.Result(root);
        var bytes = result.GetProperty("context").GetProperty("allRouted").GetProperty("bytes");
        Assert.Equal(JsonValueKind.Null, bytes.ValueKind);

        var extensions = result.GetProperty("extensions");
        var installed = Assert.Single(extensions.EnumerateArray());
        Assert.Equal("toolkit", installed.GetProperty("id").GetString());
        Assert.Equal("1.0.0", installed.GetProperty("version").GetString());
        var target = Assert.Single(installed.GetProperty("files").EnumerateArray());
        Assert.Equal("unavailable", target.GetProperty("state").GetString());
        Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "status.context-inventory-incomplete");
        Assert.Contains(root.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "status.extension-source-unavailable");
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lockBefore, workspace.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(workspace.RecoveryDirectory()));
    }
}
