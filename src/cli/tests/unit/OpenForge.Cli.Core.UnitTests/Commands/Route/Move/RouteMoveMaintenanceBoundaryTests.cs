using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

[Trait("Feature", "route-move"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteMoveMaintenanceBoundaryTests
{
    [Theory(DisplayName = "Outside move authority compares exact registration coordinates while primary native moves remain admitted")]
    [InlineData("docs/a/old.md", "docs/a/new.md", true)]
    [InlineData("docs/a/old.md", "docs/b/new.md", false)]
    [InlineData("docs/a/old.md", "DOCS/a/new.md", false)]
    [InlineData("docs/a/old.md", "docs/ab/new.md", false)]
    [InlineData("docs/a/old.md", ".agents/guidance/new.md", false)]
    [InlineData(".agents/guidance/old.md", "docs/a/new.md", false)]
    [InlineData(".agents/guidance/old.md", ".agents/skills/workshop/references/new.md", true)]
    public void OrdinalScope(string source, string destination, bool allowed)
    {
        var root = Path.GetFullPath("move-pure-layout");
        var layout = WorkspaceLayout.Create(new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace), WorkspaceLayout.DefaultMainRoot)
            .WithOutsideRoutes(["docs/a", "docs/b"]);
        Assert.Equal(allowed, RouteMoveMaintenanceBoundaryInspector.SameBoundary(layout, source, destination));
    }
}
