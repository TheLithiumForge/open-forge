using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

public sealed class GenericRouteInitMutationIntegrationTests
{
    [Fact(DisplayName = "Generic Route Init dry-run and apply share the complete plan while dry-run remains write-free"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task DryRunAndApplyHavePlanParityAndDistinctEffectOutcomes()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-dry-run-parity");
        var metadata = new RouteInitMetadataInput(
            description: "Ready documents",
            responsibilitySpecified: false,
            responsibility: null,
            tags: ["Docs"]);
        var before = workspace.SnapshotHashes();

        var dryRun = await ExecuteAsync(
            workspace,
            workspace.Request("documents", RouteInitMode.DryRun, metadata));

        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Empty(dryRun.Findings);
        Assert.All(
            dryRun.Effects,
            effect => Assert.Equal(RouteInitEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/documents/_documents.md"));
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));

        var applied = await ExecuteAsync(
            workspace,
            workspace.Request("documents", metadata: metadata));

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Empty(applied.Findings);
        Assert.All(
            applied.Effects,
            effect => Assert.Equal(RouteInitEffectOutcome.Verified, effect.Outcome));
        Assert.Equal(
            dryRun.Effects.Select(StableEffectShape),
            applied.Effects.Select(StableEffectShape));
        Assert.True(workspace.Exists(".agents/documents/_documents.md"));
        Assert.NotEqual(before, workspace.SnapshotHashes());
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Generic Route Init converges on an explicit target and reports a verified no-op without recovery"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task RepeatedApplyIsIdempotentAndSecondRunIsNoOp()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-idempotence");
        var metadata = new RouteInitMetadataInput(
            description: "Ready documents",
            responsibilitySpecified: true,
            responsibility: null,
            tags: ["Docs"]);
        var request = workspace.Request("documents", metadata: metadata);

        var first = await ExecuteAsync(workspace, request);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.Equal(RouteInitVerificationState.Verified, first.Verification);
        var afterFirst = workspace.SnapshotHashes();

        var second = await ExecuteAsync(
            workspace,
            workspace.Request("documents", metadata: RouteInitMetadataInput.None));

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.Empty(second.Effects);
        Assert.Contains(
            second.Entrypoints,
            entrypoint => entrypoint.Current == RouteInitEntrypointCurrent.Existing
                && entrypoint.Outcome == RouteInitEntrypointOutcome.Unchanged);
        Assert.Contains(
            second.UnchangedPaths,
            path => path == ".agents/documents/_documents.md");
        Assert.Equal(RouteInitRecoveryState.NotRequired, second.Recovery.State);
        Assert.Equal(afterFirst, workspace.SnapshotHashes());
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Generic Route Init replaces only a stale generated region and deletes its verified recovery bundle"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task GeneratedRegionReplacementUsesRecoveryAndLeavesNoCandidate()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-recovery");
        workspace.WriteText(
            ".agents/memory/_memory.md",
            "---\nopen-forge:\n  description: Memory\n  tags: [Memory]\n---\n\n# memory\n\n"
                + "Authored bytes before the generated region.\n\n"
                + "Authored bytes after the generated region.\n\n"
                + "## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n\n"
                + "- stale - Stale entry - #Old\n\n"
                + "<!-- open-forge:generated-index:end -->\n");
        var before = workspace.ReadText(".agents/memory/_memory.md");

        var result = await ExecuteAsync(
            workspace,
            workspace.Request("memory/project"));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Contains(
            result.Effects,
            effect => effect.Kind == RouteInitEffectKind.GeneratedRegion
                && effect.Action == RouteInitEffectAction.Replace
                && effect.Residual == RouteInitEffectResidual.None);
        Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
        Assert.Equal(0, await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
        var after = workspace.ReadText(".agents/memory/_memory.md");
        Assert.Contains("Authored bytes before the generated region.", after, StringComparison.Ordinal);
        Assert.Contains("Authored bytes after the generated region.", after, StringComparison.Ordinal);
        Assert.Contains("project/_project.md", after, StringComparison.Ordinal);
        Assert.DoesNotContain("- stale - Stale entry - #Old", after, StringComparison.Ordinal);
        Assert.NotEqual(before, after);
    }

