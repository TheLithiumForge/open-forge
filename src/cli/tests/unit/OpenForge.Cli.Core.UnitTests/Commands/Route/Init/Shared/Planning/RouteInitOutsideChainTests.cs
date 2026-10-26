using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Init.Shared.Planning;

[Trait("Feature", "route-init-outside"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteInitOutsideChainTests
{
    [Theory(DisplayName = "Init chain coordinates stop at the registered outside anchor and keep primary short-ID mapping")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public void OutsideAndPrimaryChainsHaveIndependentCoordinates(string root)
    {
        const string outside = "docs/team";
        var layout = WorkspaceLayout.Create(RouteInitRedTestData.Workspace(), root).WithOutsideRoutes([outside]);
        var path = WorkspaceRoutePath.CanonicalEntrypointPath(outside + "/child");
        var id = SourceWorkspaceRelativePath.LiteralReferencePrefix + path;
        var target = new RouteInitTargetFacts(id, RouteInitTargetKind.ExactPath,
            ["docs", "team", "child"], ["docs", "team", "child"], id, path);
        Assert.Equal([WorkspaceRoutePath.CanonicalEntrypointPath(outside), path], RouteInitTargetPlanner.ReadChainPaths(layout, target));
        Assert.False(layout.IsImplicitContentDestination(path));
        const string primaryId = "notes/child";
        var primary = new RouteInitTargetFacts(primaryId, RouteInitTargetKind.SourceId,
            ["notes", "child"], ["notes", "child"], primaryId, WorkspaceRoutePath.CanonicalEntrypointPath(layout.SourcePath(primaryId)));
        Assert.Equal([WorkspaceRoutePath.CanonicalEntrypointPath(layout.SourcePath("notes")),
            WorkspaceRoutePath.CanonicalEntrypointPath(layout.SourcePath(primaryId))], RouteInitTargetPlanner.ReadChainPaths(layout, primary));
    }
}
