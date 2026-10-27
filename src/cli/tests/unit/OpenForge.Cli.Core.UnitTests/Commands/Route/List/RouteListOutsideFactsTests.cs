using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List;

public sealed class RouteListOutsideFactsTests
{
    [Fact]
    public void RowCopiesPreserveIndependentRegistrationAndMissingFacts()
    {
        var original = RouteListRow.Unlisted("docs/team/child.md").WithMaintenanceRoot("docs/team");
        var copy = original.AsMissing().WithDirectChildCount(null);
        Assert.True(copy.IsUnlisted);
        Assert.True(copy.IsMissing);
        Assert.Equal(original.Id, copy.Id);
        Assert.Equal(original.Path, copy.Path);
        Assert.Equal(original.MaintenanceRoot, copy.MaintenanceRoot);
        Assert.Null(original.WithMaintenanceRoot(null).MaintenanceRoot);
    }

    [Theory]
    [InlineData("docs/team/child.md", "docs/team")]
    [InlineData("docs/team", "docs/team")]
    [InlineData("docs/teamish/child.md", null)]
    [InlineData("Docs/team/child.md", null)]
    [InlineData(".agents/guide/child.md", null)]
    public void MaintenanceCoordinateUsesOrdinalDeclaredBoundaries(string path, string? expected)
    {
        var workspace = new CliWorkspace("/workspace", "/workspace", CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot).WithOutsideRoutes(["docs/team"]);
        Assert.Equal(expected, RouteOutsideRouteReader.MaintenanceRoot(layout, path));
        Assert.Null(RouteOutsideRouteReader.MaintenanceRoot(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), path));
        Assert.False(layout.IsImplicitContentDestination("docs/team/child.md"));
        Assert.True(layout.IsImplicitContentDestination(".agents/guide/child.md"));
    }
}
