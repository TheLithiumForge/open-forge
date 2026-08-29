using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

public sealed class GeneratedNavigationIntendedTargetCollisionTests
{
    [Fact(DisplayName = "Prospective generated navigation retains truthful observed aliases beside an unrelated intended addition")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void RetainedObservedAliasesRemainTruthful()
    {
        var compatiblePhysical = Physical("shared/compatible.md");
        var compatibleFirst = Source(".agents/root/a.md", SourceDocumentForm.Markdown, compatiblePhysical);
        var compatibleSecond = Source(".agents/root/z.md", SourceDocumentForm.Markdown, compatiblePhysical);
        var incompatiblePhysical = Physical("shared/incompatible.md");
        var incompatibleFirst = Source(
            ".agents/first/_first.md",
            SourceDocumentForm.CanonicalEntrypoint,
            incompatiblePhysical);
        var incompatibleSecond = Source(
            ".agents/second/_second.md",
            SourceDocumentForm.CanonicalEntrypoint,
            incompatiblePhysical);
        var added = Source(".agents/root/new.md", SourceDocumentForm.Markdown);
        SourceLogicalSource[] observed =
        [
            compatibleFirst,
            compatibleSecond,
            incompatibleFirst,
            incompatibleSecond,
        ];
        var catalogue = GeneratedNavigationTestData.Catalogue(observed);

        var formation = new GeneratedNavigationFormationBuilder().Build(
            catalogue,
            [.. observed, added]);

        Assert.Equal(2, formation.PhysicalAliasGroups.Count);
        Assert.Contains(
            formation.PhysicalAliasGroups,
            group => group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Compatible
                && group.Candidates.Select(candidate => candidate.CanonicalPath).SequenceEqual(
                    [compatibleFirst.Identity.CanonicalBasePath, compatibleSecond.Identity.CanonicalBasePath]));
        Assert.Contains(
            formation.PhysicalAliasGroups,
            group => group.Compatibility == GeneratedNavigationPhysicalAliasCompatibility.Incompatible
                && group.Candidates.Select(candidate => candidate.CanonicalPath).SequenceEqual(
                    [incompatibleFirst.Identity.CanonicalBasePath, incompatibleSecond.Identity.CanonicalBasePath]));
        Assert.Empty(formation.IntendedTargetCollisions);
    }

    [Fact(DisplayName = "Intended target collisions type new-new and new-retained targets without stale removed observations")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedCollisionsUseExactLayersWithoutFabricatedCandidates()
    {
        var retainedTarget = Physical("shared/retained.md");
        var removedTarget = Physical("shared/removed.md");
        var newTarget = Physical("shared/new.md");
        var retained = Source(".agents/root/retained.md", SourceDocumentForm.Markdown, retainedTarget);
        var removed = Source(".agents/root/removed.md", SourceDocumentForm.Markdown, removedTarget);
        var againstRetained = Source(".agents/root/against-retained.md", SourceDocumentForm.Markdown, retainedTarget);
        var firstNew = Source(".agents/root/first-new.md", SourceDocumentForm.Markdown, newTarget);
        var secondNew = Source(".agents/root/second-new.md", SourceDocumentForm.Markdown, newTarget);
        var reusingRemoved = Source(".agents/root/reuse-removed.md", SourceDocumentForm.Markdown, removedTarget);
        var catalogue = GeneratedNavigationTestData.Catalogue([retained, removed]);

        var formation = new GeneratedNavigationFormationBuilder().Build(
            catalogue,
            [retained, againstRetained, firstNew, secondNew, reusingRemoved]);

        Assert.Equal(2, formation.IntendedTargetCollisions.Count);
        var retainedCollision = Assert.Single(
            formation.IntendedTargetCollisions,
            collision => string.Equals(collision.TargetPath, retainedTarget, StringComparison.Ordinal));
        Assert.Equal(
            [againstRetained.Identity.CanonicalBasePath, retained.Identity.CanonicalBasePath],
            retainedCollision.Layers.Select(layer => layer.CanonicalPath));
        Assert.Same(againstRetained.Base, retainedCollision.Layers[0]);
        Assert.Same(retained.Base, retainedCollision.Layers[1]);
        var newCollision = Assert.Single(
            formation.IntendedTargetCollisions,
            collision => string.Equals(collision.TargetPath, newTarget, StringComparison.Ordinal));
        Assert.Equal(
            [firstNew.Identity.CanonicalBasePath, secondNew.Identity.CanonicalBasePath],
            newCollision.Layers.Select(layer => layer.CanonicalPath));
        Assert.DoesNotContain(
            formation.IntendedTargetCollisions,
            collision => string.Equals(collision.TargetPath, removedTarget, StringComparison.Ordinal));
        Assert.Empty(formation.PhysicalAliasGroups);
        Assert.Null(catalogue.FindCandidateByPath(againstRetained.Identity.CanonicalBasePath));
        Assert.Null(catalogue.FindCandidateByPath(firstNew.Identity.CanonicalBasePath));
        Assert.Null(catalogue.FindCandidateByPath(secondNew.Identity.CanonicalBasePath));
        Assert.Null(formation.FindSource(removed.Identity.CanonicalBasePath));
        Assert.Same(reusingRemoved, formation.FindSource(reusingRemoved.Identity.CanonicalBasePath));
    }

    [Fact(DisplayName = "An intended colliding root is not admitted as a Loader root")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void IntendedCollisionPreventsLoaderRootAdmission()
    {
        var shared = Physical("shared/root-target.md");
        var loader = Source(".agents/loader.md", SourceDocumentForm.Loader);
        var root = Source(".agents/root/_root.md", SourceDocumentForm.CanonicalEntrypoint, shared);
        var collision = Source(".agents/content.md", SourceDocumentForm.Markdown, shared);
        var catalogue = GeneratedNavigationTestData.Catalogue([]);

        var formation = new GeneratedNavigationFormationBuilder().Build(
            catalogue,
            [loader, root, collision]);

        Assert.Single(formation.IntendedTargetCollisions);
        Assert.Empty(formation.Topology.LoaderRootPaths);
    }

    [Fact(DisplayName = "Generated-navigation formation owns read-only alias and intended-target collision collections")]
    [Trait("Feature", "generated-navigation"), Trait("Evidence", "Unit")]
    public void FormationCollisionCollectionsRejectMutation()
    {
        var observedTarget = Physical("shared/observed.md");
        var observedFirst = Source(".agents/root/observed-first.md", SourceDocumentForm.Markdown, observedTarget);
        var observedSecond = Source(".agents/root/observed-second.md", SourceDocumentForm.Markdown, observedTarget);
        var intendedTarget = Physical("shared/intended.md");
        var intendedFirst = Source(".agents/root/intended-first.md", SourceDocumentForm.Markdown, intendedTarget);
        var intendedSecond = Source(".agents/root/intended-second.md", SourceDocumentForm.Markdown, intendedTarget);
        var catalogue = GeneratedNavigationTestData.Catalogue([observedFirst, observedSecond]);

        var formation = new GeneratedNavigationFormationBuilder().Build(
            catalogue,
            [observedFirst, observedSecond, intendedFirst, intendedSecond]);

        var aliases = Assert.IsType<ReadOnlyCollection<GeneratedNavigationPhysicalAliasGroup>>(
            formation.PhysicalAliasGroups);
        var intendedCollisions = Assert.IsType<ReadOnlyCollection<GeneratedNavigationIntendedTargetCollision>>(
            formation.IntendedTargetCollisions);
        Assert.Throws<NotSupportedException>(() => ((IList<GeneratedNavigationPhysicalAliasGroup>)aliases).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<GeneratedNavigationIntendedTargetCollision>)intendedCollisions).Clear());
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
