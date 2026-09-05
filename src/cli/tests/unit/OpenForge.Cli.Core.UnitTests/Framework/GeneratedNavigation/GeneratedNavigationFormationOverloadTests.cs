using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationFormationOverloadTests
{
    [Fact(DisplayName = "Current and explicit-current generated navigation formation are semantically equivalent")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void CurrentFormationDelegatesWithoutLosingObservedEvidence()
    {
        var shared = GeneratedNavigationTestData.Physical("shared/layer.md");
        var source = GeneratedNavigationTestData.Source(
            ".agents/root/source.md",
            SourceDocumentForm.Markdown,
            overwritePhysicalPath: shared);
        var orphan = GeneratedNavigationTestData.Candidate(
            canonicalPath: ".agents/root/orphan.overwrite.md",
            form: SourceDocumentForm.OverwriteCompanion,
            physicalPath: shared);
        var issue = new SourceCatalogueIssue(
            code: SourceCatalogueIssueCode.OrphanOverwrite,
            attemptedCanonicalPath: orphan.CanonicalPath,
            relatedPaths: [],
            scopePhysicalPath: GeneratedNavigationTestData.Physical("root"),
            failure: null);
        var catalogue = GeneratedNavigationTestData.Catalogue(
            [source],
            additionalCandidates: [orphan],
            issues: [issue]);
        var builder = new GeneratedNavigationFormationBuilder();

        var current = builder.Build(catalogue);
        var explicitCurrent = builder.Build(catalogue, catalogue.Sources);

        Assert.Same(catalogue, current.Catalogue);
        Assert.Same(catalogue.Sources, current.Sources);
        Assert.Same(catalogue.Issues, current.Issues);
        var overwrite = Assert.IsType<SourceLayer>(source.Overwrite);
        Assert.Same(source, current.FindSource(overwrite.CanonicalPath));
        Assert.Same(explicitCurrent.Sources, current.Sources);
        Assert.Same(explicitCurrent.Issues, current.Issues);
        Assert.Equal(
            current.Topology.Nodes.Select(NodeShape),
            explicitCurrent.Topology.Nodes.Select(NodeShape));
        Assert.Equal(
            current.PhysicalAliasGroups.Select(AliasShape),
            explicitCurrent.PhysicalAliasGroups.Select(AliasShape));
        Assert.Equal(
            current.Ambiguities.Select(AmbiguityShape),
            explicitCurrent.Ambiguities.Select(AmbiguityShape));
        Assert.Empty(current.IntendedTargetCollisions);
        Assert.Empty(explicitCurrent.IntendedTargetCollisions);
    }

    [Fact(DisplayName = "Intended source addition forms intended topology without inventing observed evidence")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedAdditionUsesTheExactNewSourceReference()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var added = Source(".agents/root/added.md", SourceDocumentForm.Markdown);
        var catalogue = GeneratedNavigationTestData.Catalogue([loader, root]);

        var formation = Build(catalogue, [loader, root, added]);

        Assert.Same(added, formation.FindSource(added.Identity.CanonicalBasePath));
        Assert.Same(added.Identity, formation.Topology.FindByPath(added.Identity.CanonicalBasePath)?.Identity);
        Assert.Equal(
            [added.Identity.CanonicalBasePath],
            formation.Topology.FindByPath(root.Identity.CanonicalBasePath)?.ChildPaths);
        Assert.Null(catalogue.FindCandidateByPath(added.Identity.CanonicalBasePath));
        Assert.Empty(formation.PhysicalAliasGroups);
        Assert.Empty(formation.Issues);
    }

    [Fact(DisplayName = "Intended source addition retains unrelated observed orphan evidence without attaching it to the new source")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedAdditionRetainsUnrelatedObservedEvidence()
    {
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var added = Source(".agents/root/added.md", SourceDocumentForm.Markdown);
        var orphan = GeneratedNavigationTestData.Candidate(
            canonicalPath: ".agents/root/orphan.overwrite.md",
            form: SourceDocumentForm.OverwriteCompanion,
            physicalPath: GeneratedNavigationTestData.Physical("root/orphan.overwrite.md"));
        var issue = new SourceCatalogueIssue(
            code: SourceCatalogueIssueCode.OrphanOverwrite,
            attemptedCanonicalPath: orphan.CanonicalPath,
            relatedPaths: [],
            scopePhysicalPath: GeneratedNavigationTestData.Physical("root"),
            failure: null);
        var catalogue = GeneratedNavigationTestData.Catalogue(
            [root],
            additionalCandidates: [orphan],
            issues: [issue]);

        var formation = Build(catalogue, [root, added]);

        Assert.Same(catalogue.Issues, formation.Issues);
        Assert.Same(issue, Assert.Single(formation.Issues));
        Assert.Same(added, formation.FindSource(added.Identity.CanonicalBasePath));
        Assert.Null(catalogue.FindCandidateByPath(added.Identity.CanonicalBasePath));
    }

    [Fact(DisplayName = "Intended source removal drops membership topology and source-local observed evidence")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedRemovalDoesNotRetainStaleAliasOrIssueFacts()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var shared = GeneratedNavigationTestData.Physical("shared/removed.md");
        var removed = Source(".agents/root/removed.md", SourceDocumentForm.Markdown, shared);
        var removedAlias = Source(".agents/root/removed-alias.md", SourceDocumentForm.Markdown, shared);
        var issue = new SourceCatalogueIssue(
            code: SourceCatalogueIssueCode.PhysicalAlias,
            attemptedCanonicalPath: removed.Identity.CanonicalBasePath,
            relatedPaths: [removed.Identity.CanonicalBasePath, removedAlias.Identity.CanonicalBasePath],
            scopePhysicalPath: GeneratedNavigationTestData.Physical("root"),
            failure: null);
        var catalogue = GeneratedNavigationTestData.Catalogue(
            [loader, root, removed, removedAlias],
            issues: [issue]);

        var formation = Build(catalogue, [loader, root]);

        Assert.Null(formation.FindSource(removed.Identity.CanonicalBasePath));
        Assert.DoesNotContain(
            formation.Topology.Nodes,
            node => node.Identity.CanonicalBasePath == removed.Identity.CanonicalBasePath);
        Assert.Empty(formation.Topology.FindByPath(root.Identity.CanonicalBasePath)?.ChildPaths ?? []);
        Assert.Empty(formation.PhysicalAliasGroups);
        Assert.Empty(formation.Ambiguities);
        Assert.Empty(formation.Issues);
    }

    [Fact(DisplayName = "Projection ingress accepts exact intended sources and rejects clones and removed sources")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void ProjectionIngressUsesOnlyIntendedMembershipReferences()
    {
        var retained = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint);
        var removed = Source(".agents/root/removed.md", SourceDocumentForm.Markdown);
        var added = Source(".agents/root/added.md", SourceDocumentForm.Markdown);
        var catalogue = GeneratedNavigationTestData.Catalogue([retained, removed]);
        var formation = Build(catalogue, [retained, added]);
        var clone = new SourceLogicalSource(added.Identity, added.Base);

        var request = new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(added, "unavailable")],
            metadata: []);
        var cloneException = Assert.Throws<ArgumentException>(() => new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(clone, "unavailable")],
            metadata: []));
        var removedException = Assert.Throws<ArgumentException>(() => new GeneratedNavigationProjectionRequest(
            formation: formation,
            regions: [new GeneratedNavigationRegionInput(removed, "unavailable")],
            metadata: []));

        Assert.Same(added, Assert.Single(request.Regions).Source);
        Assert.Equal("regions", cloneException.ParamName);
        Assert.Equal("regions", removedException.ParamName);
    }

    private static GeneratedNavigationFormation Build(
        SourceCatalogue catalogue,
        IReadOnlyList<SourceLogicalSource> intendedSources)
    {
        return new GeneratedNavigationFormationBuilder().Build(catalogue, intendedSources);
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form,
        string? physicalPath = null)
    {
        return GeneratedNavigationTestData.Source(canonicalPath, form, physicalPath);
    }

    private static string NodeShape(SourceRouteNode node)
        => $"{node.Identity.CanonicalBasePath}|{node.ParentState}|{string.Join(',', node.ParentPaths)}|{string.Join(',', node.ChildPaths)}";

    private static string AliasShape(GeneratedNavigationPhysicalAliasGroup group)
        => $"{group.PhysicalPath}|{group.Compatibility}|{string.Join(',', group.Candidates.Select(candidate => candidate.CanonicalPath))}";
    private static string AmbiguityShape(GeneratedNavigationFormationAmbiguity ambiguity)
    {
        var sources = string.Join(',', ambiguity.IntendedSources.Select(source => source.Identity.CanonicalBasePath));
        var candidates = string.Join(',', ambiguity.Candidates.Select(candidate => candidate.CanonicalPath));
        return $"{ambiguity.Kind}|{ambiguity.Subject}|{sources}|{candidates}";
    }
}
