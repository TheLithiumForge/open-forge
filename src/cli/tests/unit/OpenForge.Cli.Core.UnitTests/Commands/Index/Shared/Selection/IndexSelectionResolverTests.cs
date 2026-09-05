using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Selection;

public sealed class IndexSelectionResolverTests
{
    private const string RootPath = ".agents/root/_root.md";
    private const string ChildPath = ".agents/root/child/_child.md";
    private const string LeafPath = ".agents/root/leaf.md";

    [Fact(DisplayName = "Index selection closes automatic and explicit rooted entrypoints through Loader"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootedEntrypointClosureIncludesLoaderAndDescendants()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(ChildPath, SourceDocumentForm.CanonicalEntrypoint);
        var formation = Formation(loader, child, root);
        var resolver = Resolver();

        var automatic = resolver.Resolve(Request(formation), formation);
        var explicitRoot = resolver.Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexSelectionOrigin.AutomaticLoader, automatic.Selection.Origin);
        Assert.Equal([SourceLogicalPath.LoaderPath], automatic.Selection.Sources.Select(source => source.Path));
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: loader,
                formation: formation),
            Assert.Single(automatic.Selection.Sources));
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: root,
                formation: formation),
            Assert.Single(explicitRoot.Selection.Sources));
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, RootPath, ChildPath],
            automatic.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, RootPath, ChildPath],
            explicitRoot.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.All([automatic, explicitRoot], resolution =>
        {
            Assert.True(resolution.IsComplete);
            Assert.Empty(resolution.Findings);
        });
    }

    [Fact(DisplayName = "Index selection keeps a complete detached entrypoint local when Loader is missing"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DetachedEntrypointDoesNotInventLoaderOrParent()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(ChildPath, SourceDocumentForm.CanonicalEntrypoint);
        var formation = Formation(child, root);

        var resolution = Resolver().Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexSelectionScope.Detached, resolution.Selection.Scope);
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: root,
                formation: formation),
            Assert.Single(resolution.Selection.Sources));
        Assert.Equal([RootPath, ChildPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.DoesNotContain(
            resolution.Targets,
            source => source.Identity.CanonicalBasePath == SourceLogicalPath.LoaderPath);
        Assert.True(resolution.IsComplete);
    }

    [Fact(DisplayName = "Index leaf and overwrite references normalize to one base selection and direct parent target"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void LeafAndOverwriteReferencesSelectOnlyTheDirectParent()
    {
        var parent = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(LeafPath, SourceDocumentForm.Markdown, withOverwrite: true);
        var formation = Formation(leaf, parent);

        var resolution = Resolver().Resolve(
            Request(formation, LeafPath, ".agents/root/leaf.overwrite.md", LeafPath),
            formation);

        Assert.Equal([LeafPath], resolution.Selection.Sources.Select(source => source.Path));
        Assert.Equal([RootPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.True(resolution.IsComplete);
    }

    [Fact(DisplayName = "Index overlapping selections union target closure once in canonical order"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DuplicateAndOverlappingSelectionsDoNotDuplicateTargets()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(ChildPath, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(LeafPath, SourceDocumentForm.Markdown);
        var formation = Formation(leaf, child, root);

        var resolution = Resolver().Resolve(
            Request(formation, ChildPath, LeafPath, RootPath, RootPath),
            formation);

        Assert.Equal([RootPath, ChildPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            [RootPath, ChildPath, LeafPath],
            resolution.Selection.Sources.Select(source => source.Path));
    }

    [Fact(DisplayName = "Index uses compatible physical aliases once and blocks relevant incompatible aliases"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void PhysicalAliasesUseFormationCompatibilityAndAuthoritativePathIdentity()
    {
        var parent = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var sharedLeaf = Physical("shared/leaf.md");
        var alpha = Source(".agents/root/alpha.md", SourceDocumentForm.Markdown, sharedLeaf);
        var zeta = Source(".agents/root/zeta.md", SourceDocumentForm.Markdown, sharedLeaf);
        var compatible = Formation(zeta, parent, alpha);

        var normalized = Resolver().Resolve(
            Request(compatible, alpha.Identity.CanonicalBasePath, zeta.Identity.CanonicalBasePath),
            compatible);

        Assert.Equal([alpha.Identity.CanonicalBasePath], normalized.Selection.Sources.Select(source => source.Path));
        Assert.Equal([RootPath], normalized.Targets.Select(source => source.Identity.CanonicalBasePath));

        var sharedRoot = Physical("shared/root.md");
        var firstRoot = Source(".agents/alpha/_alpha.md", SourceDocumentForm.CanonicalEntrypoint, sharedRoot);
        var secondRoot = Source(".agents/zeta/_zeta.md", SourceDocumentForm.CanonicalEntrypoint, sharedRoot);
        var incompatible = Formation(secondRoot, firstRoot);

        var blocked = Resolver().Resolve(Request(incompatible, firstRoot.Identity.CanonicalBasePath), incompatible);

        Assert.Empty(blocked.Targets);
        Assert.Equal(IndexFindingCode.SourceUnsafe, Assert.Single(blocked.Findings).Code);
    }

    [Fact(DisplayName = "Index source ID collisions block IDs while exact paths remain disambiguated"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ExactPathsDisambiguateSourceIdCollisions()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var entrypoint = Source(".agents/memory/_memory.md", SourceDocumentForm.CanonicalEntrypoint);
        var collidingLeaf = Source(".agents/memory.md", SourceDocumentForm.Markdown);
        var formation = Formation(collidingLeaf, entrypoint, loader);

        var ambiguous = Resolver().Resolve(Request(formation, "memory"), formation);
        var exact = Resolver().Resolve(Request(formation, entrypoint.Identity.CanonicalBasePath), formation);

        Assert.Empty(ambiguous.Targets);
        Assert.Equal(IndexFindingCode.SourceAmbiguous, Assert.Single(ambiguous.Findings).Code);
        Assert.True(exact.IsComplete);
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, entrypoint.Identity.CanonicalBasePath],
            exact.Targets.Select(source => source.Identity.CanonicalBasePath));
    }

    [Theory(DisplayName = "Index root safety and availability own explicit failure before source resolution"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    [InlineData(SourceCatalogueIssueCode.RootUnsafe, IndexFindingCode.WorkspaceUnsafe)]
    [InlineData(SourceCatalogueIssueCode.RootUnavailable, IndexFindingCode.DiscoveryIncomplete)]
    internal void RootBoundaryFailuresShortCircuitExplicitResolution(
        SourceCatalogueIssueCode issueCode,
        IndexFindingCode expectedCode)
    {
        var formation = Formation([], RootIssue(issueCode));

        var resolution = Resolver().Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexSelectionScope.NotEstablished, resolution.Selection.Scope);
        Assert.Empty(resolution.Targets);
        Assert.Equal(expectedCode, Assert.Single(resolution.Findings).Code);
        Assert.DoesNotContain(resolution.Findings, finding => finding.Code == IndexFindingCode.InvalidSource);
    }

    [Fact(DisplayName = "Index root missing is an automatic Loader failure but adds no explicit workspace finding"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootMissingKeepsAutomaticAndExplicitMeaningsDistinct()
    {
        var formation = Formation([], RootIssue(SourceCatalogueIssueCode.RootMissing));
        var resolver = Resolver();

        var automatic = resolver.Resolve(Request(formation), formation);
        var explicitSource = resolver.Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexFindingCode.TargetUnexposed, Assert.Single(automatic.Findings).Code);
        Assert.Equal(IndexFindingCode.InvalidSource, Assert.Single(explicitSource.Findings).Code);
        Assert.DoesNotContain(
            explicitSource.Findings,
            finding => finding.Code is IndexFindingCode.WorkspaceUnavailable or IndexFindingCode.DiscoveryIncomplete);
    }

    [Fact(DisplayName = "Index selection resolution enforces complete and incomplete target coherence"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResolutionRejectsNullFindingsAndImpossibleExecutableStates()
    {
        var target = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var projected = new IndexLogicalSource(
            target.Identity.AutomaticId,
            target.Identity.CanonicalBasePath,
            IndexLogicalSourceScope.Detached);
        var selection = new IndexSelection(
            IndexSelectionOrigin.ExplicitSources,
            IndexSelectionScope.Detached,
            [projected]);
        var finding = new IndexFinding(
            IndexFindingCode.InvalidSource,
            sourceOccurrence: 1,
            source: null,
            cause: "The source reference is invalid.",
            candidates: []);
        var invalidTarget = Source(LeafPath, SourceDocumentForm.Markdown);
        var physicalAliasTarget = Source(
            ".agents/alias/_alias.md",
            SourceDocumentForm.CanonicalEntrypoint,
            target.Base.PhysicalPath);

        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [target], [finding]));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [], []));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(
            IndexSelection.NotEstablished(IndexSelectionOrigin.ExplicitSources),
            [target],
            []));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [], new IndexFinding[1]));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [invalidTarget], []));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [target, physicalAliasTarget], []));

        var complete = new IndexSelectionResolution(selection, [target], []);
        Assert.True(complete.IsComplete);
        Assert.Same(target, Assert.Single(complete.Targets));
    }

    private static IndexSelectionResolver Resolver()
        => new(new SourceReferenceResolver((_, path) =>
            PhysicalPathResolution.Classified(PhysicalPathState.Missing, path)));

    private static IndexRequest Request(
        GeneratedNavigationFormation formation,
        params string[] sourceReferences)
        => new(formation.Catalogue.Workspace, sourceReferences, IndexMode.Apply);

    private static GeneratedNavigationFormation Formation(params SourceLogicalSource[] sources)
        => Formation(sources, []);

    private static GeneratedNavigationFormation Formation(
        IEnumerable<SourceLogicalSource> sources,
        params SourceCatalogueIssue[] issues)
        => new GeneratedNavigationFormationBuilder().Build(
            GeneratedNavigationTestData.Catalogue(sources, issues: issues));

    private static SourceCatalogueIssue RootIssue(SourceCatalogueIssueCode code)
        => new(
            code,
            SourceLogicalPath.AgentsRoot,
            relatedPaths: [],
            scopePhysicalPath: null,
            failure: null);

    private static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form,
        string? physicalPath = null,
        bool withOverwrite = false)
    {
        var resolvedPhysicalPath = physicalPath ?? Physical(canonicalPath);
        var identity = new SourceLogicalIdentity(
            SourceIdentity.DeriveId(canonicalPath)
                ?? throw new ArgumentException("An Index selection test source requires a derived ID.", nameof(canonicalPath)),
            canonicalPath);
        var baseLayer = new SourceLayer(
            canonicalPath,
            resolvedPhysicalPath,
            form,
            SourceLayerKind.Base);
        if (!withOverwrite)
        {
            return new SourceLogicalSource(identity, baseLayer);
        }

        var overwritePath = canonicalPath[..^".md".Length] + ".overwrite.md";
        var overwriteLayer = new SourceLayer(
            overwritePath,
            Physical(overwritePath),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        return new SourceLogicalSource(identity, baseLayer, overwriteLayer);
    }

    private static string Physical(string relativePath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-index-selection-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
