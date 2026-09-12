using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusLifecycleIntegrationTests
{
    [Fact(DisplayName = "Framework source mismatch remains incomplete while the persisted target is compared independently"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task FrameworkSourceMismatchDoesNotBlockIndependentTargetComparison()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-framework-source-mismatch");
        var targetBytes = Encoding.UTF8.GetBytes("# Persisted loader\n");
        workspace.WriteBytes(FrameworkPayloadAsset.LoaderPath, targetBytes);
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        StatusLifecycleFixture.Write(
            workspace,
            new FrameworkLifecycleState
            {
                Coverage = LifecycleSchema.CompleteCoverage,
                Source = new FrameworkLifecycleSource
                {
                    Id = "open-forge",
                    Version = "1.0.0",
                    InventoryFingerprint = payload.InventoryFingerprint,
                },
                Targets =
                [
                    new FrameworkLifecycleTarget
                    {
                        Path = FrameworkPayloadAsset.LoaderPath,
                        SourceAssetPath = ".agents/missing-running-source.md",
                        Region = null,
                        BaselineFingerprint = StatusLifecycleFixture.Hash(targetBytes),
                        FingerprintKind = LifecycleSchema.ExactBytesFingerprintKind,
                    },
                ],
                GeneratedRegions = [],
            },
            extensions: null);

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--json");

        Assert.Equal(3, run.ExitCode);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        var framework = result.GetProperty("lifecycle").GetProperty("framework");
        Assert.Equal("unavailable", framework.GetProperty("sourceAvailability").GetString());
        var target = Assert.Single(framework.GetProperty("targets").EnumerateArray());
        Assert.Equal("current", target.GetProperty("state").GetString());
        var findingCodes = result.GetProperty("findings").EnumerateArray()
            .Select(finding => finding.GetProperty("code").GetString())
            .ToArray();
        Assert.Contains("framework-lifecycle-incomplete", findingCodes);
        Assert.DoesNotContain("framework-target-blocked", findingCodes);
    }

    [Fact(DisplayName = "Installed workspace with a missing Extension lifecycle section retains incomplete lifecycle facts"), Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task InstalledWorkspaceDoesNotTreatMissingLifecycleSectionAsAbsent()
    {
        using var workspace = await StatusIntegrationWorkspace.CreateInstalledAsync(
            "status-installed-missing-lifecycle");
        var lifecycle = StatusLifecycleFixture.Read(workspace);
        StatusLifecycleFixture.WriteSections(
            workspace,
            lifecycle.Framework,
            extensions: null);

        var run = await StatusIntegrationApplication.RunAsync(
            workspace,
            "status",
            "--workspace",
            workspace.Path,
            "--json");

        Assert.Equal(3, run.ExitCode);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        Assert.Equal(
            "trusted",
            result.GetProperty("lifecycle").GetProperty("framework")
                .GetProperty("state").GetString());
        Assert.Equal(
            "incomplete",
            result.GetProperty("lifecycle").GetProperty("extensions")
                .GetProperty("state").GetString());
    }

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
            "--json");

        Assert.Equal(3, run.ExitCode);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var result = StatusJsonAssertions.Result(document.RootElement);
        Assert.Equal(
            "incomplete",
            result.GetProperty("lifecycle").GetProperty("framework")
                .GetProperty("state").GetString());
        Assert.Equal(
            "incomplete",
            result.GetProperty("lifecycle").GetProperty("extensions")
                .GetProperty("state").GetString());
        Assert.Single(result.GetProperty("recovery").GetProperty("candidates").EnumerateArray());
    }

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
                ],
                [new StatusLifecycleFixture.PathSeed(
                    StatusIntegrationWorkspace.ExtensionTargetPath,
                    ["alpha", "beta"],
                    StatusLifecycleFixture.Hash(sharedBytes))]));
        shared.SeedLockBytes([0x51, 0x52]);
        var before = shared.SnapshotHashes();
        var lockBefore = shared.SnapshotLockBytes();
        var recoveryBefore = StatusRecoveryCatalogue.SnapshotEntries(shared.RecoveryDirectory());

        var run = await StatusIntegrationApplication.RunAsync(
            shared,
            "status",
            "--workspace",
            shared.Path,
            "--json");

        Assert.Equal(3, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = StatusIntegrationApplication.ParseJson(run);
        var extensions = StatusJsonAssertions.Result(document.RootElement)
            .GetProperty("lifecycle").GetProperty("extensions");
        Assert.Equal(
            ["alpha", "beta"],
            extensions.GetProperty("installed").EnumerateArray()
                .Select(item => item.GetProperty("id").GetString()));
        var target = Assert.Single(extensions.GetProperty("managedFiles").GetProperty("targets").EnumerateArray());
        Assert.Equal(["alpha", "beta"], target.GetProperty("owners").EnumerateArray().Select(owner => owner.GetString()));
        Assert.Equal("current", target.GetProperty("state").GetString());
        StatusJsonAssertions.AvailableZero(extensions.GetProperty("managedFiles").GetProperty("counts").GetProperty("changed"));
        StatusJsonAssertions.AvailableZero(extensions.GetProperty("managedFiles").GetProperty("counts").GetProperty("missing"));
        Assert.Equal(before, shared.SnapshotHashes());
        Assert.Equal(lockBefore, shared.SnapshotLockBytes());
        Assert.Equal(recoveryBefore, StatusRecoveryCatalogue.SnapshotEntries(shared.RecoveryDirectory()));
    }
}
