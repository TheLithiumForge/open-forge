using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Routing;

public sealed class SourceRoutingModelTests
{
    [Fact(DisplayName = "Neutral route nodes retain reciprocal parent and child facts in ordinal order")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void TopologyModelsSnapshotValidatedRelationships()
    {
        var topology = SourceRoutingTestData.Topology();

        Assert.Equal(
            [".agents/root/_root.md", ".agents/root/child.md"],
            topology.Nodes.Select(node => node.Identity.CanonicalBasePath));
        Assert.Equal([".agents/root/_root.md"], topology.LoaderRootPaths);
        var child = topology.Nodes.Single(node => node.Identity.AutomaticId == "root/child");
        Assert.Equal(SourceRouteParentState.Resolved, child.ParentState);
        Assert.Equal([".agents/root/_root.md"], child.ParentPaths);
        Assert.Empty(child.ChildPaths);
    }

    [Fact(DisplayName = "Neutral route nodes retain all ambiguous authored parent candidates without choosing by order")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void AmbiguousParentsRemainAmbiguous()
    {
        var identity = SourceRoutingTestData.Source(
            ".agents/root/ambiguous.md",
            "root/ambiguous").Identity;
        var node = new SourceRouteNode(
            identity,
            SourceRouteParentState.Ambiguous,
            [".agents/root/z.md", ".agents/root/a.md"],
            []);

        Assert.Equal(
            [".agents/root/a.md", ".agents/root/z.md"],
            node.ParentPaths);
    }

    [Fact(DisplayName = "Neutral topology models reject nonreciprocal relationships, unknown roots, and cycles")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void TopologyInvariantsRemainStrict()
    {
        var root = SourceRoutingTestData.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = SourceRoutingTestData.Source(".agents/root/child.md", "root/child");
        var rootNode = new SourceRouteNode(root.Identity, SourceRouteParentState.None, [], []);
        var childNode = new SourceRouteNode(
            child.Identity,
            SourceRouteParentState.Resolved,
            [root.Identity.CanonicalBasePath],
            []);

        Assert.Throws<ArgumentException>(() => new SourceRouteTopology(
            [rootNode, childNode],
            [child.Identity.CanonicalBasePath]));

        var first = SourceRoutingTestData.Source(
            ".agents/first/_first.md",
            "first",
            SourceDocumentForm.CanonicalEntrypoint);
        var second = SourceRoutingTestData.Source(
            ".agents/second/_second.md",
            "second",
            SourceDocumentForm.CanonicalEntrypoint);
        var firstNode = new SourceRouteNode(
            first.Identity,
            SourceRouteParentState.Resolved,
            [second.Identity.CanonicalBasePath],
            [second.Identity.CanonicalBasePath]);
        var secondNode = new SourceRouteNode(
            second.Identity,
            SourceRouteParentState.Resolved,
            [first.Identity.CanonicalBasePath],
            [first.Identity.CanonicalBasePath]);
        Assert.Throws<ArgumentException>(() => new SourceRouteTopology(
            [firstNode, secondNode],
            [first.Identity.CanonicalBasePath]));
    }

    [Theory(DisplayName = "Neutral route facts derive route metadata only for unique routed identities"),
        InlineData(nameof(SourceRouteState.Routed), true, "root"),
        InlineData(nameof(SourceRouteState.Routed), false, null),
        InlineData(nameof(SourceRouteState.Unrouted), true, null),
        InlineData(nameof(SourceRouteState.Ambiguous), true, null),
        InlineData(nameof(SourceRouteState.Unavailable), true, null)]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void RouteProjectionHonorsStateAndIdentityUniqueness(
        string stateName,
        bool isIdentityUnique,
        string? expectedRoute)
    {
        var fact = new SourceRouteFact(
            SourceRoutingTestData.Source(
                ".agents/root/_root.md",
                "root",
                SourceDocumentForm.CanonicalEntrypoint).Identity,
            Enum.Parse<SourceRouteState>(stateName),
            isIdentityUnique);

        Assert.Equal(expectedRoute, fact.Route);
        Assert.Equal(isIdentityUnique, fact.IsIdentityUnique);
    }

    [Fact(DisplayName = "Neutral route issues retain stable occurrence and bounded direct cause")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void RouteIssuesRetainTypedOrderingFacts()
    {
        var issue = new SourceRouteIssue(
            SourceRouteIssueCode.RouteSupportUnavailable,
            ".agents/root/leaf.md",
            [".agents/root/z.md", ".agents/root/a.md"],
            4,
            new string('x', 300));

        Assert.Equal(SourceRouteIssueCode.RouteSupportUnavailable, issue.Code);
        Assert.Equal(
            [".agents/root/a.md", ".agents/root/z.md"],
            issue.RelatedPaths);
        Assert.Equal(4, issue.Occurrence);
        Assert.Equal(256, issue.Cause.Length);
    }

    [Fact(DisplayName = "Neutral route facts retain ordered facts, issues, topology, completeness, and cancellation")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void RouteFactsAreOneImmutableBoundary()
    {
        var topology = SourceRoutingTestData.Topology();
        var first = new SourceRouteFact(
            SourceRoutingTestData.Source(".agents/root/child.md", "root/child").Identity,
            SourceRouteState.Routed,
            true);
        var second = new SourceRouteFact(
            SourceRoutingTestData.Source(
                ".agents/root/_root.md",
                "root",
                SourceDocumentForm.CanonicalEntrypoint).Identity,
            SourceRouteState.Routed,
            true);
        var facts = new SourceRouteFacts(
            topology,
            [first, second],
            [],
            areLoaderRootFactsComplete: true,
            isCancelled: true);

        Assert.Same(topology, facts.Topology);
        Assert.Equal(["root", "root/child"], facts.RouteFacts.Select(fact => fact.Identity.AutomaticId));
        Assert.True(facts.AreLoaderRootFactsComplete);
        Assert.True(facts.IsCancelled);
    }
}
