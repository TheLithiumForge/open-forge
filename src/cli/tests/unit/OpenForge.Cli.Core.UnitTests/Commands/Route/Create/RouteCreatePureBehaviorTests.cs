using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreatePureBehaviorTests
{
    [Fact(DisplayName = "Route Create target planner resolves ordinary ID and exact path"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void TargetPlannerResolvesOrdinaryIdAndExactPath()
    {
        var planner = new RouteCreateTargetPlanner();

        var id = planner.Resolve(RouteCreateTestData.Request());
        var path = planner.Resolve(RouteCreateTestData.Request(fileTarget: RouteCreateTestData.TargetPath));
        var unicodePath = planner.Resolve(RouteCreateTestData.Request(
            fileTarget: ".agents/memory/Überblick.md"));

        Assert.Equal(RouteCreateTestData.TargetId, id.Target.Id);
        Assert.Equal(RouteCreateTestData.TargetPath, id.Target.Path);
        Assert.Equal(RouteCreateTestData.TargetId, path.Target.Id);
        Assert.Equal(RouteCreateTestData.TargetPath, path.Target.Path);
        Assert.Equal("memory/Überblick", unicodePath.Target.Id);
        Assert.Equal(".agents/memory/Überblick.md", unicodePath.Target.Path);

        foreach (var invalidTarget in new[]
        {
            ".agents/loader.md",
            ".agents/memory/_memory.md",
            ".agents/memory/overview.overwrite.md",
            ".agents/skills/example/SKILL.md",
            ".agents/memory/data.json",
            "../outside.md",
        })
        {
            var invalid = planner.Resolve(RouteCreateTestData.Request(fileTarget: invalidTarget));
            Assert.Equal(RouteCreateTargetResolutionState.Invalid, invalid.State);
        }
    }

    [Fact(DisplayName = "Route Create plan equivalence rejects a changed plan"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void PlanEquivalenceRejectsChangedPlan()
    {
        var plan = RouteCreateTestData.Plan();
        var changed = plan with
        {
            IntendedTargetBytes = ImmutableArray.Create<byte>(1, 2, 3),
        };

        Assert.False(RouteCreatePlanEquivalence.Matches(plan, changed));
    }

    [Fact(DisplayName = "Route Create application result factory preserves plan and progress facts"), Trait("Feature", "route-create"), Trait("Evidence", "UnitBehavior")]
    public void ApplicationResultFactoryPreservesPlanAndProgressFacts()
    {
        var plan = RouteCreateTestData.Plan();

        var formation = RouteCreateApplicationResultFactory.Build(
            plan,
            RouteCreateTestData.ApplicationProgress(plan));

        Assert.Equal(plan.Preview.Target, formation.Target);
        Assert.Equal(RouteCreateVerificationState.Verified, formation.Verification);
    }
}
