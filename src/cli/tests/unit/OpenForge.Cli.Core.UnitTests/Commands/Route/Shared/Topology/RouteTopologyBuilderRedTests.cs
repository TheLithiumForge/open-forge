using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Commands.Route.Shared.Topology;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Topology;

public sealed class RouteTopologyBuilderRedTests
{
    [Fact(DisplayName = "Route topology builder derives authored parent and child relationships from fixed source facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AuthoredRelationshipsAreParentFirst()
    {
        var root = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var child = RouteSourceTestData.Source(".agents/root/child/_child.md", RouteSourceKind.Entrypoint);
        var grandchild = RouteSourceTestData.Source(".agents/root/child/grand.md", RouteSourceKind.Markdown);
        var leaf = RouteSourceTestData.Source(".agents/root/leaf.md", RouteSourceKind.Markdown);
        var native = RouteSourceTestData.Source(".agents/root/native/SKILL.md", RouteSourceKind.Native);

        var topology = new RouteTopologyBuilder().Build(
            [grandchild, native, leaf, child, root],
            [root.CanonicalPath]);

        Assert.Equal(
            [child.CanonicalPath, leaf.CanonicalPath, native.CanonicalPath],
            topology.FindByPath(root.CanonicalPath)!.ChildPaths);
        Assert.Equal([grandchild.CanonicalPath], topology.FindByPath(child.CanonicalPath)!.ChildPaths);
        Assert.Equal(root.CanonicalPath, topology.FindByPath(child.CanonicalPath)!.ParentPath);
        Assert.Equal(child.CanonicalPath, topology.FindByPath(grandchild.CanonicalPath)!.ParentPath);
        Assert.Equal([root.CanonicalPath], topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Route topology builder keeps Loader roots separate from detached roots")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DetachedRootsHaveNoFabricatedLoaderParent()
    {
        var loaderRoot = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var detached = RouteSourceTestData.Source(".agents/detached/_detached.md", RouteSourceKind.Entrypoint);
        var detachedLeaf = RouteSourceTestData.Source(".agents/detached/leaf.md", RouteSourceKind.Markdown);

        var topology = new RouteTopologyBuilder().Build(
            [detachedLeaf, detached, loaderRoot],
            [loaderRoot.CanonicalPath]);

        var detachedNode = topology.FindByPath(detached.CanonicalPath)!;
        Assert.Equal(RouteTopologyParentState.None, detachedNode.ParentState);
        Assert.Empty(detachedNode.ParentPaths);
        Assert.Equal([detachedLeaf.CanonicalPath], detachedNode.ChildPaths);
        Assert.DoesNotContain(detached.CanonicalPath, topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Route topology builder retains every ambiguous parent candidate in ordinal path order")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void AmbiguousParentageIsNotResolvedByOrder()
    {
        var root = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var canonical = RouteSourceTestData.Source(
            ".agents/root/ambiguous/_ambiguous.md",
            RouteSourceKind.Entrypoint);
        var compatibility = RouteSourceTestData.Source(
            ".agents/root/ambiguous/index.md",
            RouteSourceKind.Entrypoint,
            RouteSourceForm.IndexEntrypoint);
        var leaf = RouteSourceTestData.Source(".agents/root/ambiguous/leaf.md", RouteSourceKind.Markdown);

        var topology = new RouteTopologyBuilder().Build(
            [leaf, compatibility, root, canonical],
            [root.CanonicalPath]);

        var leafNode = topology.FindByPath(leaf.CanonicalPath)!;
        Assert.Equal(RouteTopologyParentState.Ambiguous, leafNode.ParentState);
        Assert.Equal(
            [canonical.CanonicalPath, compatibility.CanonicalPath],
            leafNode.ParentPaths);
    }

    [Fact(DisplayName = "Route topology builder retains identity collisions for the source catalogue")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IdentityCollisionsRemainDistinctTopologyNodes()
    {
        var root = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var leaf = RouteSourceTestData.Source(".agents/root/collision.md", RouteSourceKind.Markdown);
        var entrypoint = RouteSourceTestData.Source(
            ".agents/root/collision/_collision.md",
            RouteSourceKind.Entrypoint);

        Assert.Equal(leaf.Id, entrypoint.Id);
        var topology = new RouteTopologyBuilder().Build(
            [entrypoint, root, leaf],
            [root.CanonicalPath]);

        Assert.Same(leaf, topology.FindByPath(leaf.CanonicalPath)!.Source);
        Assert.Same(entrypoint, topology.FindByPath(entrypoint.CanonicalPath)!.Source);
        Assert.Equal(3, topology.Nodes.Count);
    }

    [Fact(DisplayName = "Route topology builder derives relationships only from supplied source paths and Loader roots")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void SuppliedFactsAreTheOnlyTopologyInput()
    {
        var root = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var child = RouteSourceTestData.Source(".agents/root/child.md", RouteSourceKind.Markdown);

        var topology = new RouteTopologyBuilder().Build([root, child], [root.CanonicalPath]);

        Assert.Equal([child.CanonicalPath], topology.FindByPath(root.CanonicalPath)!.ChildPaths);
    }
}
