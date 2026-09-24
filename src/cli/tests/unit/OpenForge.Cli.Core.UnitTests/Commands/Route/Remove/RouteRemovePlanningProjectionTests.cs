using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Remove;

public sealed class RouteRemovePlanningProjectionTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove effect facts keep delete and bounded replacement coordinates"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void EffectFactsKeepDeleteAndReplacementCoordinates()
    {
        var effects = new[]
        {
            RouteRemoveTestData.Effect(
                ".agents/guidance/_guidance.md",
                RouteRemoveEffectKind.GeneratedRegion,
                RouteRemoveEffectAction.Replace),
            RouteRemoveTestData.Effect(
                "README.md",
                RouteRemoveEffectKind.ReferenceSource,
                RouteRemoveEffectAction.Replace),
            RouteRemoveTestData.Effect(
                RouteRemoveTestData.LeafPath,
                RouteRemoveEffectKind.RemovedFile,
                RouteRemoveEffectAction.Delete),
        };

        Assert.Equal(
            [RouteRemoveEffectKind.GeneratedRegion, RouteRemoveEffectKind.ReferenceSource, RouteRemoveEffectKind.RemovedFile],
            effects.Select(effect => effect.Kind));
        Assert.Equal(
            [RouteRemoveEffectAction.Replace, RouteRemoveEffectAction.Replace, RouteRemoveEffectAction.Delete],
            effects.Select(effect => effect.Action));
        Assert.All(effects, effect =>
        {
            Assert.Equal(RouteRemoveEffectOutcome.Planned, effect.Outcome);
            Assert.Equal(RouteRemoveEffectResidual.None, effect.Residual);
            Assert.NotNull(effect.Before.ContentSha256);
            Assert.Null(effect.Expected.ContentSha256);
        });
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove plan projection distinguishes an exact no-op from planned effects"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void PlanProjectionDistinguishesNoOpFromPlannedEffects()
    {
        var emptyProjection = Projection();
        var noOp = new RouteRemovePlan
        {
            Request = RouteRemoveTestData.Request(),
            Preview = RouteRemoveTestData.Formation(),
            Projection = emptyProjection,
        };
        var planned = noOp with
        {
            Projection = emptyProjection with
            {
                FileChanges = [null!],
            },
        };

        Assert.True(noOp.IsNoOp);
        Assert.False(planned.IsNoOp);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove review preview projects dry-run mode from retained plan facts"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitBehavior")]
    public void ReviewPreviewUsesRetainedPlanWithoutChangingThePlan()
    {
        var plan = new RouteRemovePlan
        {
            Request = RouteRemoveTestData.Request(mode: RouteRemoveMode.Apply),
            Preview = RouteRemoveTestData.Formation(mode: RouteRemoveMode.Apply),
            Projection = Projection(),
        };

        var preview = RouteRemovePlanProjector.CreateDryRunPreview(plan);

        Assert.Equal(RouteRemoveMode.DryRun, preview.Mode);
        Assert.Equal(RouteRemoveMode.Apply, plan.Preview.Mode);
        Assert.Same(plan.Preview.Workspace, preview.Workspace);
        Assert.Same(plan.Preview.Source, preview.Source);
        Assert.Equal(plan.Preview.Effects, preview.Effects);
        Assert.Same(plan.Preview.References, preview.References);
        Assert.Same(plan.Preview.Recovery, preview.Recovery);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Remove navigation postcondition retains its failure cause"),
     Trait("Feature", "route-remove"), Trait("Evidence", "UnitContract")]
    public void PostconditionsRetainFailureCauses()
    {
        var navigation = new RouteRemoveNavigationPostRemoveResult(
            RouteRemoveNavigationPostRemoveState.Failed,
            "The generated region could not be verified.");

        Assert.Equal(RouteRemoveNavigationPostRemoveState.Failed, navigation.State);
        Assert.Equal("The generated region could not be verified.", navigation.Cause);
    }

    private static RouteRemovePlanProjectionInput Projection(
        ImmutableArray<PlannedFileChange> fileChanges = default)
        => new()
        {
            Subject = null!,
            Ownership = null!,
            Settings = WorkspaceSettingsRead.Absent(Path.Combine(
                RouteRemoveTestData.Workspace().LexicalRoot,
                ".agents",
                "open-forge.json")),
            RemovalSelection = new WorkspaceRemovalSelection(),
            References = null!,
            Navigation = null!,
            FileChanges = fileChanges.IsDefault ? [] : fileChanges,
            DirectoryDeletions = [],
            RecoveryTargets = [],
        };
}
