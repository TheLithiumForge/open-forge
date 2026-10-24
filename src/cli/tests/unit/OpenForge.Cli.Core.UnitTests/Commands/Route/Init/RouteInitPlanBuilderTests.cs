using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init;

public sealed class RouteInitPlanBuilderTests
{
    private const string ScopeName = "release-notes";
    private const string StateName = "working";
    private const string TargetPath = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Memory
        + "/" + ScopeName + "/" + StateName + "/" + SourceFormClassifier.IndexFileName;
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
        var layout = WorkspaceLayout.Create(request.Workspace, WorkspaceLayout.DefaultMainRoot);
        var projected = new EmbeddedFrameworkSourceProjector().Project(layout, payload);
        var requested = new RouteInitTargetFacts(request.RouteTarget, RouteInitTargetKind.SourceId,
            request.RouteTarget.Split('/'), canonicalSegments: null, id: null, canonicalPath: null);
        var build = new RouteInitFrameworkAlignmentBuilder().Build(
            layout,
            requested,
            projected,
            payload);
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
        const string targetPath = TargetPath;
        var request = RouteInitRedTestData.Request(
            routeTarget: targetPath,
            scaffold: RouteInitScaffold.Framework,
            mode: RouteInitMode.DryRun);
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var layout = WorkspaceLayout.Create(request.Workspace, WorkspaceLayout.DefaultMainRoot);
        var projected = new EmbeddedFrameworkSourceProjector().Project(layout, payload);
        var segments = new[] { WorkspaceCategoryNames.Memory, ScopeName, StateName };
        var requested = new RouteInitTargetFacts(targetPath, RouteInitTargetKind.ExactPath,
            segments, segments, string.Join('/', segments), targetPath);

        var build = new RouteInitFrameworkAlignmentBuilder().Build(
            layout,
            requested,
            projected,
            payload);
        var alignment = Assert.IsType<RouteInitFrameworkAlignment>(build.Alignment);

        Assert.Equal(targetPath, alignment.Target.CanonicalPath);
        Assert.Equal(targetPath, alignment.Segments[^1].IntendedSource.Identity.CanonicalBasePath);
    }

}
