using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitPlanBuilderRedTests
{
    [Fact(DisplayName = "Route Init plan formation emits the fixed generic scaffold and draft provenance"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public async Task PlanFormationEmitsFixedGenericScaffoldAndDraftProvenance()
    {
        using var workspaceDirectory = TemporaryWorkspace.Create("route-init-plan-draft");
        var request = RouteInitRedTestData.Request(
            workspace: Workspace(workspaceDirectory),
            routeTarget: "memory/project-alpha/documents");
        var build = await new RouteInitPlanBuilder().BuildAsync(
            request,
            CancellationToken.None);
        Assert.NotNull(build.Plan);
        var plan = build.Plan!;
        var entrypoint = Assert.Single(
            plan.Preview.Entrypoints.Where(item =>
                item.Current == RouteInitEntrypointCurrent.Missing
                && item.Id == "memory/project-alpha/documents"));
        var change = Assert.Single(
            plan.FileChanges.Where(item =>
                item.Kind == OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.PlannedFileChangeKind.Create
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

            <!-- open-forge:generated-index:start -->
            - none - No entries - #Empty
            <!-- open-forge:generated-index:end -->
            """;

        Assert.Equal(RouteInitEntrypointOwnership.User, entrypoint.Ownership);
        Assert.Equal(RouteInitEntrypointOutcome.Planned, entrypoint.Outcome);
        Assert.Equal(RouteInitDescriptionSource.Draft, entrypoint.Metadata?.DescriptionSource);
        Assert.Equal(RouteInitTagsSource.Draft, entrypoint.Metadata?.TagsSource);
        Assert.Equal(["NeedsAuthoring"], entrypoint.Metadata?.Tags);
        Assert.Equal(expected, Encoding.UTF8.GetString(change.IntendedBytes.AsSpan()));
    }

    [Fact(DisplayName = "Route Init plan formation applies explicit final metadata without changing ancestor draft provenance"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public async Task PlanFormationAppliesExplicitFinalMetadataOnlyToFinalTarget()
    {
        using var workspaceDirectory = TemporaryWorkspace.Create("route-init-plan-explicit");
        var request = RouteInitRedTestData.Request(
            workspace: Workspace(workspaceDirectory),
            routeTarget: "memory/project-alpha/documents",
            metadata: RouteInitRedTestData.Metadata(
                description: "Project documents",
                responsibilitySpecified: true,
                responsibility: "Owns project documents",
                tags: ["Docs", "Public"]));
        var build = await new RouteInitPlanBuilder().BuildAsync(
            request,
            CancellationToken.None);
        Assert.NotNull(build.Plan);
        var plan = build.Plan!;
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

    [Fact(DisplayName = "Route Init Framework plan formation slugifies inserted Unicode scope labels and preserves managed segment order"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FrameworkPlanFormationSlugifiesInsertedUnicodeScopes()
    {
        var request = RouteInitRedTestData.Request(
            routeTarget: "memory/Überblick 2026/crystallized/documents",
            scaffold: RouteInitScaffold.Framework,
            mode: RouteInitMode.DryRun);
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var projected = new EmbeddedFrameworkSourceProjector().Project(request.Workspace, payload);
        var requested = new RouteInitTargetPlanner().Resolve(request).Target
            ?? throw new InvalidOperationException("The Framework target operand was not resolved.");
        var build = new RouteInitFrameworkAlignmentBuilder().Build(
            request.Workspace,
            requested,
            projected);
        var alignment = Assert.IsType<RouteInitFrameworkAlignment>(build.Alignment);

        Assert.Equal(
            "memory/überblick-2026/crystallized/documents",
            alignment.Target.Id);
        Assert.Equal(
            ["memory", "überblick-2026", "crystallized", "documents"],
            alignment.Segments.Select(item => item.ConcreteSegment));
        Assert.Equal(
            [RouteInitFrameworkSegmentRole.InstalledRoot, RouteInitFrameworkSegmentRole.Scope, RouteInitFrameworkSegmentRole.Managed, RouteInitFrameworkSegmentRole.Managed],
            alignment.Segments.Select(item => item.Role));
    }

    [Fact(DisplayName = "Route Init Framework alignment preserves an exact compatibility target path"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FrameworkAlignmentPreservesExactCompatibilityTargetPath()
    {
        const string targetPath = ".agents/memory/release-notes/crystallized/documents/index.md";
        var request = RouteInitRedTestData.Request(
            routeTarget: targetPath,
            scaffold: RouteInitScaffold.Framework,
            mode: RouteInitMode.DryRun);
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var projected = new EmbeddedFrameworkSourceProjector().Project(request.Workspace, payload);
        var requested = new RouteInitTargetPlanner().Resolve(request).Target
            ?? throw new InvalidOperationException("The Framework target operand was not resolved.");

        var build = new RouteInitFrameworkAlignmentBuilder().Build(
            request.Workspace,
            requested,
            projected);
        var alignment = Assert.IsType<RouteInitFrameworkAlignment>(build.Alignment);

        Assert.Equal(targetPath, alignment.Target.CanonicalPath);
        Assert.Equal(targetPath, alignment.Segments[^1].IntendedSource.Identity.CanonicalBasePath);
    }

    [Fact(DisplayName = "Route Init plan formation rejects punctuation in a Framework scope label before effects"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public async Task FrameworkPlanFormationRejectsPunctuationBeforeEffects()
    {
        var request = RouteInitRedTestData.Request(
            routeTarget: "memory/Mobile!App/crystallized/documents",
            scaffold: RouteInitScaffold.Framework,
            mode: RouteInitMode.DryRun);
        var build = await new RouteInitPlanBuilder().BuildAsync(
            request,
            CancellationToken.None);

        Assert.Null(build.Plan);
        Assert.Equal(RouteInitPlanCompleteness.Incomplete, build.Formation.Plan.Completeness);
        Assert.Contains(
            build.Formation.Findings,
            finding => finding.Code == RouteInitFindingCode.InvalidTarget);
    }

    private static CliWorkspace Workspace(TemporaryWorkspace workspace)
        => new(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
