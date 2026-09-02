using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

public sealed class RouteCreatePlanningIntegrationTests
{
    public enum PlanningScenario
    {
        Apply,
        DryRun,
        Identical,
        Differing,
    }

    [Fact(DisplayName = "Route Create Template resolution copies only the exact classified body"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task TemplateResolutionCopiesOnlyExactClassifiedBody()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-template-resolution");
        workspace.SeedBase();
        workspace.SeedTemplate();
        var catalogue = await workspace.ReadCatalogueAsync();

        var resolution = await new RouteCreateTemplateResolver().ResolveAsync(
            workspace.Request(templateReference: RouteCreateIntegrationWorkspace.TemplateId),
            catalogue,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteCreateTemplateResolutionState.Resolved, resolution.State);
        Assert.Equal(RouteCreateIntegrationWorkspace.TemplateId, resolution.Template?.Id);
        Assert.Equal(RouteCreateIntegrationWorkspace.TemplatePath, resolution.Template?.Path);
        Assert.Equal(
            RouteCreateIntegrationWorkspace.TemplateBody,
            System.Text.Encoding.UTF8.GetString(resolution.BodyBytes.AsSpan()));
    }

    [Theory(DisplayName = "Route Create Template resolution rejects missing or malformed sources"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    [InlineData(false, (int)RouteCreateFindingCode.InvalidTemplate)]
    [InlineData(true, (int)RouteCreateFindingCode.MetadataUnsafe)]
    public async Task TemplateResolutionRejectsMissingOrMalformedSources(
        bool seedMalformedTemplate,
        int expectedFindingValue)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            seedMalformedTemplate
                ? "route-create-template-malformed"
                : "route-create-template-missing");
        workspace.SeedBase();
        if (seedMalformedTemplate)
        {
            workspace.SeedMalformedTemplate();
        }

        var catalogue = await workspace.ReadCatalogueAsync();

        var resolution = await new RouteCreateTemplateResolver().ResolveAsync(
            workspace.Request(templateReference: RouteCreateIntegrationWorkspace.TemplateId),
            catalogue,
            TestContext.Current.CancellationToken);

        Assert.NotNull(resolution.Finding);
        Assert.Equal((RouteCreateFindingCode)expectedFindingValue, resolution.Finding.Code);
        Assert.Null(resolution.Template);
        Assert.Empty(resolution.BodyBytes);
    }

    [Theory(DisplayName = "Route Create planning classifies complete, dry-run, no-op, and collision states"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    [InlineData(PlanningScenario.Apply)]
    [InlineData(PlanningScenario.DryRun)]
    [InlineData(PlanningScenario.Identical)]
    [InlineData(PlanningScenario.Differing)]
    public async Task PlanningClassifiesAcceptedWorkspaceStates(
        PlanningScenario scenario)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            $"route-create-plan-{scenario.ToString().ToLowerInvariant()}");
        workspace.SeedBase();
        SeedScenario(workspace, scenario);
        var before = workspace.SnapshotHashes();
        var mode = scenario == PlanningScenario.DryRun
            ? RouteCreateMode.DryRun
            : RouteCreateMode.Apply;

        var build = await new RouteCreatePlanBuilder().BuildAsync(
            workspace.Request(mode),
            TestContext.Current.CancellationToken);

        Assert.Equal(before, workspace.SnapshotHashes());
        switch (scenario)
        {
            case PlanningScenario.Apply:
            case PlanningScenario.DryRun:
                Assert.NotNull(build.Plan);
                Assert.Equal(2, build.Plan.FileChanges.Length);
                Assert.Equal(RouteCreatePlanCompleteness.Complete, build.Formation.Plan.Completeness);
                Assert.Equal(RouteCreatePlanSafety.Safe, build.Formation.Plan.Safety);
                break;
            case PlanningScenario.Identical:
                Assert.NotNull(build.Plan);
                Assert.True(build.Plan.IsNoOp);
                Assert.Equal(RouteCreateVerificationState.Verified, build.Formation.Verification);
                break;
            case PlanningScenario.Differing:
                Assert.Null(build.Plan);
                Assert.Contains(
                    build.Formation.Findings,
                    finding => finding.Code == RouteCreateFindingCode.TargetContentDiffers);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(scenario),
                    scenario,
                    "The Route Create planning scenario is not defined.");
        }
    }

    [Theory(DisplayName = "Route Create planning blocks missing or ambiguous parents before effects"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    [InlineData(false, (int)RouteCreateFindingCode.ParentMissing)]
    [InlineData(true, (int)RouteCreateFindingCode.RouteAmbiguous)]
    public async Task PlanningBlocksMissingOrAmbiguousParents(
        bool seedAmbiguousParent,
        int expectedFindingValue)
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            seedAmbiguousParent
                ? "route-create-parent-ambiguous"
                : "route-create-parent-missing");
        workspace.SeedBase();
        if (seedAmbiguousParent)
        {
            workspace.SeedAmbiguousParent();
        }
        else
        {
            workspace.RemoveParent();
        }

        var before = workspace.SnapshotHashes();
        var build = await new RouteCreatePlanBuilder().BuildAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        Assert.Null(build.Plan);
        Assert.Contains(
            build.Formation.Findings,
            finding => finding.Code == (RouteCreateFindingCode)expectedFindingValue);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Create planning blocks a physical alias at the target path"), Trait("Feature", "route-create"), Trait("Evidence", "IntegrationBehavior")]
    public async Task PlanningBlocksPhysicalAliasAtTarget()
    {
        using var workspace = RouteCreateIntegrationWorkspace.Create(
            "route-create-target-alias");
        workspace.SeedBase();
        workspace.SeedUnsafeTargetAlias();
        var before = workspace.SnapshotHashes();

        var build = await new RouteCreatePlanBuilder().BuildAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        Assert.Null(build.Plan);
        Assert.Contains(
            build.Formation.Findings,
            finding => finding.Code == RouteCreateFindingCode.IdentityCollision);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void SeedScenario(
        RouteCreateIntegrationWorkspace workspace,
        PlanningScenario scenario)
    {
        switch (scenario)
        {
            case PlanningScenario.Apply:
            case PlanningScenario.DryRun:
                return;
            case PlanningScenario.Identical:
                workspace.SeedCompleteTarget();
                return;
            case PlanningScenario.Differing:
                workspace.SeedDifferingTarget();
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(scenario),
                    scenario,
                    "The Route Create planning scenario is not defined.");
        }
    }
}
