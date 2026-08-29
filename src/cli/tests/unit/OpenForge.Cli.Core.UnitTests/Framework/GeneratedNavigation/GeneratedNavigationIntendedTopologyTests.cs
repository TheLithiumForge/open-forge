using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationIntendedTopologyTests
{
    [Fact(DisplayName = "Intended source move derives its replacement parent and retains the exact replacement reference")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedMoveReplacesPathAndTopology()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var firstRoot = Source(".agents/first/_first.md", SourceDocumentForm.CanonicalEntrypoint);
        var secondRoot = Source(".agents/second/_second.md", SourceDocumentForm.CanonicalEntrypoint);
        var current = Source(".agents/first/item.md", SourceDocumentForm.Markdown);
        var moved = Source(".agents/second/item.md", SourceDocumentForm.Markdown);
        var catalogue = GeneratedNavigationTestData.Catalogue([loader, firstRoot, secondRoot, current]);

        var formation = Build(catalogue, [loader, firstRoot, secondRoot, moved]);

        Assert.Null(formation.FindSource(current.Identity.CanonicalBasePath));
        Assert.Same(moved, formation.FindSource(moved.Identity.CanonicalBasePath));
        Assert.Empty(formation.Topology.FindByPath(firstRoot.Identity.CanonicalBasePath)?.ChildPaths ?? []);
        Assert.Equal(
            [moved.Identity.CanonicalBasePath],
            formation.Topology.FindByPath(secondRoot.Identity.CanonicalBasePath)?.ChildPaths);
        var movedNode = Assert.IsType<SourceRouteNode>(
            formation.Topology.FindByPath(moved.Identity.CanonicalBasePath));
        Assert.Equal([secondRoot.Identity.CanonicalBasePath], movedNode.ParentPaths);
        Assert.Same(moved.Identity, movedNode.Identity);
    }

    [Fact(DisplayName = "Intended Loader addition and removal change only Loader-root admission")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedLoaderControlsRootAdmission()
    {
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var withoutLoader = GeneratedNavigationTestData.Catalogue([root]);
        var added = Build(withoutLoader, [loader, root]);
        var withLoader = GeneratedNavigationTestData.Catalogue([loader, root]);
        var removed = Build(withLoader, [root]);

        Assert.Same(loader, added.Loader);
        Assert.Equal([root.Identity.CanonicalBasePath], added.Topology.LoaderRootPaths);
        Assert.Null(removed.Loader);
        Assert.Empty(removed.Topology.LoaderRootPaths);
        Assert.Same(root, removed.FindSource(root.Identity.CanonicalBasePath));
    }

    [Fact(DisplayName = "Intended topology does not invent a missing intermediate entrypoint")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedMissingIntermediateStaysDetached()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var detached = Source(
            ".agents/root/missing/leaf/_leaf.md",
            SourceDocumentForm.CanonicalEntrypoint);
        var catalogue = GeneratedNavigationTestData.Catalogue([loader, root]);

        var formation = Build(catalogue, [loader, root, detached]);

        var detachedNode = Assert.IsType<SourceRouteNode>(
            formation.Topology.FindByPath(detached.Identity.CanonicalBasePath));
        Assert.Equal(SourceRouteParentState.None, detachedNode.ParentState);
        Assert.Null(formation.Topology.ReadAbsoluteDepth(detached.Identity.CanonicalBasePath));
    }

    [Fact(DisplayName = "Intended unobserved entrypoints type root and route-parent ambiguities without fabricated candidates")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedAmbiguitiesRetainExactSourcesWithoutObservedCandidates()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var canonical = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var compatibility = Source(".agents/root/index.md", SourceDocumentForm.IndexEntrypoint);
        var child = Source(".agents/root/child.md", SourceDocumentForm.Markdown);
        var catalogue = GeneratedNavigationTestData.Catalogue([]);

        var formation = Build(catalogue, [loader, compatibility, canonical, child]);

        var root = Assert.Single(
            formation.Ambiguities,
            ambiguity => ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint);
        Assert.Equal([canonical, compatibility], root.IntendedSources);
        Assert.Empty(root.Candidates);
        var parent = Assert.Single(
            formation.Ambiguities,
            ambiguity => ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.RouteParent);
        Assert.Equal(child.Identity.CanonicalBasePath, parent.Subject);
        Assert.Equal([canonical, compatibility], parent.IntendedSources);
        Assert.Empty(parent.Candidates);
        Assert.Equal(
            SourceRouteParentState.Ambiguous,
            formation.Topology.FindByPath(child.Identity.CanonicalBasePath)?.ParentState);
        Assert.Empty(formation.Topology.LoaderRootPaths);
    }

    private static GeneratedNavigationFormation Build(
        SourceCatalogue catalogue,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        return new GeneratedNavigationFormationBuilder().Build(catalogue, intendedSources);
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form)
    {
        return GeneratedNavigationTestData.Source(canonicalPath, form);
    }
}
