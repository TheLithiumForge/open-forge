using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Topology;

public sealed class RouteTopologyModelTests
{
    [Fact(DisplayName = "Route topology models retain reciprocal parent-child facts and deterministic lookup")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void TopologyFactsSnapshotValidatedRelationships()
    {
        var root = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var child = RouteSourceTestData.Source(".agents/root/child.md", RouteSourceKind.Markdown);
        var rootNode = new RouteTopologyNode(
            root,
            RouteTopologyParentState.None,
            [],
            [child.CanonicalPath]);
        var childNode = new RouteTopologyNode(
            child,
            RouteTopologyParentState.Resolved,
            [root.CanonicalPath],
            []);
        var facts = new RouteTopologyFacts([childNode, rootNode], [root.CanonicalPath]);

        Assert.Equal([root.CanonicalPath, child.CanonicalPath], facts.Nodes.Select(node => node.Source.CanonicalPath));
        Assert.Same(childNode, facts.FindByPath(child.CanonicalPath));
        Assert.Equal(root.CanonicalPath, childNode.ParentPath);
        Assert.True(rootNode.HasCompleteMetadata);
        Assert.Equal([root.CanonicalPath], facts.LoaderRootPaths);
    }

    [Fact(DisplayName = "Route topology node models reject Loader sources and child paths on leaves")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void TopologyNodesRequireRoutedSourceShapes()
    {
        var loader = RouteSourceTestData.Source(".agents/loader.md", RouteSourceKind.Loader);
        Assert.Throws<ArgumentException>(() => new RouteTopologyNode(loader, RouteTopologyParentState.None, [], []));

        var leaf = RouteSourceTestData.Source(".agents/root/leaf.md", RouteSourceKind.Markdown);
        Assert.Throws<ArgumentException>(() => new RouteTopologyNode(
            leaf,
            RouteTopologyParentState.None,
            [],
            [".agents/root/child.md"]));
    }

    [Fact(DisplayName = "Route topology models reject nonreciprocal relationships and invalid Loader roots")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void TopologyFactsRequireReciprocalRoots()
    {
        var root = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var child = RouteSourceTestData.Source(".agents/root/child.md", RouteSourceKind.Markdown);
        var rootNode = new RouteTopologyNode(root, RouteTopologyParentState.None, [], []);
        var childNode = new RouteTopologyNode(child, RouteTopologyParentState.Resolved, [root.CanonicalPath], []);

        Assert.Throws<ArgumentException>(() => new RouteTopologyFacts([rootNode, childNode], [child.CanonicalPath]));
        Assert.Throws<ArgumentException>(() => new RouteTopologyFacts([rootNode], [".agents/missing/_missing.md"]));
    }

    [Fact(DisplayName = "Route topology models reject a resolved parent cycle")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CyclesAreNotAValidTopologyFact()
    {
        var first = RouteSourceTestData.Source(".agents/first/_first.md", RouteSourceKind.Entrypoint);
        var second = RouteSourceTestData.Source(".agents/second/_second.md", RouteSourceKind.Entrypoint);
        var firstNode = new RouteTopologyNode(
            first,
            RouteTopologyParentState.Resolved,
            [second.CanonicalPath],
            [second.CanonicalPath]);
        var secondNode = new RouteTopologyNode(
            second,
            RouteTopologyParentState.Resolved,
            [first.CanonicalPath],
            [first.CanonicalPath]);

        Assert.Throws<ArgumentException>(() => new RouteTopologyFacts([firstNode, secondNode], [first.CanonicalPath]));
    }
}
