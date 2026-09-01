using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

public sealed class SourceRouteTopologyBuilderTests
{
    [Fact(DisplayName = "Neutral route topology admits entrypoints Markdown and Skill sources by authored directory topology")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void AuthoredRelationshipsAreParentFirst()
    {
        var root = Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child/_child.md", "root/child", SourceDocumentForm.CanonicalEntrypoint);
        var grandchild = Source(".agents/root/child/grand.md", "root/child/grand");
        var leaf = Source(".agents/root/leaf.md", "root/leaf");
        var skill = Source(".agents/root/native/SKILL.md", "root/native", SourceDocumentForm.Skill);

        var topology = new SourceRouteTopologyBuilder().Build(
            [grandchild, skill, leaf, child, root],
            [root.Identity.CanonicalBasePath]);

        var rootNode = Assert.IsType<SourceRouteNode>(topology.FindByPath(root.Identity.CanonicalBasePath));
        var childNode = Assert.IsType<SourceRouteNode>(topology.FindByPath(child.Identity.CanonicalBasePath));
        Assert.Equal(
            [child.Identity.CanonicalBasePath, leaf.Identity.CanonicalBasePath, skill.Identity.CanonicalBasePath],
            rootNode.ChildPaths);
        Assert.Equal(
            [grandchild.Identity.CanonicalBasePath],
            childNode.ChildPaths);
        Assert.Equal(
            [root.Identity.CanonicalBasePath],
            childNode.ParentPaths);
        Assert.Equal([root.Identity.CanonicalBasePath], topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Neutral route topology keeps detached entrypoints separate from Loader roots")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void DetachedRootsHaveNoFabricatedLoaderParent()
    {
        var loaderRoot = Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint);
        var detached = Source(
            ".agents/detached/_detached.md",
            "detached",
            SourceDocumentForm.CanonicalEntrypoint);
        var detachedLeaf = Source(".agents/detached/leaf.md", "detached/leaf");

        var topology = new SourceRouteTopologyBuilder().Build(
            [detachedLeaf, detached, loaderRoot],
            [loaderRoot.Identity.CanonicalBasePath]);

        var detachedNode = Assert.IsType<SourceRouteNode>(topology.FindByPath(detached.Identity.CanonicalBasePath));
        Assert.Equal(SourceRouteParentState.None, detachedNode.ParentState);
        Assert.Empty(detachedNode.ParentPaths);
        Assert.Equal(
            [detachedLeaf.Identity.CanonicalBasePath],
            detachedNode.ChildPaths);
    }

    [Fact(DisplayName = "Neutral route topology retains ambiguous compatibility parent candidates")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void AmbiguousParentageIsNotResolvedByOrder()
    {
        var root = Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint);
        var canonical = Source(
            ".agents/root/ambiguous/_ambiguous.md",
            "root/ambiguous",
            SourceDocumentForm.CanonicalEntrypoint);
        var compatibility = Source(
            ".agents/root/ambiguous/index.md",
            "root/ambiguous",
            SourceDocumentForm.IndexEntrypoint);
        var leaf = Source(".agents/root/ambiguous/leaf.md", "root/ambiguous/leaf");

        var topology = new SourceRouteTopologyBuilder().Build(
            [leaf, compatibility, root, canonical],
            [root.Identity.CanonicalBasePath]);

        var leafNode = Assert.IsType<SourceRouteNode>(topology.FindByPath(leaf.Identity.CanonicalBasePath));
        Assert.Equal(SourceRouteParentState.Ambiguous, leafNode.ParentState);
        Assert.Equal(
            [canonical.Identity.CanonicalBasePath, compatibility.Identity.CanonicalBasePath],
            leafNode.ParentPaths);
    }

    [Fact(DisplayName = "Neutral route topology retains identity collisions as distinct path nodes")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void IdentityCollisionsRemainDistinctTopologyNodes()
    {
        var root = Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(".agents/root/collision.md", "root/collision");
        var entrypoint = Source(
            ".agents/root/collision/_collision.md",
            "root/collision",
            SourceDocumentForm.CanonicalEntrypoint);

        var topology = new SourceRouteTopologyBuilder().Build(
            [entrypoint, root, leaf],
            [root.Identity.CanonicalBasePath]);

        var leafNode = Assert.IsType<SourceRouteNode>(topology.FindByPath(leaf.Identity.CanonicalBasePath));
        var entrypointNode = Assert.IsType<SourceRouteNode>(topology.FindByPath(entrypoint.Identity.CanonicalBasePath));
        Assert.Same(leaf.Identity, leafNode.Identity);
        Assert.Same(entrypoint.Identity, entrypointNode.Identity);
        Assert.Equal(3, topology.Nodes.Count);
    }

    [Fact(DisplayName = "Neutral route topology uses only selected sources and explicit Loader roots")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void GeneratedEntriesNeverBecomeTopologyInput()
    {
        var root = Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");

        var topology = new SourceRouteTopologyBuilder().Build(
            [root, child],
            [root.Identity.CanonicalBasePath]);

        Assert.Equal(
            [child.Identity.CanonicalBasePath],
            Assert.IsType<SourceRouteNode>(topology.FindByPath(root.Identity.CanonicalBasePath)).ChildPaths);
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        string id,
        SourceDocumentForm form = SourceDocumentForm.Markdown)
    {
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            new SourceLayer(
                canonicalPath,
                Path.GetFullPath(Path.Combine(
                    Path.GetTempPath(),
                    "source-route-topology-unit",
                    canonicalPath.Replace('/', Path.DirectorySeparatorChar))),
                form,
                SourceLayerKind.Base));
    }
}
