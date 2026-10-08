using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.IntegrationTests.Framework.Distribution.Operational.Shared.Frontmatter;
using OpenForge.Cli.IntegrationTests.Hosting;

using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusLifecycleIntegrationTests
{
    [Fact(DisplayName = "Status reports matching root Framework delivery as current"), Trait("Boundary", "Host"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task MatchingRootDeliveryIsCurrent()
    {
        using var workspace = TemporaryWorkspace.Create("status-root-framework");
        FrameworkLifecycleFrontmatterFixture.Write(workspace, FrontmatterForm.Root);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"root"}""");
        using var result = await ReadFrontmatterStatusAsync(workspace);
        Assert.Equal("current", Assert.Single(result.RootElement.GetProperty("data").GetProperty("frameworkFiles").EnumerateArray())
            .GetProperty("state").GetString());
        Assert.DoesNotContain(result.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.framework-target-changed");
    }

    [Theory(DisplayName = "Status reports opposite Framework frontmatter forms as managed changes"), Trait("Boundary", "Host"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    [InlineData("root", (int)FrontmatterForm.Scoped)]
    [InlineData("scoped", (int)FrontmatterForm.Root)]
    public async Task OppositeFormReportsManagedChange(string setting, int authoredForm)
    {
        using var workspace = TemporaryWorkspace.Create("status-opposite-framework");
        FrameworkLifecycleFrontmatterFixture.Write(workspace, (FrontmatterForm)authoredForm);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""{"schemaVersion":1,"frontmatter":"{{setting}}"}""");
        using var result = await ReadFrontmatterStatusAsync(workspace);
        Assert.Equal("changed", Assert.Single(result.RootElement.GetProperty("data").GetProperty("frameworkFiles").EnumerateArray())
            .GetProperty("state").GetString());
        Assert.Contains(result.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.framework-target-changed");
    }

    [Theory(DisplayName = "Status leaves Framework comparison unavailable when settings cannot supply a form")]
    [Trait("Boundary", "Host"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidSettingsMakesComparisonUnavailable(bool unavailable)
    {
        using var workspace = TemporaryWorkspace.Create("status-invalid-form");
        FrameworkLifecycleFrontmatterFixture.Write(workspace, FrontmatterForm.Scoped);
        if (unavailable) workspace.CreateDirectory(WorkspaceSettingsDefinitions.RelativePath);
        else workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"unknown"}""");
        using var result = await ReadFrontmatterStatusAsync(workspace);
        Assert.Equal("unavailable", Assert.Single(result.RootElement.GetProperty("data").GetProperty("frameworkFiles").EnumerateArray())
            .GetProperty("state").GetString());
        Assert.Contains(result.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.framework-target-unavailable");
        Assert.DoesNotContain(result.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.framework-target-changed");
    }

    private static async Task<JsonDocument> ReadFrontmatterStatusAsync(TemporaryWorkspace workspace)
    {
        var before = workspace.SnapshotHashes();
        var run = await CliHostCapture.RunAsync(["status", "--workspace", workspace.Path, "--format", "json", "--detail", "full"], workspace.Path);
        Assert.Equal(string.Empty, run.Error);
        Assert.Equal(before, workspace.SnapshotHashes());
        return JsonDocument.Parse(run.Output);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Framework ignores recorded source identity and compares the target with the running payload"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task RecordedSourceIdentityDoesNotReplaceCurrentPayloadComparison()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-framework-source-mismatch");
        var targetBytes = Encoding.UTF8.GetBytes("# Persisted loader\n");
        workspace.WriteBytes(FrameworkPayloadAsset.LoaderPath, targetBytes);
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        StatusLifecycleFixture.Write(
            workspace,
            new(new("older-framework", "0.0.1"), [FrameworkPayloadAsset.LoaderPath], []),
            extensions: null);

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--format", "json",
            "--detail", "full");

        Assert.Equal(3, run.ExitCode);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        var framework = result.GetProperty("frameworkFiles");
        var target = Assert.Single(framework.EnumerateArray(), item =>
            item.GetProperty("path").GetString() == FrameworkPayloadAsset.LoaderPath);
        Assert.Equal("changed", target.GetProperty("state").GetString());
        var findingCodes = document.RootElement.GetProperty("findings").EnumerateArray()
            .Select(finding => finding.GetProperty("code").GetString())
            .ToArray();
        Assert.Contains("status.framework-target-changed", findingCodes);
        Assert.DoesNotContain("status.framework-lifecycle-incomplete", findingCodes);
        Assert.DoesNotContain("status.framework-target-blocked", findingCodes);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Installed workspace ignores a missing legacy Extension section"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task InstalledWorkspaceIgnoresMissingLegacySection()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-installed-missing-lifecycle");
        workspace.WritePostInstallAgentText(StatusIntegrationWorkspace.LifecyclePath, "{\"framework\":{},\"extensions\":null}");

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--format", "json",
            "--detail", "full");

        Assert.Equal(0, run.ExitCode);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        Assert.NotEmpty(result.GetProperty("frameworkFiles").EnumerateArray());
        Assert.Empty(result.GetProperty("extensions").EnumerateArray());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Uninstalled workspace with a recovery residual does not claim lifecycle absence"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task RecoveryResidualPreventsUninstalledLifecycleAbsenceProof()
    {
        using var workspace = StatusIntegrationWorkspace.Create(
            "status-uninstalled-recovery-residual");
        using var recovery = StatusRecoveryFixture.Create(workspace);
        _ = recovery.AddDraft();

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--format", "json",
            "--detail", "full");

        Assert.Equal(3, run.ExitCode);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        Assert.Empty(result.GetProperty("frameworkFiles").EnumerateArray());
        Assert.Empty(result.GetProperty("extensions").EnumerateArray());
        Assert.Single(result.GetProperty("recovery").GetProperty("candidates").EnumerateArray());
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.recovery-draft-incomplete");
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Generated and managed observations preserve every target state and deduplicate shared owners"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task GeneratedAndManagedObservationsPreserveEveryTargetStateAndSharedOwnerDeduplication()
    {
        foreach (var scenario in new[] { "current", "changed", "missing", "unavailable", "blocked", "not-applicable" })
        {
            Assert.Equal(scenario, await StatusLifecycleTargetScenarios.ReadGeneratedNavigationStateAsync(scenario));
        }

        foreach (var scenario in new[] { "current", "changed", "missing", "unavailable", "blocked" })
        {
            Assert.Equal(scenario, await StatusLifecycleTargetScenarios.ReadFrameworkTargetStateAsync(scenario));
            Assert.Equal(scenario, await StatusLifecycleTargetScenarios.ReadExtensionTargetStateAsync(scenario));
        }

        using var shared = StatusIntegrationWorkspace.Create("status-shared-extension-owners");
        var sharedBytes = Encoding.UTF8.GetBytes("# Shared extension target\n");
        shared.WriteBytes(StatusIntegrationWorkspace.ExtensionTargetPath, sharedBytes);
        StatusLifecycleFixture.Write(
            shared,
            framework: null,
            extensions: StatusLifecycleFixture.Extensions(
                [
                    new StatusLifecycleFixture.ExtensionSeed(
                        "alpha", "1.0.0", shared.Combine("missing-alpha-source"), [],
                        [StatusIntegrationWorkspace.ExtensionTargetPath]),
                    new StatusLifecycleFixture.ExtensionSeed(
                        "beta", "2.0.0", shared.Combine("missing-beta-source"), [],
                        [StatusIntegrationWorkspace.ExtensionTargetPath]),
                ]));
        shared.SeedLockBytes([0x51, 0x52]);
        var before = shared.SnapshotHashes();
        var lockBefore = shared.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(shared.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(
            shared,
            "status",
            "--workspace",
            shared.Path,
            "--format", "json",
            "--detail", "full");

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        var extensions = result.GetProperty("extensions");
        Assert.Equal(
            ["alpha", "beta"],
            extensions.EnumerateArray()
                .Select(item => item.GetProperty("id").GetString()));
        Assert.All(extensions.EnumerateArray(), extension =>
            Assert.Equal("unavailable", Assert.Single(extension.GetProperty("files").EnumerateArray())
                .GetProperty("state").GetString()));
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "status.extension-source-unavailable");
        Assert.Equal(before, shared.SnapshotHashes());
        Assert.Equal(lockBefore, shared.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(shared.RecoveryDirectory()));
    }
}
