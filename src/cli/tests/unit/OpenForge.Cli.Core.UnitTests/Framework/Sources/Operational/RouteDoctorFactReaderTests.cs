using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

public sealed class RouteDoctorFactReaderTests
{
    [Fact(DisplayName = "Route Doctor facts retain the exact Loader-declared missing root")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void DeclaredRootReaderRetainsMissingDestinationAndLoader()
    {
        var routes = new SourceRouteFacts(
            new SourceRouteTopology([], []),
            [],
            [new SourceRouteIssue(
                SourceRouteIssueCode.LoaderDestinationMissing,
                ".agents/missing/AGENTS.md",
                [".agents/loader.md"],
                occurrence: 0,
                "The Loader destination is missing.")],
            areLoaderRootFactsComplete: false,
            isCancelled: false);

        var root = Assert.Single(RouteDoctorFactReader.ReadDeclaredRoots(routes));

        Assert.Equal(RouteDeclaredRootState.Missing, root.State);
        Assert.Equal(".agents/missing/AGENTS.md", root.Path);
        Assert.Equal(".agents/loader.md", root.LoaderPath);
    }

    [Fact(DisplayName = "Route Doctor facts distinguish unreachable leaves from detached trees")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ShapeReaderClassifiesOnlyMaterializedRouteRelationships()
    {
        var tree = Identity("tree", ".agents/tree/AGENTS.md");
        var child = Identity("tree/child", ".agents/tree/child.md");
        var orphan = Identity("orphan", ".agents/orphan.md");
        var topology = new SourceRouteTopology(
        [
            new SourceRouteNode(
                tree,
                SourceRouteParentState.None,
                [],
                [child.CanonicalBasePath]),
            new SourceRouteNode(
                child,
                SourceRouteParentState.Resolved,
                [tree.CanonicalBasePath],
                []),
            new SourceRouteNode(
                orphan,
                SourceRouteParentState.None,
                [],
                []),
        ], []);
        var routes = new SourceRouteFacts(
            topology,
            [
                new SourceRouteFact(tree, SourceRouteState.Unrouted, isIdentityUnique: true),
                new SourceRouteFact(child, SourceRouteState.Unrouted, isIdentityUnique: true),
                new SourceRouteFact(orphan, SourceRouteState.Unrouted, isIdentityUnique: true),
            ],
            [],
            areLoaderRootFactsComplete: true,
            isCancelled: false);
        var workspace = OpenForge.Cli.Core.UnitTests.Commands.Doctor.DoctorOperationTestSupport.Workspace();
        var inspection = new RouteSourceInspection
        {
            Catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false),
            Routes = routes,
            WorkspaceEntry = new RouteSourceLayerObservation(
                "AGENTS.md",
                FileReadState.Missing,
                Text: null),
            Sources = [],
            State = OperationalViewState.Complete,
        };

        var shape = RouteDoctorFactReader.ReadShape(inspection);

        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Detached
            && observation.Path == tree.CanonicalBasePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == orphan.CanonicalBasePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == child.CanonicalBasePath);
    }

    private static SourceLogicalIdentity Identity(string id, string path)
        => new(id, path);
}
