using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Routing;

public sealed class SourceRouteFactsResolverIntegrationTests
{
    [Fact(DisplayName = "Neutral route facts resolve Loader roots and admitted entrypoint Markdown and Skill topology from real selected sources")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task FullRouteFactsUseOneSelectedRealWorkspace()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/child.md", "child");
        workspace.Write(".agents/root/native/SKILL.md", "skill");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = workspace.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var child = workspace.Source(".agents/root/child.md", "root/child");
        var skill = workspace.Source(
            ".agents/root/native/SKILL.md",
            "root/native",
            SourceDocumentForm.Skill);
        var before = workspace.SnapshotHashes();
        var boundary = Boundary(workspace, [loader, root, child, skill]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.True(facts.AreLoaderRootFactsComplete);
        Assert.Equal(
            ["root", "root/child", "root/native"],
            facts.RouteFacts.Select(fact => fact.Identity.AutomaticId));
        Assert.All(facts.RouteFacts, fact => Assert.Equal(SourceRouteState.Routed, fact.State));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral route facts treat a missing Loader as complete empty-root evidence")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task MissingLoaderHasCompleteEmptyRootFacts()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.Write(".agents/detached.md", "detached");
        var detached = workspace.Source(".agents/detached.md", "detached");
        var boundary = Boundary(workspace, [detached]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.True(facts.AreLoaderRootFactsComplete);
        Assert.Empty(facts.Topology.LoaderRootPaths);
        Assert.Empty(facts.Issues);
        Assert.Equal(SourceRouteState.Unrouted, Assert.Single(facts.RouteFacts).State);
    }

    [Fact(DisplayName = "Neutral route facts retain unavailable identities when the selected Loader body is unreadable")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task UnreadableLoaderMakesUnprovedRoutesUnavailable()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.Write(".agents/loader.md", [0xC3, 0x28]);
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/other/_other.md", "other");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = workspace.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var other = workspace.Source(
            ".agents/other/_other.md",
            "other",
            SourceDocumentForm.CanonicalEntrypoint);
        var boundary = Boundary(workspace, [loader, root, other]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.False(facts.AreLoaderRootFactsComplete);
        Assert.Contains(facts.Issues, issue => issue.Code == SourceRouteIssueCode.LoaderUnreadable);
        Assert.Empty(facts.Topology.LoaderRootPaths);
        Assert.Contains(
            facts.RouteFacts,
            fact => fact.Identity.AutomaticId == "root"
                && fact.State == SourceRouteState.Unavailable);
        Assert.Contains(
            facts.RouteFacts,
            fact => fact.Identity.AutomaticId == "other"
                && fact.State == SourceRouteState.Unavailable);
    }

    [Fact(DisplayName = "Neutral route facts retain earlier valid Loader destinations when a later declaration is malformed")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task MalformedLoaderRetainsEarlierSafeRoots()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [Root](root/_root.md) - #Root\n"
            + "- [Broken](broken%2/_broken.md) - #Broken");
        workspace.Write(".agents/root/_root.md", "root");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = workspace.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var boundary = Boundary(workspace, [loader, root]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.False(facts.AreLoaderRootFactsComplete);
        Assert.Contains(facts.Issues, issue => issue.Code == SourceRouteIssueCode.LoaderMalformed);
        Assert.Contains(facts.Topology.LoaderRootPaths, path =>
            path == ".agents/root/_root.md");
    }

    [Fact(DisplayName = "Neutral route facts detect duplicate canonical Loader roots after destination canonicalization")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task DuplicateLoaderRootsAreMalformedAfterCanonicalResolution()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [First](root/_root.md) - #Root\n"
            + "- [Duplicate](root%2F_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = workspace.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var boundary = Boundary(workspace, [loader, root]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.Contains(facts.Issues, issue => issue.Code == SourceRouteIssueCode.LoaderDuplicateRoot);
        Assert.Single(facts.Topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Neutral route facts classify an unsafe Loader destination without widening physical containment")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task UnsafeLoaderDestinationIsTyped()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Unsafe](root%2F%2F_root.md) - #Root");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var boundary = Boundary(workspace, [loader]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.False(facts.AreLoaderRootFactsComplete);
        Assert.Contains(facts.Issues, issue => issue.Code == SourceRouteIssueCode.LoaderUnsafe);
        Assert.Empty(facts.Topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Neutral route facts retain ambiguous authored parentage as an Ambiguous route state")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task AmbiguousEntrypointsProduceAmbiguousFacts()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/ambiguous/_ambiguous.md", "canonical");
        workspace.Write(".agents/root/ambiguous/index.md", "compatibility");
        workspace.Write(".agents/root/ambiguous/leaf.md", "leaf");
        SourceLogicalSource[] sources =
        [
            workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader),
            workspace.Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint),
            workspace.Source(
                ".agents/root/ambiguous/_ambiguous.md",
                "root/ambiguous",
                SourceDocumentForm.CanonicalEntrypoint),
            workspace.Source(
                ".agents/root/ambiguous/index.md",
                "root/ambiguous",
                SourceDocumentForm.IndexEntrypoint),
            workspace.Source(".agents/root/ambiguous/leaf.md", "root/ambiguous/leaf"),
        ];
        var boundary = Boundary(workspace, sources);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.Contains(
            facts.RouteFacts,
            fact => fact.Identity.AutomaticId == "root/ambiguous/leaf"
                && fact.State == SourceRouteState.Ambiguous);
        Assert.Contains(facts.Issues, issue => issue.Code == SourceRouteIssueCode.RouteAmbiguous);
    }

    [Fact(DisplayName = "Neutral route facts report an excluded Loader as LoaderUnavailable at its exact path")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task FilteredAllowlistReportsExcludedLoaderUnavailable()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/leaf.md", "leaf");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = workspace.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var leaf = workspace.Source(".agents/root/leaf.md", "root/leaf");
        SourceLogicalSource[] all = [loader, root, leaf];
        var boundary = Boundary(workspace, all, selected: [root, leaf]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        var issue = Assert.Single(facts.Issues);
        Assert.Equal(SourceRouteIssueCode.LoaderUnavailable, issue.Code);
        Assert.Equal(".agents/loader.md", issue.CanonicalPath);
        Assert.Empty(issue.RelatedPaths);
        Assert.All(facts.RouteFacts, fact => Assert.Equal(SourceRouteState.Unavailable, fact.State));
    }

    [Fact(DisplayName = "Neutral route facts report an excluded Loader destination as RouteSupportUnavailable without widening the allowlist")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task FilteredAllowlistReportsExcludedRouteSupportUnavailable()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/leaf.md", "leaf");
        var loader = workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader);
        var root = workspace.Source(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint);
        var leaf = workspace.Source(".agents/root/leaf.md", "root/leaf");
        SourceLogicalSource[] all = [loader, root, leaf];
        var boundary = Boundary(workspace, all, selected: [loader, leaf]);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.NotEmpty(facts.Issues);
        Assert.All(facts.Issues, issue =>
        {
            Assert.Equal(SourceRouteIssueCode.RouteSupportUnavailable, issue.Code);
            Assert.Equal(".agents/root/_root.md", issue.CanonicalPath);
        });
        Assert.All(facts.RouteFacts, fact => Assert.Equal(SourceRouteState.Unavailable, fact.State));
    }

    [Fact(DisplayName = "Neutral route facts retain identity uniqueness separately from structural routed state")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task IdentityCollisionDoesNotEraseStructuralRouteFact()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/collision.md", "leaf");
        workspace.Write(".agents/root/collision/_collision.md", "entrypoint");
        SourceLogicalSource[] sources =
        [
            workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader),
            workspace.Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint),
            workspace.Source(".agents/root/collision.md", "root/collision"),
            workspace.Source(
                ".agents/root/collision/_collision.md",
                "root/collision",
                SourceDocumentForm.CanonicalEntrypoint),
        ];
        var boundary = Boundary(workspace, sources);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        var collisionFacts = facts.RouteFacts
            .Where(fact => fact.Identity.AutomaticId == "root/collision")
            .ToArray();
        Assert.Equal(2, collisionFacts.Length);
        Assert.All(collisionFacts, fact =>
        {
            Assert.Equal(SourceRouteState.Routed, fact.State);
            Assert.False(fact.IsIdentityUnique);
            Assert.Null(fact.Route);
        });
    }

    [Fact(DisplayName = "Neutral route facts sort a deterministic sequence of Loader and topology issues")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task RouteFactsIssueOrderingIsExplicitForMultipleIssues()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader(
            "- [Root](root/_root.md) - #Root\n"
            + "- [Broken](broken%2/_broken.md) - #Broken");
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/alpha/_alpha.md", "alpha");
        workspace.Write(".agents/root/alpha/index.md", "alpha compatibility");
        workspace.Write(".agents/root/alpha/leaf.md", "alpha leaf");
        workspace.Write(".agents/root/z/_z.md", "z");
        workspace.Write(".agents/root/z/index.md", "z compatibility");
        workspace.Write(".agents/root/z/leaf.md", "z leaf");
        SourceLogicalSource[] sources =
        [
            workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader),
            workspace.Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint),
            workspace.Source(
                ".agents/root/alpha/_alpha.md",
                "root/alpha",
                SourceDocumentForm.CanonicalEntrypoint),
            workspace.Source(
                ".agents/root/alpha/index.md",
                "root/alpha",
                SourceDocumentForm.IndexEntrypoint),
            workspace.Source(".agents/root/alpha/leaf.md", "root/alpha/leaf"),
            workspace.Source(
                ".agents/root/z/_z.md",
                "root/z",
                SourceDocumentForm.CanonicalEntrypoint),
            workspace.Source(
                ".agents/root/z/index.md",
                "root/z",
                SourceDocumentForm.IndexEntrypoint),
            workspace.Source(".agents/root/z/leaf.md", "root/z/leaf"),
        ];
        var boundary = Boundary(workspace, sources);

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "LoaderMalformed|.agents/loader.md",
                "RouteAmbiguous|.agents/root/alpha/leaf.md",
                "RouteAmbiguous|.agents/root/z/leaf.md",
            ],
            facts.Issues.Select(issue => $"{issue.Code}|{issue.CanonicalPath}"));
    }

    [Fact(DisplayName = "Neutral route facts retain a pre-cancelled outcome without reading or writing the workspace")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task RouteFactsPreCancellationIsTerminal()
    {
        using var workspace = SourceRouteFactsIntegrationWorkspace.Create();
        workspace.WriteLoader("- [Root](root/_root.md) - #Root");
        workspace.Write(".agents/root/_root.md", "root");
        SourceLogicalSource[] sources =
        [
            workspace.Source(".agents/loader.md", "loader", SourceDocumentForm.Loader),
            workspace.Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint),
        ];
        var boundary = Boundary(workspace, sources);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var facts = await new SourceRouteFactsResolver().ResolveAsync(
            boundary.Request,
            boundary.Reader,
            cancellation.Token);

        Assert.True(facts.IsCancelled);
        Assert.Empty(facts.Issues);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static ResolverBoundary Boundary(
        SourceRouteFactsIntegrationWorkspace workspace,
        IReadOnlyList<SourceLogicalSource> sources,
        IReadOnlyList<SourceLogicalSource>? selected = null)
    {
        var candidates = sources.Select(source => new SourceCandidate(
            source.Base.CanonicalPath,
            source.Base.Form,
            source.Identity.AutomaticId,
            PhysicalPathState.Contained,
            source.Base.PhysicalPath,
            Path.GetDirectoryName(source.Base.PhysicalPath)
                ?? throw new InvalidOperationException("The integration source must have a physical parent."))).ToArray();
        var catalogue = new SourceCatalogue(
            workspace.Workspace,
            candidates,
            sources,
            [],
            isCancelled: false);
        var selectedSources = selected ?? sources;
        var selectedCandidates = candidates
            .Where(candidate => selectedSources.Any(source =>
                source.Identity.CanonicalBasePath == candidate.CanonicalPath))
            .ToArray();
        var selection = new SourceCatalogueSelection(
            selectedSources,
            selectedCandidates,
            [],
            []);
        return new ResolverBoundary(
            new SourceRouteFactsRequest(catalogue, selection),
            new SourceDocumentReader(workspace.Workspace));
    }

    private sealed record ResolverBoundary(
        SourceRouteFactsRequest Request,
        SourceDocumentReader Reader);
}
