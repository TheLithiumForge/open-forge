using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Selection;

public sealed class SourceUniverseFilterResolverTests
{
    [Fact(DisplayName = "Ordered source-universe selectors retain occurrences while exclusion wins the deduplicated set")]
    [Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void OrderedSelectorsRetainReportingFactsAndExclusionPrecedence()
    {
        var docs = SourceInventoryTestData.Source(
            ".agents/docs/_docs.md",
            "docs",
            SourceDocumentForm.CanonicalEntrypoint);
        var guide = SourceInventoryTestData.Source(
            ".agents/docs/guide.md",
            withOverwrite: true);
        var nested = SourceInventoryTestData.Source(".agents/docs/nested/topic.md");
        var other = SourceInventoryTestData.Source(".agents/other.md");
        var catalogue = Catalogue(docs, guide, nested, other);
        var resolver = CreateResolver();
        var resolution = resolver.Resolve(new SourceUniverseFilterRequest(
            catalogue,
            Scope(".agents"),
            [
                new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 1),
                new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, "docs/guide", 2),
                new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "other", 3),
                new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "docs", 4),
            ]));

        Assert.True(resolution.IsResolved);
        Assert.Equal([1, 2, 3, 4], resolution.Selectors.Select(selector => selector.Occurrence.Position));
        Assert.Equal([1, 1, 2, 3], resolution.Selectors.Select(selector => selector.RoleOccurrence));
        Assert.Equal(
            [
                SourceUniverseSelectorRole.Include,
                SourceUniverseSelectorRole.Exclude,
                SourceUniverseSelectorRole.Include,
                SourceUniverseSelectorRole.Include,
            ],
            resolution.Selectors.Select(selector => selector.Occurrence.Role));
        Assert.Equal(
            [
                ".agents/docs/_docs.md",
                ".agents/docs/nested/topic.md",
                ".agents/other.md",
            ],
            resolution.Selection.Sources.Select(source => source.Identity.CanonicalBasePath));
        Assert.DoesNotContain(
            resolution.Selection.Sources,
            source => source.Identity.CanonicalBasePath == guide.Identity.CanonicalBasePath);
    }

    [Fact(DisplayName = "Folder selectors expand physically while ordinary and overwrite paths retain one logical source")]
    [Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void ExpansionUsesPhysicalKindAndLogicalSourceIdentity()
    {
        var skill = SourceInventoryTestData.Source(
            ".agents/skills/example/SKILL.md",
            "skills/example",
            SourceDocumentForm.Skill);
        var resource = SourceInventoryTestData.Source(".agents/skills/example/references/topic.md");
        var ordinary = SourceInventoryTestData.Source(
            ".agents/ordinary.md",
            withOverwrite: true);
        var catalogue = Catalogue(skill, resource, ordinary);
        var resolver = CreateResolver();

        var folder = resolver.Resolve(new SourceUniverseFilterRequest(
            catalogue,
            Scope(".agents"),
            [new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "skills/example", 1)]));
        Assert.Equal(SourceUniverseSelectorExpansion.Folder, folder.Selectors[0].Expansion);
        Assert.Equal(2, folder.Selection.Sources.Count);

        var overwrite = resolver.Resolve(new SourceUniverseFilterRequest(
            catalogue,
            Scope(".agents"),
            [new SourceUniverseSelectorOccurrence(
                SourceUniverseSelectorRole.Include,
                ".agents/ordinary.overwrite.md",
                1)]));
        Assert.Equal(SourceUniverseSelectorExpansion.Source, overwrite.Selectors[0].Expansion);
        Assert.Same(ordinary, Assert.Single(overwrite.Selection.Sources));
    }

    [Fact(DisplayName = "One unresolved selector preserves its ordered fact and admits no effective sources")]
    [Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void UnresolvedSelectorStopsEffectiveSetFormationWithoutErasingEvidence()
    {
        var catalogue = Catalogue(SourceInventoryTestData.Source(".agents/ordinary.md"));
        var resolution = CreateResolver().Resolve(new SourceUniverseFilterRequest(
            catalogue,
            Scope(".agents"),
            [new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Include, "missing", 1)]));

        Assert.False(resolution.IsResolved);
        Assert.Equal(SourceReferenceResolutionState.Unknown, Assert.Single(resolution.Selectors).Reference.State);
        Assert.Empty(resolution.Selection.Sources);
        Assert.Empty(resolution.Selection.Candidates);
    }

    private static SourceUniverseFilterResolver CreateResolver()
        => new(new SourceReferenceResolver((_, path) =>
            PhysicalPathResolution.Classified(PhysicalPathState.Missing, path)));

    private static SourceCatalogue Catalogue(params SourceLogicalSource[] sources)
        => new(
            SourceInventoryTestData.Workspace(),
            sources.SelectMany(source =>
            {
                var values = new List<SourceCandidate>
                {
                    Candidate(source.Base, source.Identity.AutomaticId),
                };
                if (source.Overwrite is { } overwrite)
                {
                    values.Add(Candidate(overwrite, source.Identity.AutomaticId));
                }

                return values;
            }),
            sources,
            [],
            false);

    private static SourceCandidate Candidate(SourceLayer layer, string id)
        => SourceInventoryTestData.Candidate(
            layer.CanonicalPath,
            layer.Form,
            id,
            PhysicalPathState.Contained,
            layer.PhysicalPath,
            Path.GetDirectoryName(layer.PhysicalPath));

    private static SourceCatalogueSelectionScope Scope(string canonicalPath)
        => new(canonicalPath, SourceInventoryTestData.Physical(canonicalPath));
}
