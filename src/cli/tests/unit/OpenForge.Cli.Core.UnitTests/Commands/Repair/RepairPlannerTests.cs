using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairPlannerTests
{
    [Fact(DisplayName = "Repair planner forms one safe-exact effect with one selected step and recovery boundary"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void PlannerBuildsSafeExactEffect()
    {
        var request = RepairTestData.Request(
            mode: RepairMode.DryRun,
            automatic: true);

        var plan = RepairPlanner.Build(
            request,
            [RepairTestData.Reference()]);

        Assert.False(plan.IsBlocked);
        Assert.False(plan.IsNoOp);
        var selected = Assert.Single(plan.Selection.Selected);
        var step = Assert.Single(plan.Steps);
        Assert.Same(selected, step.SelectedProposal);
        Assert.Equal(RepairStepOutcome.Planned, step.Outcome);
        var effect = Assert.Single(plan.Effects);
        Assert.Equal(RepairTestData.SourcePath, effect.SourceCanonicalPath);
        Assert.Equal("old", effect.Changes[0].ExpectedDestination);
        Assert.Equal("new", effect.Changes[0].IntendedDestination);
        Assert.Equal(
            RepairRecoveryRequirementKind.Required,
            step.Recovery.Kind);
        Assert.NotNull(step.Recovery.Attribution);
    }

    [Fact(DisplayName = "Repair planner forms a verified no-op when the observed destination already equals the intended destination"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void PlannerBuildsNoOpForAlreadyIntendedState()
    {
        var plan = RepairPlanner.Build(
            RepairTestData.Request(mode: RepairMode.DryRun, automatic: true),
            [
                RepairTestData.Reference(
                    expectedDestination: "old",
                    observedDestination: "new",
                    intendedDestination: "new",
                    fileText: "new"),
            ]);

        Assert.False(plan.IsBlocked);
        Assert.True(plan.IsNoOp);
        Assert.Empty(plan.Effects);
        var noOp = Assert.Single(plan.NoOps);
        Assert.Equal("new", noOp.Destination);
        Assert.Equal(RepairStepOutcome.NoOp, Assert.Single(plan.Steps).Outcome);
        Assert.Equal(
            RepairRecoveryRequirementKind.NotRequired,
            Assert.Single(plan.Steps).Recovery.Kind);
    }
}
