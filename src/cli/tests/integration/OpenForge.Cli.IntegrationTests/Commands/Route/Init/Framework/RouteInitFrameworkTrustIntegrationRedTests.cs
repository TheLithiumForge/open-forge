using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

public sealed class RouteInitFrameworkTrustIntegrationRedTests
{
    [Theory(DisplayName = "Framework Route Init requires a trusted current root Install"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("missing-install")]
    [InlineData("missing-lifecycle")]
    public async Task MissingTrustedInstallIsRefused(string scenario)
    {
        using var workspace = scenario == "missing-install"
            ? RouteInitFrameworkIntegrationWorkspace.CreateEmpty("route-init-framework-no-install")
            : await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
                "route-init-framework-no-lifecycle",
                TestContext.Current.CancellationToken);
        if (scenario == "missing-lifecycle")
        {
            workspace.Delete(RouteInitFrameworkIntegrationWorkspace.LifecyclePath);
        }

        var before = workspace.SnapshotHashes();
        var result = await ExecuteAsync(workspace);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.FrameworkInstallRequired);
        Assert.Equal(RouteInitPlanSafety.Blocked, result.Plan.Safety);
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Framework Route Init refuses changed physical state and an outdated running inventory"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("changed-physical-state")]
    [InlineData("outdated-inventory")]
    public async Task UntrustedInstalledStateIsRefused(string scenario)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-untrusted-install",
            TestContext.Current.CancellationToken);
        if (scenario == "changed-physical-state")
        {
            workspace.WriteText(
                RouteInitFrameworkIntegrationWorkspace.LoaderPath,
                "# Loader changed outside the trusted Install baseline\n");
        }
        else
        {
            var lifecycle = workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath);
            using var document = JsonDocument.Parse(lifecycle);
            var fingerprint = document.RootElement
                .GetProperty("framework")
                .GetProperty("source")
                .GetProperty("inventoryFingerprint")
                .GetString()
                ?? throw new InvalidOperationException("The Install fixture has no inventory fingerprint.");
            workspace.WriteText(
                RouteInitFrameworkIntegrationWorkspace.LifecyclePath,
                lifecycle.Replace(
                    fingerprint,
                    new string('0', fingerprint.Length),
                    StringComparison.Ordinal));
        }

        var before = workspace.SnapshotHashes();
        var result = await ExecuteAsync(workspace);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.FrameworkUpdateRequired);
        Assert.Equal(RouteInitPlanSafety.Blocked, result.Plan.Safety);
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Framework Route Init refuses malformed or incomplete canonical lifecycle sections before any write"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    [InlineData("malformed")]
    [InlineData("framework-null")]
    [InlineData("extensions-null")]
    [InlineData("framework-missing")]
    [InlineData("extensions-missing")]
    [InlineData("framework-incomplete")]
    [InlineData("extensions-incomplete")]
    public async Task IncompleteOrMalformedLifecycleCannotAuthorizeFrameworkEffects(string scenario)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-lifecycle-boundary",
            TestContext.Current.CancellationToken);
        RewriteLifecycle(workspace, scenario);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(workspace);

        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code is RouteInitFindingCode.LifecycleBlocked
                or RouteInitFindingCode.LifecycleUnavailable
                or RouteInitFindingCode.FrameworkInstallRequired
                or RouteInitFindingCode.FrameworkUpdateRequired);
        Assert.NotEqual(RouteInitLifecycleOutcome.Verified, result.Lifecycle.Outcome);
        Assert.NotEqual(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    private static void RewriteLifecycle(
        RouteInitFrameworkIntegrationWorkspace workspace,
        string scenario)
    {
        if (scenario == "malformed")
        {
            workspace.WriteBytes(
                RouteInitFrameworkIntegrationWorkspace.LifecyclePath,
                [0x7B, 0xFF, 0x7D]);
            return;
        }

        using var current = JsonDocument.Parse(
            workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.LifecyclePath));
        var root = current.RootElement;
        var framework = root.GetProperty("framework").GetRawText();
        var extensions = root.GetProperty("extensions").GetRawText();
        var includeFramework = scenario != "framework-missing";
        var includeExtensions = scenario != "extensions-missing";
        var frameworkValue = scenario == "framework-null" ? "null" : framework;
        var extensionsValue = scenario == "extensions-null" ? "null" : extensions;
        if (scenario == "framework-incomplete")
        {
            frameworkValue = ReplaceCoverage(frameworkValue, "incomplete");
        }
        else if (scenario == "extensions-incomplete")
        {
            extensionsValue = ReplaceCoverage(extensionsValue, "incomplete");
        }

        var json = "{"
            + "\"schemaVersion\":1,"
            + "\"fingerprintPolicy\":\"open-forge-markdown-v1\","
            + $"\"workspacePath\":\"{JsonEncodedText.Encode(workspace.PhysicalPath)}\"";
        if (includeFramework)
        {
            json += $",\"framework\":{frameworkValue}";
        }

        if (includeExtensions)
        {
            json += $",\"extensions\":{extensionsValue}";
        }

        workspace.WriteText(
            RouteInitFrameworkIntegrationWorkspace.LifecyclePath,
            json + "}");
    }

    private static string ReplaceCoverage(string json, string coverage)
    {
        const string complete = "\"coverage\":\"complete\"";
        var index = json.IndexOf(complete, StringComparison.Ordinal);
        if (index < 0)
        {
            throw new InvalidOperationException("The lifecycle fixture has no complete coverage field.");
        }

        return json[..index]
            + $"\"coverage\":\"{coverage}\""
            + json[(index + complete.Length)..];
    }

    private static ValueTask<OpenForge.Cli.Core.Commands.Route.Init.Models.Result.RouteInitResult> ExecuteAsync(
        RouteInitFrameworkIntegrationWorkspace workspace)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request(),
                TestContext.Current.CancellationToken);
}