    [Fact(DisplayName = "Generic Route Init blocks a saved plan when a routed sibling appears before application"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task UnderLeaseRevalidationPreservesAConcurrentRoutedSibling()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-generic-plan-race");
        workspace.WriteText(
            ".agents/memory/_memory.md",
            OpenForgeDocumentSeed.Metadata(
                "Memory",
                ["Memory"],
                $"\n{OpenForgeDocumentSeed.GeneratedEntries("- stale - Stale entry - #Old")}"));
        var operation = RouteInitOperationFactory.Create(workspace.LockStoreRoot);
        var build = await operation.PlanBuilder.BuildAsync(
            workspace.Request("memory/project"),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Init.Models.Planning.RouteInitPlan>(build.Plan);

        workspace.WriteText(
            ".agents/memory/other/_other.md",
            OpenForgeDocumentSeed.Metadata(
                "Other",
                ["Other"],
                $"\n{OpenForgeDocumentSeed.GeneratedEntries(string.Empty)}"));
        var afterConcurrentEdit = workspace.SnapshotHashes();

        var outcome = await operation.ApplicationOperation.ExecuteAsync(
            plan,
            TestContext.Current.CancellationToken);
        var result = operation.ResultBuilder.Build(outcome.Formation);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInitFindingCode.TargetChanged, Assert.Single(result.Findings).Code);
        Assert.Equal(afterConcurrentEdit, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(".agents/memory/project/_project.md"));
        Assert.Equal(
            0,
            await workspace.RecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "Route Init final verification rejects a changed exact receipt postcondition"), Trait("Feature", "route-init-generic"), Trait("Evidence", "Integration")]
    public async Task FinalVerificationRejectsChangedReceiptPostcondition()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-final-postcondition");
        workspace.CreateDirectory(".agents/documents");
        var logicalPath = workspace.Absolute(".agents/documents/_documents.md");
        var before = FileStateSnapshot.Missing(logicalPath);
        var intendedBytes = "intended scaffold bytes"u8.ToArray();
        var change = PlannedFileChange.Create(before.Expectation, intendedBytes);
        workspace.WriteBytes(".agents/documents/_documents.md", intendedBytes);
        var after = FileStateSnapshot.File(logicalPath, logicalPath, intendedBytes);
        var receipt = FileChangeReceipt.Verified(change, before, after);
        File.WriteAllText(logicalPath, "concurrent authored replacement");
        var request = workspace.Request("documents");
        var preview = new RouteInitResultFormation(
            workspace.Workspace,
            RouteInitMode.Apply,
            RouteInitScaffold.Generic,
            new RouteInitTarget("documents", "documents", ".agents/documents/_documents.md"),
            new RouteInitPlanFacts(RouteInitPlanCompleteness.Complete, RouteInitPlanSafety.Safe),
            framework: null,
            entrypoints: [],
            effects: [],
            unchangedPaths: [],
            new RouteInitLifecycle(RouteInitLifecycleAction.None, RouteInitLifecycleOutcome.NotRequested),
            new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
            RouteInitVerificationState.NotRequested,
            findings: []);
        var plan = new RouteInitPlan(
            request,
            preview,
            directoryCreations: [],
            fileChanges: [change],
            recoveryTargets: [],
            intendedLifecycle: null);
        var lockManager = new WorkspaceLockManager(workspace.LockStoreRoot);
        var lockResult = await lockManager.AcquireAsync(
            new WorkspaceLockRequest(
                workspace.Workspace,
                RouteInitDefinitions.CommandIdentity,
                Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        await using (lease.ConfigureAwait(false))
        {
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            var verification = await new RouteInitAppliedVerifier(
                    new RouteInitPlanBuilder(),
                    validator)
                .VerifyAsync(
                    plan,
                    lease,
                    directoryReceipts: [],
                    fileReceipts: [receipt],
                    TestContext.Current.CancellationToken);

            Assert.Equal(RouteInitAppliedVerificationState.Failed, verification.State);
            Assert.Contains("no longer matches", verification.Cause, StringComparison.Ordinal);
        }
    }

    private static async ValueTask<RouteInitResult> ExecuteAsync(
        GenericRouteInitIntegrationWorkspace workspace,
        RouteInitRequest request)
        => await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);

    private static string StableEffectShape(RouteInitEffect effect)
        => string.Join(
            "|",
            effect.Path,
            effect.Kind,
            effect.Action,
            effect.SourceAssetPath,
            effect.Change?.Before,
            effect.Change?.Expected,
            effect.Residual);
}
