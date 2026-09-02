using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdatePlanningIntegrationTests
{
    [Theory(DisplayName = "Route Update description and tags regenerate only their dependent navigation")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task NavigationDependenciesAreMinimal(bool updateDescription)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            updateDescription
                ? "route-update-description-navigation"
                : "route-update-tags-navigation");
        var patch = updateDescription
            ? RouteUpdateIntegrationWorkspace.DescriptionPatch("After overview")
            : RouteUpdateIntegrationWorkspace.TagsPatch("After", "Memory");
        var before = workspace.SnapshotHashes();

        var build = await workspace.BuildPlanAsync(workspace.Request(patch: patch));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(
            [
                workspace.Absolute(RouteUpdateIntegrationWorkspace.TargetPath),
                workspace.Absolute(RouteUpdateIntegrationWorkspace.ParentPath),
            ],
            plan.FileChanges.Select(change => change.LogicalPath));
        Assert.Equal(
            [RouteUpdateIntegrationWorkspace.TargetPath, RouteUpdateIntegrationWorkspace.ParentPath],
            build.Formation.Effects.Select(effect => effect.Path));
        Assert.Equal(2, build.Formation.Effects.Length);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Update responsibility-only patch schedules no generated work")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task ResponsibilityHasNoNavigationDependency()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-responsibility-minimal");

        var build = await workspace.BuildPlanAsync(workspace.Request(
            patch: RouteUpdateIntegrationWorkspace.ResponsibilityPatch(
                "Owns the revised overview")));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        var change = Assert.Single(plan.FileChanges);
        Assert.Equal(
            workspace.Absolute(RouteUpdateIntegrationWorkspace.TargetPath),
            change.LogicalPath);
        Assert.Equal(
            RouteUpdateIntegrationWorkspace.TargetPath,
            Assert.Single(build.Formation.Effects).Path);
        Assert.DoesNotContain(
            build.Formation.Effects,
            effect => effect.Kind == RouteUpdateEffectKind.GeneratedRegion);
    }

    [Fact(DisplayName = "Route Update coalesces target metadata and self-region work before the parent")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task TargetSelfRegionIsOneOrderedPhysicalEffect()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-self-region-coalescing");
        workspace.SeedSelfRegionTarget();

        var build = await workspace.BuildPlanAsync(workspace.Request(
            sourceReference: ".agents/memory/project-alpha/overview/_overview.md"));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(
            [
                workspace.Absolute(".agents/memory/project-alpha/overview/_overview.md"),
                workspace.Absolute(RouteUpdateIntegrationWorkspace.ParentPath),
            ],
            plan.FileChanges.Select(change => change.LogicalPath));
        Assert.Equal(2, plan.FileChanges.Length);
        Assert.Equal(
            [
                ".agents/memory/project-alpha/overview/_overview.md",
                RouteUpdateIntegrationWorkspace.ParentPath,
            ],
            build.Formation.Effects.Select(effect => effect.Path));
    }

    [Fact(DisplayName = "Route Update always mutates base bytes and preserves the overwrite companion")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task OverwriteSelectionStillPlansBaseOnly()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-base-only-overwrite");
        workspace.SeedOverwrite();
        var overwriteBefore = workspace.ReadText(RouteUpdateIntegrationWorkspace.OverwritePath);

        var build = await workspace.BuildPlanAsync(workspace.Request(
            sourceReference: RouteUpdateIntegrationWorkspace.OverwritePath));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Contains(
            plan.FileChanges,
            change => change.LogicalPath
                == workspace.Absolute(RouteUpdateIntegrationWorkspace.TargetPath));
        Assert.DoesNotContain(
            plan.FileChanges,
            change => change.LogicalPath
                == workspace.Absolute(RouteUpdateIntegrationWorkspace.OverwritePath));
        Assert.Contains(
            build.Formation.Effects,
            effect => effect.Path == RouteUpdateIntegrationWorkspace.TargetPath);
        Assert.DoesNotContain(
            build.Formation.Effects,
            effect => effect.Path == RouteUpdateIntegrationWorkspace.OverwritePath);
        Assert.Equal(
            overwriteBefore,
            workspace.ReadText(RouteUpdateIntegrationWorkspace.OverwritePath));
    }

    [Theory(DisplayName = "Route Update Template copies an exact empty-body completion and protects authored body")]
    [InlineData(true, (int)RouteUpdateBodyState.TemplateCopied, (int)RouteUpdateTemplateDecision.Copied)]
    [InlineData(false, (int)RouteUpdateBodyState.AuthoredBodyProtected, (int)RouteUpdateTemplateDecision.AuthoredBodyProtected)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task TemplateDecisionDependsOnlyOnExistingBody(
        bool emptyTargetBody,
        int expectedBodyValue,
        int expectedDecisionValue)
    {
        var expectedBody = (RouteUpdateBodyState)expectedBodyValue;
        var expectedDecision = (RouteUpdateTemplateDecision)expectedDecisionValue;
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            emptyTargetBody
                ? "route-update-template-copy"
                : "route-update-template-protect");
        workspace.SeedTemplate();
        if (emptyTargetBody)
        {
            workspace.SeedEmptyBodyTarget();
        }

        var build = await workspace.BuildPlanAsync(workspace.Request(
            templateReference: RouteUpdateIntegrationWorkspace.TemplateId));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(expectedBody, build.Formation.Plan.Body);
        Assert.Equal(expectedDecision, build.Formation.Template?.Decision);
        Assert.Equal(
            emptyTargetBody,
            plan.Destination.IntendedTargetBytes.AsSpan().EndsWith(
                System.Text.Encoding.UTF8.GetBytes(RouteUpdateIntegrationWorkspace.TemplateBody)));
    }

    [Fact(DisplayName = "Route Update ordinary Template-only copy has no navigation dependency")]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task OrdinaryTemplateOnlyCopyHasNoNavigationDependency()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-ordinary-minimal");
        workspace.SeedEmptyBodyTarget();
        workspace.SeedTemplate();
        var before = workspace.SnapshotHashes();

        var build = await workspace.BuildPlanAsync(workspace.Request(
            patch: RouteUpdateIntegrationWorkspace.Patch(),
            templateReference: RouteUpdateIntegrationWorkspace.TemplateId));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(RouteUpdateBodyState.TemplateCopied, plan.Destination.Body.State);
        Assert.Empty(plan.Navigation.Regions);
        Assert.Equal(
            workspace.Absolute(RouteUpdateIntegrationWorkspace.TargetPath),
            Assert.Single(plan.FileChanges).LogicalPath);
        Assert.Equal(
            RouteUpdateIntegrationWorkspace.TargetPath,
            Assert.Single(build.Formation.Effects).Path);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Route Update no-op and dry-run planning never write or create recovery")]
    [InlineData(false, "Before overview", true)]
    [InlineData(true, "After overview", false)]
    [Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task PlanningModesAreWriteFree(
        bool dryRun,
        string description,
        bool expectedNoOp)
    {
        var mode = dryRun ? RouteUpdateMode.DryRun : RouteUpdateMode.Apply;
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-plan-{mode.ToString().ToLowerInvariant()}");
        var before = workspace.SnapshotHashes();

        var build = await workspace.BuildPlanAsync(workspace.Request(
            patch: RouteUpdateIntegrationWorkspace.DescriptionPatch(description),
            mode: mode));

        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(expectedNoOp, plan.IsNoOp);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, build.Formation.Recovery.State);
    }
}
