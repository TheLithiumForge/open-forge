using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitApplicationIntegrityTests
{
    [Fact(DisplayName = "An escaped directory attempt is the only effect reported with unknown completion"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public async Task EscapedDirectoryAttemptIsReportedAsUnknown()
    {
        using var owned = TemporaryWorkspace.Create("route-init-attempt-directory");
        var workspace = Workspace(owned);
        var build = await new RouteInitPlanBuilder().BuildAsync(
            RouteInitRedTestData.Request(workspace, routeTarget: "documents"),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteInitPlan>(build.Plan);
        var attempted = plan.DirectoryCreations[0];

        var outcome = RouteInitApplicationResultFactory.Build(
            plan,
            directoryReceipts: [],
            fileReceipts: [],
            new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
            RouteInitVerificationState.Unknown,
            RouteInitApplicationResultFactory.Finding(
                plan,
                RouteInitFindingCode.OperationFailed,
                "The directory applier escaped without a receipt."),
            new RouteInitApplicationAttempt(
                RouteInitApplicationAttemptKind.Directory,
                attempted.LogicalPath));

        var unknown = Assert.Single(outcome.Formation.Effects.Where(effect =>
            effect.Outcome == RouteInitEffectOutcome.CompletionUnknown));
        Assert.Equal(RouteInitEffectKind.Directory, unknown.Kind);
        Assert.Equal(RouteInitEffectResidual.Unknown, unknown.Residual);
        Assert.All(
            outcome.Formation.Effects.Where(effect => !ReferenceEquals(effect, unknown)),
            effect => Assert.Equal(RouteInitEffectOutcome.NotStarted, effect.Outcome));
        Assert.All(
            outcome.Formation.Entrypoints,
            entrypoint => Assert.Equal(RouteInitEntrypointOutcome.NotStarted, entrypoint.Outcome));
    }

    [Fact(DisplayName = "An escaped entrypoint attempt reports only that entrypoint and effect with unknown completion"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public async Task EscapedEntrypointAttemptIsReportedAsUnknown()
    {
        using var owned = TemporaryWorkspace.Create("route-init-attempt-file");
        var workspace = Workspace(owned);
        var build = await new RouteInitPlanBuilder().BuildAsync(
            RouteInitRedTestData.Request(workspace, routeTarget: "documents"),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteInitPlan>(build.Plan);
        var attempted = Assert.Single(plan.FileChanges);

        var outcome = RouteInitApplicationResultFactory.Build(
            plan,
            directoryReceipts: [],
            fileReceipts: [],
            new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
            RouteInitVerificationState.Unknown,
            RouteInitApplicationResultFactory.Finding(
                plan,
                RouteInitFindingCode.OperationFailed,
                "The file applier escaped without a receipt."),
            new RouteInitApplicationAttempt(
                RouteInitApplicationAttemptKind.File,
                attempted.LogicalPath));

        var entrypoint = Assert.Single(outcome.Formation.Entrypoints);
        Assert.Equal(RouteInitEntrypointOutcome.CompletionUnknown, entrypoint.Outcome);
        var unknown = Assert.Single(outcome.Formation.Effects.Where(effect =>
            effect.Outcome == RouteInitEffectOutcome.CompletionUnknown));
        Assert.Equal(RouteInitEffectKind.Entrypoint, unknown.Kind);
        Assert.Equal(RouteInitEffectResidual.Unknown, unknown.Residual);
        Assert.All(
            outcome.Formation.Effects.Where(effect => effect.Kind == RouteInitEffectKind.Directory),
            effect => Assert.Equal(RouteInitEffectOutcome.NotStarted, effect.Outcome));
    }

    [Fact(DisplayName = "An escaped lifecycle attempt reports lifecycle completion and recovery state as unknown"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void EscapedLifecycleAndRecoveryStoreAttemptsAreUnknown()
    {
        using var owned = TemporaryWorkspace.Create("route-init-attempt-lifecycle");
        var workspace = Workspace(owned);
        var lifecyclePath = owned.CreateFile(
            ".agents/open-forge.lifecycle.json",
            "old lifecycle bytes"u8.ToArray());
        var before = FileStateSnapshot.File(
            lifecyclePath,
            lifecyclePath,
            "old lifecycle bytes"u8);
        var change = PlannedFileChange.Replace(
            before.Expectation,
            "new lifecycle bytes"u8);
        var preview = RouteInitRedTestData.Formation(
            workspace,
            scaffold: RouteInitScaffold.Framework,
            entrypoints: [],
            effects: [],
            unchangedPaths: [],
            lifecycle: new RouteInitLifecycle(
                RouteInitLifecycleAction.Publish,
                RouteInitLifecycleOutcome.Planned),
            recovery: new RouteInitRecovery(RouteInitRecoveryState.NotCreated, ResidualPath: null),
            verification: RouteInitVerificationState.NotRequested);
        var plan = new RouteInitPlan(
            RouteInitRedTestData.Request(
                workspace,
                routeTarget: "memory",
                scaffold: RouteInitScaffold.Framework),
            preview,
            directoryCreations: [],
            fileChanges: [change],
            recoveryTargets: [OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundleTarget.Create(change, before)],
            intendedLifecycle: null);

        var outcome = RouteInitApplicationResultFactory.Build(
            plan,
            directoryReceipts: [],
            fileReceipts: [],
            new RouteInitRecovery(RouteInitRecoveryState.Unknown, ResidualPath: null),
            RouteInitVerificationState.Unknown,
            RouteInitApplicationResultFactory.Finding(
                plan,
                RouteInitFindingCode.OperationFailed,
                "The lifecycle applier escaped without a receipt."),
            new RouteInitApplicationAttempt(
                RouteInitApplicationAttemptKind.File,
                lifecyclePath));

        Assert.Equal(RouteInitLifecycleOutcome.CompletionUnknown, outcome.Formation.Lifecycle.Outcome);
        Assert.Equal(RouteInitRecoveryState.Unknown, outcome.Formation.Recovery.State);
        Assert.Equal(
            RouteInitRecoveryState.Unknown,
            RouteInitRecoveryLifecycle.FailedAfterStoreEntry().Recovery.State);
        Assert.Equal(
            RouteInitRecoveryState.Unknown,
            RouteInitRecoveryLifecycle.CancelledAfterStoreEntry().Recovery.State);
    }

    private static CliWorkspace Workspace(TemporaryWorkspace workspace)
        => new(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
