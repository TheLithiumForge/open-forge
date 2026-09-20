using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Routing;

public sealed class SourceRouteTopologyLookupTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral route topology finds exact paths and computes absolute and relative depths")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void TopologyLookupsUseExactCanonicalPaths()
    {
        var topology = SourceRoutingTopology();

        Assert.NotNull(topology.FindByPath(".agents/root/_root.md"));
        Assert.Equal(0, topology.ReadAbsoluteDepth(".agents/root/_root.md"));
        Assert.True(topology.TryReadRelativeDepth(
            ".agents/root/_root.md",
            ".agents/root/child.md",
            out var relativeDepth));
        Assert.Equal(1, relativeDepth);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Neutral route topology reports missing and unrelated exact paths without inventing ancestry")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void UnknownTopologyPathsRemainUnavailable()
    {
        var topology = SourceRoutingTopology();

        Assert.Null(topology.FindByPath(".agents/missing.md"));
        Assert.Null(topology.ReadAbsoluteDepth(".agents/missing.md"));
        Assert.False(topology.TryReadRelativeDepth(
            ".agents/root/_root.md",
            ".agents/unrelated.md",
            out _));
    }

    private static SourceRouteTopology SourceRoutingTopology()
    {
        var root = SourceRoutingTestData.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = SourceRoutingTestData.Source(".agents/root/child.md", "root/child");
        var rootNode = new SourceRouteNode(
            root.Identity,
            SourceRouteParentState.None,
            [],
            [child.Identity.CanonicalBasePath]);
        var childNode = new SourceRouteNode(
            child.Identity,
            SourceRouteParentState.Resolved,
            [root.Identity.CanonicalBasePath],
            []);
        return new SourceRouteTopology(
            [rootNode, childNode],
            [root.Identity.CanonicalBasePath]);
    }
}
