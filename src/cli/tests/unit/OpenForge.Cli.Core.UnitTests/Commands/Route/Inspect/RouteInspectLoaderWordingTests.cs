using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Wording;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect;

public sealed class RouteInspectLoaderWordingTests
{
    private const string RelatedId = "context/start";
    private const string MainRoot = "knowledge/context";

    [Theory(DisplayName = "Route Inspect describes the projected loader role without interpreting its identity"),
     InlineData(true), InlineData(false), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void UsesProjectedLoaderRole(bool isLoader)
    {
        var reading = new RouteInspectAutomaticReading(RouteInspectAutomaticReadingKind.ParentLoadNow,
            RelatedId, [RouteInspectAutomaticReadingEvent.ExposingParentRead])
        {
            RelatedSourceIsLoader = isLoader,
        };

        CommandOutputSnapshot.MatchSnapshot(RouteInspectWording.AutomaticExplanation(reading), isLoader ? "loader" : "route");
    }

    [Fact(DisplayName = "Route Inspect maps unsafe custom loader and native Skills boundaries with their observed paths"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MapsObservedBoundaries()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), nameof(RouteInspectLoaderWordingTests)));
        var layout = WorkspaceLayout.Create(new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory), MainRoot);
        var loaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var catalogue = new SourceCatalogue(layout, [], [],
            [new SourceCatalogueIssue(SourceCatalogueIssueCode.CandidateUnsafe, loaderPath, [], root, null),
             new SourceCatalogueIssue(SourceCatalogueIssueCode.RootUnsafe, WorkspaceLayout.NativeSkillsPath, [], null, null)], false);

        var issues = RouteInspectResolutionSupport.ReadCatalogueBoundaryIssues(catalogue);

        Assert.Equal(2, issues.Count);
        Assert.All(issues, issue => Assert.Equal(RouteInspectResolutionIssueCode.UnsafeSource, issue.Code));
        Assert.Equal(new[] { WorkspaceLayout.NativeSkillsPath, loaderPath }.Order(StringComparer.Ordinal), issues.Select(issue => issue.Subject).Order(StringComparer.Ordinal));
        CommandOutputSnapshot.MatchSnapshot(string.Join('\n', issues.Select(issue => issue.Message)), "custom-boundaries");
    }
}
