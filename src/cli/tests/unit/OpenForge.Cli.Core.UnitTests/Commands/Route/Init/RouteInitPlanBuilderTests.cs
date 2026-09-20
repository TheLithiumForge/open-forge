using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitPlanBuilderTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Init Framework plan formation slugifies inserted Unicode scope labels and preserves managed segment order"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FrameworkPlanFormationSlugifiesInsertedUnicodeScopes()
    {
        var request = RouteInitRedTestData.Request(
            routeTarget: "memory/Überblick 2026/working",
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
            "memory/überblick-2026/working",
            alignment.Target.Id);
        Assert.Equal(
            ["memory", "überblick-2026", "working"],
            alignment.Segments.Select(item => item.ConcreteSegment));
        Assert.Equal(
            [RouteInitFrameworkSegmentRole.InstalledRoot, RouteInitFrameworkSegmentRole.Scope, RouteInitFrameworkSegmentRole.Managed],
            alignment.Segments.Select(item => item.Role));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Init Framework alignment preserves an exact compatibility target path"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void FrameworkAlignmentPreservesExactCompatibilityTargetPath()
    {
        const string targetPath = ".agents/memory/release-notes/working/index.md";
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

}
