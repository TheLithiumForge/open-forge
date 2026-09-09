using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

public sealed class RouteInitFrameworkSafetyIntegrationTests
{
    [Fact(DisplayName = "Framework dry-run forms the complete sparse plan without workspace or recovery effects"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task DryRunIsReadOnlyAndReportsEveryScopeEffect()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-dry-run",
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents",
            RouteInitMode.DryRun);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RouteInitPlanCompleteness.Complete, result.Plan.Completeness);
        Assert.Equal(RouteInitPlanSafety.Safe, result.Plan.Safety);
        Assert.All(
            result.Effects,
            effect => Assert.Equal(RouteInitEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(RouteInitLifecycleAction.Publish, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.Planned, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
        Assert.Equal(RouteInitRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/release-notes"));
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(RouteInitJsonRenderer.Render(
            new CliPresentationRequest<RouteInitResult>(result, new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal))));
        var root = document.RootElement;
        var facts = root.GetProperty("result");
        Assert.Equal("attention", root.GetProperty("status").GetString());
        Assert.Equal("dry-run", facts.GetProperty("mode").GetString());
        Assert.Equal("framework", facts.GetProperty("scaffold").GetString());
        Assert.Equal("memory/release-notes/crystallized/documents", facts.GetProperty("target").GetProperty("id").GetString());
        Assert.Equal(["installed-root", "scope", "managed", "managed"],
            facts.GetProperty("framework").GetProperty("segments").EnumerateArray().Select(segment => segment.GetProperty("role").GetString()));
        Assert.Equal("route-init.needs-authoring", facts.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal("publish", facts.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("planned", facts.GetProperty("lifecycle").GetProperty("outcome").GetString());
    }

    [Fact(DisplayName = "Framework application reaches verified attention, preserves user scope ownership, and deletes its recovery bundle"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ApplicationVerifiesSparseScopeAndRemovesRecovery()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-apply",
            TestContext.Current.CancellationToken);

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents");

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
        Assert.Null(result.Recovery.ResidualPath);
        Assert.True(workspace.Exists(".agents/memory/release-notes/_release-notes.md"));
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
        Assert.DoesNotContain(
            Assert.IsType<RouteInitFramework>(result.Framework).Segments,
            segment => segment.Path == ".agents/memory/release-notes/_release-notes.md"
                && segment.Role != RouteInitFrameworkSegmentRole.Scope);

        using var document = JsonDocument.Parse(RouteInitJsonRenderer.Render(
            new CliPresentationRequest<RouteInitResult>(result, new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal))));
        var facts = document.RootElement.GetProperty("result");
        Assert.Equal("verified", facts.GetProperty("verification").GetString());
        Assert.Equal("verified", facts.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("removed", facts.GetProperty("recovery").GetProperty("state").GetString());
        var entrypoints = facts.GetProperty("entrypoints").EnumerateArray().ToArray();
        var scope = Assert.Single(entrypoints, entrypoint => entrypoint.GetProperty("path").GetString() == ".agents/memory/release-notes/_release-notes.md");
        Assert.Equal("user", scope.GetProperty("ownership").GetString());
        Assert.Equal(JsonValueKind.Null, scope.GetProperty("sourceAssetPath").ValueKind);
        Assert.Contains("NeedsAuthoring", scope.GetProperty("metadata").GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
        Assert.All(entrypoints.Where(entrypoint => entrypoint.GetProperty("ownership").GetString() == "framework"),
            entrypoint => Assert.Equal(JsonValueKind.String, entrypoint.GetProperty("sourceAssetPath").ValueKind));
    }

    [Fact(DisplayName = "A verified Framework no-op reports existing managed targets without a recovery or lifecycle write"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ExistingCanonicalRouteIsVerifiedNoOp()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-no-op",
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            RouteInitFrameworkIntegrationWorkspace.FrameworkRoute);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Empty(result.UnchangedPaths.Where(path =>
            path == RouteInitFrameworkIntegrationWorkspace.LifecyclePath));
        Assert.Equal(RouteInitLifecycleAction.Preserve, result.Lifecycle.Action);
        Assert.Equal(RouteInitLifecycleOutcome.AlreadyCurrent, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));

        using var document = JsonDocument.Parse(RouteInitJsonRenderer.Render(
            new CliPresentationRequest<RouteInitResult>(result, new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal))));
        var facts = document.RootElement.GetProperty("result");
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(facts.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", facts.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("already-current", facts.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Empty(facts.GetProperty("findings").EnumerateArray());
    }

    [Fact(DisplayName = "Framework external lock contention blocks before directory, file, lifecycle, or recovery effects"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task ExternalLockContentionIsPreEffectBlocked()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-lock-contention",
            TestContext.Current.CancellationToken);
        using var contendingLease = workspace.HoldExternalLock();
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents");

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.WorkspaceLockUnavailable);
        Assert.All(
            result.Effects,
            effect => Assert.Equal(RouteInitEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Framework cancellation before application is interrupted with no workspace or recovery effect"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task PreCancelledRequestIsInterruptedBeforeEffects()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-pre-cancel",
            TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(
                workspace.Request("memory/release-notes/crystallized/documents"),
                cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.Interrupted);
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(
            0,
            await workspace.ReadRecoveryCandidateCountAsync(
                TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Framework cancellation during the first directory effect retains the created residual and stops later effects"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task CancellationDuringEffectsRetainsDirectoryResidual()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-mid-cancel",
            TestContext.Current.CancellationToken);
        using var cancellation = new CancellationTokenSource();
        DirectoryCreationReceipt? observedReceipt = null;
        var result = await RouteInitOperationFactory.Create(
                workspace.LockStoreRoot,
                receipt =>
                {
                    Assert.Null(observedReceipt);
                    observedReceipt = receipt;
                    cancellation.Cancel();
                })
            .ExecuteAsync(
                workspace.Request("memory/release-notes/crystallized/documents"),
                cancellation.Token);

        var receipt = Assert.IsType<DirectoryCreationReceipt>(observedReceipt);
        Assert.Equal(
            workspace.Combine(".agents/memory/release-notes"),
            receipt.Creation.LogicalPath);
        Assert.Equal(
            workspace.Combine(".agents/memory/release-notes"),
            receipt.IntendedPhysicalPath);
        Assert.Equal(FilesystemEffectState.Applied, receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.Interrupted);
        Assert.Contains(
            result.Effects,
            effect => effect.Path == ".agents/memory/release-notes"
                && effect.Kind == RouteInitEffectKind.Directory
                && effect.Outcome == RouteInitEffectOutcome.Verified
                && effect.Residual == RouteInitEffectResidual.Retained);
        Assert.True(workspace.Exists(".agents/memory/release-notes"));
        Assert.False(workspace.Exists(".agents/memory/release-notes/_release-notes.md"));
    }

    [Fact(DisplayName = "Framework generated-region corruption is refused before scope creation"), Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration")]
    public async Task UnsafeGeneratedRegionStopsBeforeEffects()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-generated-boundary",
            TestContext.Current.CancellationToken);
        workspace.WriteText(
            ".agents/memory/_memory.md",
            "---\nopen-forge:\n  description: Memory\n  tags: [Memory]\n---\n\n# Memory\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n\n- malformed generated region\n");
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents");

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.LifecycleBlocked);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
    }

    [Theory(DisplayName = "Framework recovery candidate conflict blocks apply and dry-run plans without consuming the candidate"),
     Trait("Feature", "route-init-framework"), Trait("Evidence", "Integration"),
     InlineData(false),
     InlineData(true)]
    public static async Task ExistingRecoveryCandidateBlocksBeforeEffects(bool dryRun)
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-framework-recovery-conflict",
            TestContext.Current.CancellationToken);
        var candidate = workspace.CreateRecoveryConflictCandidate();
        var before = workspace.SnapshotHashes();

        var result = await ExecuteAsync(
            workspace,
            "memory/release-notes/crystallized/documents",
            dryRun ? RouteInitMode.DryRun : RouteInitMode.Apply);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteInitFindingCode.RecoveryConflict);
        Assert.True(File.Exists(candidate));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteInitLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        Assert.Equal(RouteInitVerificationState.NotRequested, result.Verification);
    }

    private static ValueTask<RouteInitResult> ExecuteAsync(
        RouteInitFrameworkIntegrationWorkspace workspace,
        string target,
        RouteInitMode mode = RouteInitMode.Apply)
        => RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(target, mode), TestContext.Current.CancellationToken);
}
