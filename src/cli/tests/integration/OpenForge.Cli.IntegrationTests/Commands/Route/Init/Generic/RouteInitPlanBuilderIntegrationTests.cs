using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

public sealed class RouteInitPlanBuilderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Init plan formation emits the fixed generic scaffold and draft provenance"), Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task PlanFormationEmitsFixedGenericScaffoldAndDraftProvenance()
    {
        using var temporary = TemporaryWorkspace.Create("route-init-plan-draft");
        var build = await new RouteInitPlanBuilder().BuildAsync(
            Request(temporary, "memory/project-alpha/documents"),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteInitPlan>(build.Plan);
        var entrypoint = Assert.Single(
            plan.Preview.Entrypoints.Where(item =>
                item.Current == RouteInitEntrypointCurrent.Missing
                && item.Id == "memory/project-alpha/documents"));
        var change = Assert.Single(
            plan.FileChanges.Where(item =>
                item.Kind == PlannedFileChangeKind.Create
                && item.LogicalPath.EndsWith("_documents.md", StringComparison.Ordinal)));

        const string expected = """
            ---
            open-forge:
              description: Draft route for memory/project-alpha/documents; replace this description before relying on it for selection
              tags: [NeedsAuthoring]
            ---

            # documents

            Draft route for memory/project-alpha/documents; replace this description before relying on it for selection.

            ## Axioms

            - inherited - No local axioms; loaded ancestor axioms remain active.

            ## Entries

            - none - No entries - #Empty

            """;

        Assert.Equal(RouteInitEntrypointOwnership.User, entrypoint.Ownership);
        Assert.Equal(RouteInitEntrypointOutcome.Planned, entrypoint.Outcome);
        Assert.Equal(RouteInitDescriptionSource.Draft, entrypoint.Metadata?.DescriptionSource);
        Assert.Equal(RouteInitTagsSource.Draft, entrypoint.Metadata?.TagsSource);
        Assert.Equal(["NeedsAuthoring"], entrypoint.Metadata?.Tags);
        Assert.Equal(expected, Encoding.UTF8.GetString(change.IntendedBytes.AsSpan()));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Init plan formation applies explicit final metadata without changing ancestor draft provenance"), Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task PlanFormationAppliesExplicitFinalMetadataOnlyToFinalTarget()
    {
        using var temporary = TemporaryWorkspace.Create("route-init-plan-explicit");
        var metadata = new RouteInitMetadataInput(
            description: "Project documents",
            responsibilitySpecified: true,
            responsibility: "Owns project documents",
            tags: ["Docs", "Public"]);
        var build = await new RouteInitPlanBuilder().BuildAsync(
            Request(temporary, "memory/project-alpha/documents", metadata),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteInitPlan>(build.Plan);
        var final = Assert.Single(
            plan.Preview.Entrypoints.Where(item => item.Id == "memory/project-alpha/documents"));
        var ancestors = plan.Preview.Entrypoints
            .Where(item => item.Id != final.Id)
            .ToArray();

        Assert.Equal(RouteInitDescriptionSource.Explicit, final.Metadata?.DescriptionSource);
        Assert.Equal(RouteInitResponsibilitySource.Explicit, final.Metadata?.ResponsibilitySource);
        Assert.Equal(RouteInitTagsSource.Explicit, final.Metadata?.TagsSource);
        Assert.Equal("Project documents", final.Metadata?.Description);
        Assert.Equal("Owns project documents", final.Metadata?.Responsibility);
        Assert.Equal(["Docs", "Public"], final.Metadata?.Tags);
        Assert.All(ancestors, item =>
        {
            Assert.Equal(RouteInitDescriptionSource.Draft, item.Metadata?.DescriptionSource);
            Assert.Equal(RouteInitTagsSource.Draft, item.Metadata?.TagsSource);
        });
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Init plan formation rejects punctuation in a Framework scope label before effects"), Trait("Feature", "route-init"), Trait("Evidence", "Integration")]
    public async Task FrameworkPlanFormationRejectsPunctuationBeforeEffects()
    {
        using var temporary = TemporaryWorkspace.Create("route-init-plan-punctuation");
        var workspace = Workspace(temporary);
        var request = new RouteInitRequest(
            workspace,
            "memory/Mobile!App/working",
            RouteInitScaffold.Framework,
            RouteInitMode.DryRun,
            RouteInitMetadataInput.None);
        var build = await new RouteInitPlanBuilder().BuildAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Null(build.Plan);
        Assert.Equal(RouteInitPlanCompleteness.Incomplete, build.Formation.Plan.Completeness);
        Assert.Contains(
            build.Formation.Findings,
            finding => finding.Code == RouteInitFindingCode.InvalidTarget);
    }

    private static RouteInitRequest Request(
        TemporaryWorkspace temporary,
        string routeTarget,
        RouteInitMetadataInput? metadata = null)
        => new(
            Workspace(temporary),
            routeTarget,
            RouteInitScaffold.Generic,
            RouteInitMode.Apply,
            metadata ?? RouteInitMetadataInput.None);

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
