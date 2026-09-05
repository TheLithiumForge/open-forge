using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationFormationBuilderTests
{
    [Fact(DisplayName = "Generated navigation formation rejects a cancelled catalogue")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void CancelledCatalogueIsRejected()
    {
        var catalogue = GeneratedNavigationTestData.Catalogue([], isCancelled: true);

        var exception = Assert.Throws<ArgumentException>(() => new GeneratedNavigationFormationBuilder().Build(catalogue));

        Assert.Equal("catalogue", exception.ParamName);
    }

    [Fact(DisplayName = "Generated navigation formation retains the exact catalogue authority and present Loader")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void CompleteCatalogueAuthorityAndLoaderAreRetainedExactly()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        SourceCatalogueIssue[] issues =
        [
            Issue(SourceCatalogueIssueStage.Root, SourceCatalogueIssueCode.RootMissing, ".agents/absent", null),
            Issue(
                SourceCatalogueIssueStage.Directory,
                SourceCatalogueIssueCode.DirectoryUnavailable,
                ".agents/directory",
                Physical("directory")),
            Issue(
                SourceCatalogueIssueStage.Candidate,
                SourceCatalogueIssueCode.CandidateUnavailable,
                ".agents/candidate.md",
                Physical("candidate")),
            Issue(
                SourceCatalogueIssueStage.Identity,
                SourceCatalogueIssueCode.IdentityUnavailable,
                ".agents/identity.md",
                Physical("identity")),
            Issue(
                SourceCatalogueIssueStage.Pairing,
                SourceCatalogueIssueCode.OrphanOverwrite,
                ".agents/orphan.overwrite.md",
                Physical("pairing")),
        ];
        var catalogue = GeneratedNavigationTestData.Catalogue([root, loader], issues: issues);

        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);

        Assert.Same(catalogue, formation.Catalogue);
        Assert.Same(catalogue.Sources, formation.Sources);
        Assert.Same(catalogue.Issues, formation.Issues);
        Assert.Equal(
            [
                SourceCatalogueIssueStage.Root,
                SourceCatalogueIssueStage.Directory,
                SourceCatalogueIssueStage.Candidate,
                SourceCatalogueIssueStage.Identity,
                SourceCatalogueIssueStage.Pairing,
            ],
            formation.Issues.Select(value => value.Stage));
        Assert.Same(loader, formation.Loader);
        Assert.Same(root, formation.FindSource(root.Identity.CanonicalBasePath));
        Assert.Equal([root.Identity.CanonicalBasePath], formation.Topology.LoaderRootPaths);
        Assert.Same(root.Identity, formation.Topology.FindByPath(root.Identity.CanonicalBasePath)?.Identity);
    }

    [Fact(DisplayName = "Generated navigation formation retains root ambiguity without admitting roots when the Loader is missing")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void MissingLoaderRetainsRootAmbiguityWithoutAdmittingRoots()
    {
        var canonical = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var compatibility = Source(".agents/root/index.md", SourceDocumentForm.IndexEntrypoint);

        var formation = Build([compatibility, canonical]);

        Assert.Null(formation.Loader);
        Assert.Empty(formation.Topology.LoaderRootPaths);
        var ambiguity = Assert.Single(
            formation.Ambiguities,
            value => value.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint);
        Assert.Equal(".agents/root", ambiguity.Subject);
        Assert.Equal(
            [canonical.Identity.CanonicalBasePath, compatibility.Identity.CanonicalBasePath],
            ambiguity.Candidates.Select(candidate => candidate.CanonicalPath));
    }

    [Fact(DisplayName = "Generated navigation formation rejects multiple recognized root entrypoint forms before root admission")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void MultipleRootFormsAreAmbiguousAndNotAdmitted()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var canonical = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var compatibility = Source(".agents/root/index.md", SourceDocumentForm.IndexEntrypoint);

        var formation = Build([compatibility, loader, canonical]);

        var ambiguity = Assert.Single(
            formation.Ambiguities,
            value => value.Kind == GeneratedNavigationFormationAmbiguityKind.RootEntrypoint);
        Assert.Equal(".agents/root", ambiguity.Subject);
        Assert.Equal(
            [canonical.Identity.CanonicalBasePath, compatibility.Identity.CanonicalBasePath],
            ambiguity.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Empty(formation.Topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Generated navigation formation keeps a missing-intermediate entrypoint detached")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void MissingIntermediateEntrypointStaysDetached()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var detached = Source(".agents/root/deep/leaf/_leaf.md", SourceDocumentForm.CanonicalEntrypoint);

        var formation = Build([detached, loader, root]);

        Assert.Equal([root.Identity.CanonicalBasePath], formation.Topology.LoaderRootPaths);
        var detachedNode = Assert.IsType<SourceRouteNode>(formation.Topology.FindByPath(detached.Identity.CanonicalBasePath));
        Assert.Equal(SourceRouteParentState.None, detachedNode.ParentState);
        Assert.Null(formation.Topology.ReadAbsoluteDepth(detached.Identity.CanonicalBasePath));
    }

    [Fact(DisplayName = "Generated navigation formation types ambiguous current route parents")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void AmbiguousRouteParentsAreRetainedAsTypedFacts()
    {
        var first = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var second = Source(".agents/root/index.md", SourceDocumentForm.IndexEntrypoint);
        var child = Source(".agents/root/child.md", SourceDocumentForm.Markdown);

        var formation = Build([child, second, first]);

        var ambiguity = Assert.Single(
            formation.Ambiguities,
            value => value.Kind == GeneratedNavigationFormationAmbiguityKind.RouteParent);
        Assert.Equal(child.Identity.CanonicalBasePath, ambiguity.Subject);
        Assert.Equal(
            [first.Identity.CanonicalBasePath, second.Identity.CanonicalBasePath],
            ambiguity.Candidates.Select(candidate => candidate.CanonicalPath));
    }

    [Fact(DisplayName = "Generated navigation formation recognizes compatible same-form aliases under one physical parent")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void SameParentSameFormLeafAliasesAreCompatible()
    {
        var parent = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var shared = Physical("shared/child.md");
        var first = Source(".agents/root/a.md", SourceDocumentForm.Markdown, shared);
        var second = Source(".agents/root/z.md", SourceDocumentForm.Markdown, shared);

        var formation = Build([second, parent, first]);

        var alias = Assert.Single(formation.PhysicalAliasGroups);
        Assert.Equal(GeneratedNavigationPhysicalAliasCompatibility.Compatible, alias.Compatibility);
        Assert.Equal(
            [first.Identity.CanonicalBasePath, second.Identity.CanonicalBasePath],
            alias.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.DoesNotContain(formation.Ambiguities, value =>
            value.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias);
    }

    [Fact(DisplayName = "Generated navigation formation keeps distinct rooted folder aliases incompatible")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void DistinctRootAliasesAreIncompatibleAndNotAdmitted()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var shared = Physical("shared/root.md");
        var first = Source(".agents/alpha/_alpha.md", SourceDocumentForm.CanonicalEntrypoint, shared);
        var second = Source(".agents/zeta/_zeta.md", SourceDocumentForm.CanonicalEntrypoint, shared);

        var formation = Build([second, loader, first]);

        Assert.Equal(
            GeneratedNavigationPhysicalAliasCompatibility.Incompatible,
            Assert.Single(formation.PhysicalAliasGroups).Compatibility);
        Assert.Empty(formation.Topology.LoaderRootPaths);
        Assert.Contains(formation.Ambiguities, value =>
            value.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias);
    }

    [Fact(DisplayName = "Generated navigation formation keeps different recognized forms incompatible across one physical alias")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void DifferentRecognizedFormsAreIncompatible()
    {
        var parent = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var shared = Physical("shared/content.md");
        var markdown = Source(".agents/root/content.md", SourceDocumentForm.Markdown, shared);
        var skill = Source(".agents/root/tool/SKILL.md", SourceDocumentForm.Skill, shared);

        var formation = Build([skill, parent, markdown]);

        Assert.Equal(
            GeneratedNavigationPhysicalAliasCompatibility.Incompatible,
            Assert.Single(formation.PhysicalAliasGroups).Compatibility);
    }

    [Theory(DisplayName = "Generated navigation formation keeps aliases with different route parents or children incompatible")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void DifferingRoutePlacementIsIncompatible(bool differChildren)
    {
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var otherParent = Source(".agents/other/_other.md", SourceDocumentForm.CanonicalEntrypoint);
        var shared = Physical("shared/alias.md");
        var firstPath = differChildren ? ".agents/root/a/_a.md" : ".agents/root/a.md";
        var secondPath = differChildren ? ".agents/root/z/_z.md" : ".agents/other/z.md";
        var form = differChildren ? SourceDocumentForm.CanonicalEntrypoint : SourceDocumentForm.Markdown;
        var first = Source(firstPath, form, shared);
        var second = Source(secondPath, form, shared);
        var sources = new List<SourceLogicalSource> { root, otherParent, first, second };
        if (differChildren)
        {
            sources.Add(Source(".agents/root/a/child.md", SourceDocumentForm.Markdown));
        }

        var formation = Build(sources);

        Assert.Equal(
            GeneratedNavigationPhysicalAliasCompatibility.Incompatible,
            Assert.Single(formation.PhysicalAliasGroups).Compatibility);
    }

    [Fact(DisplayName = "Generated navigation formation detects base-overwrite and orphan alias participation conflicts")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void BaseOverwriteAndOrphanParticipationAreIncompatible()
    {
        var sharedLayer = Physical("shared/layer.md");
        var baseSource = Source(".agents/root/base.md", SourceDocumentForm.Markdown, sharedLayer);
        var paired = Source(
            ".agents/root/paired.md",
            SourceDocumentForm.Markdown,
            physicalPath: Physical("shared/paired.md"),
            overwritePhysicalPath: sharedLayer);
        var sharedOrphan = Physical("shared/orphan.md");
        var pairedForOrphan = Source(
            ".agents/root/paired-orphan.md",
            SourceDocumentForm.Markdown,
            physicalPath: Physical("shared/paired-orphan.md"),
            overwritePhysicalPath: sharedOrphan);
        var orphan = GeneratedNavigationTestData.Candidate(
            canonicalPath: ".agents/root/orphan.overwrite.md",
            form: SourceDocumentForm.OverwriteCompanion,
            physicalPath: sharedOrphan);
        var catalogue = GeneratedNavigationTestData.Catalogue(
            [baseSource, paired, pairedForOrphan],
            additionalCandidates: [orphan]);

        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);

        Assert.Equal(2, formation.PhysicalAliasGroups.Count);
        Assert.All(
            formation.PhysicalAliasGroups,
            alias => Assert.Equal(GeneratedNavigationPhysicalAliasCompatibility.Incompatible, alias.Compatibility));
        Assert.Contains(formation.PhysicalAliasGroups, alias =>
            alias.Candidates.Select(candidate => candidate.CanonicalPath).SequenceEqual(
                [".agents/root/base.md", ".agents/root/paired.overwrite.md"]));
        Assert.Contains(formation.PhysicalAliasGroups, alias =>
            alias.Candidates.Select(candidate => candidate.CanonicalPath).SequenceEqual(
                [".agents/root/orphan.overwrite.md", ".agents/root/paired-orphan.overwrite.md"]));
    }

    [Fact(DisplayName = "Generated navigation formation orders alias groups canonically and retains exact candidate references")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void AliasGroupsAreDeterministicExactCatalogueFacts()
    {
        var firstPhysical = Physical("shared/first.md");
        var secondPhysical = Physical("shared/second.md");
        var zeta = Source(".agents/root/zeta.md", SourceDocumentForm.Markdown, firstPhysical);
        var alpha = Source(".agents/root/alpha.md", SourceDocumentForm.Markdown, firstPhysical);
        var two = Source(".agents/root/two.md", SourceDocumentForm.Markdown, secondPhysical);
        var one = Source(".agents/root/one.md", SourceDocumentForm.Markdown, secondPhysical);
        var catalogue = GeneratedNavigationTestData.Catalogue([zeta, two, alpha, one]);

        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var repeated = new GeneratedNavigationFormationBuilder().Build(catalogue);

        Assert.Equal(
            [".agents/root/alpha.md", ".agents/root/one.md"],
            formation.PhysicalAliasGroups.Select(group => group.Candidates[0].CanonicalPath));
        Assert.Equal(
            formation.PhysicalAliasGroups.Select(group => group.Candidates.Select(candidate => candidate.CanonicalPath)),
            repeated.PhysicalAliasGroups.Select(group => group.Candidates.Select(candidate => candidate.CanonicalPath)));
        Assert.All(
            formation.PhysicalAliasGroups.SelectMany(group => group.Candidates),
            candidate => Assert.Same(catalogue.FindCandidateByPath(candidate.CanonicalPath), candidate));
    }

    [Fact(DisplayName = "Generated navigation projection ingress accepts only exact formation source references")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void ProjectionCannotAssembleASecondSourceView()
    {
        var retained = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var formation = Build([retained]);
        var clone = new SourceLogicalSource(retained.Identity, retained.Base);
        var request = new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(retained, "unavailable")],
            metadata: []);

        var exception = Assert.Throws<ArgumentException>(() => new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(clone, "unavailable")],
            metadata: []));

        Assert.Equal("regions", exception.ParamName);
        Assert.Same(formation, request.Formation);
        Assert.Same(formation.Topology, request.Topology);
        Assert.Same(formation.Sources, request.Sources);
    }

    private static GeneratedNavigationFormation Build(IEnumerable<SourceLogicalSource> sources)
    {
        return new GeneratedNavigationFormationBuilder().Build(GeneratedNavigationTestData.Catalogue(sources));
    }

    private static SourceCatalogueIssue Issue(
        SourceCatalogueIssueStage stage,
        SourceCatalogueIssueCode code,
        string attemptedCanonicalPath,
        string? scopePhysicalPath)
    {
        var issue = new SourceCatalogueIssue(
            code: code,
            attemptedCanonicalPath: attemptedCanonicalPath,
            relatedPaths: [],
            scopePhysicalPath: scopePhysicalPath,
            failure: null);
        Assert.Equal(stage, issue.Stage);
        return issue;
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form,
        string? physicalPath = null,
        string? overwritePhysicalPath = null)
    {
        var resolvedPhysicalPath = physicalPath ?? Physical(canonicalPath);
        var baseLayer = new SourceLayer(
            canonicalPath: canonicalPath,
            physicalPath: resolvedPhysicalPath,
            form: form,
            kind: SourceLayerKind.Base);
        SourceLayer? overwrite = null;
        if (overwritePhysicalPath is not null)
        {
            overwrite = new SourceLayer(
                canonicalPath: canonicalPath[..^".md".Length] + ".overwrite.md",
                physicalPath: overwritePhysicalPath,
                form: SourceDocumentForm.OverwriteCompanion,
                kind: SourceLayerKind.Overwrite);
        }

        return new SourceLogicalSource(
            identity: new SourceLogicalIdentity(
                SourceIdentity.DeriveId(canonicalPath)
                    ?? throw new ArgumentException("A recognized generated navigation test source requires an automatic ID.", nameof(canonicalPath)),
                canonicalPath),
            @base: baseLayer,
            overwrite: overwrite);
    }

    private static string Physical(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "generated-navigation-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
