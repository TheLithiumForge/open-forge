using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationObservedTargetCollisionTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A new intended root colliding with an observed unrecognized occupant is not admitted")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void NewRootReportsItsObservedTargetOccupant()
    {
        var target = Physical("shared/root.md");
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint, target);
        var occupant = GeneratedNavigationTestData.UnrecognizedCandidate(".agents/occupied.md", target);
        var catalogue = GeneratedNavigationTestData.Catalogue([loader], [occupant]);

        var formation = Build(catalogue, [loader, root]);

        var collision = Assert.Single(formation.IntendedTargetCollisions);
        Assert.Equal(target, collision.TargetPath);
        Assert.Same(root.Base, Assert.Single(collision.Layers));
        Assert.Same(occupant, Assert.Single(collision.ObservedCandidates));
        Assert.Empty(formation.Topology.LoaderRootPaths);
        Assert.Empty(formation.PhysicalAliasGroups);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A same-path replacement reports a separate observed occupant without inheriting its old candidate")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void SameCanonicalReplacementKeepsSeparateObservedOccupancy()
    {
        var target = Physical("shared/replacement.md");
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var observedRoot = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint, target);
        var replacement = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint, target);
        var occupant = GeneratedNavigationTestData.Candidate(
            canonicalPath: ".agents/alias.md",
            form: SourceDocumentForm.Markdown,
            physicalPath: target);
        var catalogue = GeneratedNavigationTestData.Catalogue([loader, observedRoot], [occupant]);

        var formation = Build(catalogue, [loader, replacement]);

        Assert.Same(replacement, formation.FindSource(replacement.Identity.CanonicalBasePath));
        var collision = Assert.Single(formation.IntendedTargetCollisions);
        Assert.Same(replacement.Base, Assert.Single(collision.Layers));
        Assert.Same(occupant, Assert.Single(collision.ObservedCandidates));
        Assert.DoesNotContain(
            collision.ObservedCandidates,
            candidate => candidate.CanonicalPath == observedRoot.Identity.CanonicalBasePath);
        Assert.Empty(formation.Topology.LoaderRootPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A removed canonical layer leaves its former target available for an intended replacement")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void RemovedLayerTargetCanBeReused()
    {
        var target = Physical("shared/reused.md");
        var removed = Source(".agents/root/removed.md", SourceDocumentForm.Markdown, target);
        var replacement = Source(".agents/root/replacement.md", SourceDocumentForm.Markdown, target);
        var catalogue = GeneratedNavigationTestData.Catalogue([removed]);

        var formation = Build(catalogue, [replacement]);

        Assert.Empty(formation.IntendedTargetCollisions);
        Assert.Empty(formation.PhysicalAliasGroups);
        Assert.Null(formation.FindSource(removed.Identity.CanonicalBasePath));
        Assert.Same(replacement, formation.FindSource(replacement.Identity.CanonicalBasePath));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Observed base and overwrite layers are reconciled independently")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void ReplacementRetainsBaseEvidenceAndDropsItsRemovedOverwriteEvidence()
    {
        var baseTarget = Physical("shared/base.md");
        var overwriteTarget = Physical("shared/overwrite.md");
        var observed = GeneratedNavigationTestData.Source(
            ".agents/root/source.md",
            SourceDocumentForm.Markdown,
            physicalPath: baseTarget,
            overwritePhysicalPath: overwriteTarget);
        var replacement = Source(observed.Identity.CanonicalBasePath, SourceDocumentForm.Markdown, baseTarget);
        var reuse = Source(".agents/root/reuse.md", SourceDocumentForm.Markdown, overwriteTarget);
        var baseOccupant = GeneratedNavigationTestData.Candidate(
            canonicalPath: ".agents/base-occupant.md",
            form: SourceDocumentForm.Markdown,
            physicalPath: baseTarget);
        var catalogue = GeneratedNavigationTestData.Catalogue([observed], [baseOccupant]);

        var formation = Build(catalogue, [replacement, reuse]);

        var alias = Assert.Single(formation.PhysicalAliasGroups);
        Assert.Equal(
            [baseOccupant.CanonicalPath, observed.Base.CanonicalPath],
            alias.Candidates.Select(candidate => candidate.CanonicalPath));
        var collision = Assert.Single(formation.IntendedTargetCollisions);
        Assert.Equal(baseTarget, collision.TargetPath);
        Assert.Same(baseOccupant, Assert.Single(collision.ObservedCandidates));
        Assert.DoesNotContain(
            formation.IntendedTargetCollisions,
            candidate => candidate.TargetPath == overwriteTarget);
        Assert.Same(reuse, formation.FindSource(reuse.Identity.CanonicalBasePath));
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

    private static string Physical(string relativePath)
    {
        return GeneratedNavigationTestData.Physical(relativePath);
    }
}
