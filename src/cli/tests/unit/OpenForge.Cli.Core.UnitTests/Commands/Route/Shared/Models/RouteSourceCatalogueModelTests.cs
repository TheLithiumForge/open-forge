using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

public sealed class RouteSourceCatalogueModelTests
{
    [Fact(DisplayName = "Route source catalogue orders sources, ID collisions, and paths deterministically")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CatalogueUsesOrdinalCanonicalOrdering()
    {
        var leaf = RouteSourceTestData.Source(".agents/collision.md", RouteSourceKind.Markdown);
        var entrypoint = RouteSourceTestData.Source(".agents/collision/_collision.md", RouteSourceKind.Entrypoint);
        var compatibility = RouteSourceTestData.Source(
            ".agents/collision/index.md",
            RouteSourceKind.Entrypoint,
            RouteSourceForm.IndexEntrypoint);
        var catalogue = new RouteSourceCatalogue([compatibility, leaf, entrypoint], []);

        Assert.Equal(
            [".agents/collision.md", ".agents/collision/_collision.md", ".agents/collision/index.md"],
            catalogue.Sources.Select(source => source.CanonicalPath));
        Assert.Equal(
            [".agents/collision.md", ".agents/collision/_collision.md", ".agents/collision/index.md"],
            catalogue.FindById("collision").Select(source => source.CanonicalPath));
        var collision = Assert.Single(catalogue.IdentityCollisions);
        Assert.Equal("collision", collision.Id);
        Assert.Equal(catalogue.FindById("collision").Select(source => source.CanonicalPath), collision.Paths);
    }

    [Fact(DisplayName = "Route source catalogue resolves base and overwrite paths to one source while retaining bodies")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BaseAndOverwriteLookupShareOneLogicalSource()
    {
        var source = RouteSourceTestData.Source(
            ".agents/guidance/style.md",
            RouteSourceKind.Markdown,
            overwritePath: ".agents/guidance/style.overwrite.md");
        var overwrite = source.Overwrite!;
        var catalogue = new RouteSourceCatalogue(
            [source],
            [new RouteOverwriteFact(RouteOverwriteState.Paired, overwrite, [source.CanonicalPath])]);

        Assert.Same(source, catalogue.FindByPath(source.CanonicalPath));
        Assert.Same(source, catalogue.FindByPath(overwrite.CanonicalLogicalPath));
        Assert.Same(source, catalogue.FindById(source.Id).Single());
        Assert.Same(overwrite, catalogue.FindOverwriteByPath(overwrite.CanonicalLogicalPath)!.Overwrite);
        Assert.Equal("body", source.Base.Body);
        Assert.Equal("body", overwrite.Body);
    }

    [Fact(DisplayName = "Route source catalogue retains paired, orphan, and ambiguous overwrite facts for lookup")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void OverwriteFactsRemainCatalogueEvidence()
    {
        var source = RouteSourceTestData.Source(
            ".agents/root/leaf.md",
            RouteSourceKind.Markdown,
            overwritePath: ".agents/root/leaf.overwrite.md");
        var paired = new RouteOverwriteFact(
            RouteOverwriteState.Paired,
            source.Overwrite!,
            [source.CanonicalPath]);
        var orphanDocument = RouteSourceTestData.Document(
            ".agents/orphan.overwrite.md",
            RouteSourceForm.OverwriteCompanion);
        var orphan = new RouteOverwriteFact(RouteOverwriteState.Orphan, orphanDocument, []);
        var ambiguousDocument = RouteSourceTestData.Document(
            ".agents/shared.overwrite.md",
            RouteSourceForm.OverwriteCompanion);
        var ambiguous = new RouteOverwriteFact(
            RouteOverwriteState.Ambiguous,
            ambiguousDocument,
            [".agents/a.md", ".agents/b.md"]);
        var catalogue = new RouteSourceCatalogue([source], [ambiguous, orphan, paired]);

        Assert.Equal(
            [RouteOverwriteState.Orphan, RouteOverwriteState.Paired, RouteOverwriteState.Ambiguous],
            catalogue.OverwriteFacts.Select(fact => fact.State));
        Assert.Same(orphan, catalogue.FindOverwriteByPath(orphan.CanonicalPath));
        Assert.Equal([".agents/a.md", ".agents/b.md"], catalogue.FindOverwriteByPath(ambiguous.CanonicalPath)!.CandidateBasePaths);
        Assert.Same(paired, catalogue.FindOverwriteByPath(paired.CanonicalPath));
    }

    [Fact(DisplayName = "Route source catalogue snapshots input collections and stable lookup results")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CatalogueResultsAreStableSnapshots()
    {
        var first = RouteSourceTestData.Source(".agents/first.md", RouteSourceKind.Markdown);
        var second = RouteSourceTestData.Source(".agents/second.md", RouteSourceKind.Markdown);
        var sources = new List<RouteSource> { second, first };
        var catalogue = new RouteSourceCatalogue(sources, []);
        sources.Clear();

        Assert.Equal([".agents/first.md", ".agents/second.md"], catalogue.Sources.Select(source => source.CanonicalPath));
        Assert.Equal(catalogue.FindById("first"), catalogue.FindById("first"));
    }

    [Fact(DisplayName = "Route source catalogue rejects duplicate canonical paths")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DuplicateCanonicalPathsAreRejected()
    {
        var first = RouteSourceTestData.Source(".agents/one.md", RouteSourceKind.Markdown);
        var duplicate = RouteSourceTestData.Source(".agents/one.md", RouteSourceKind.Markdown);

        Assert.Throws<ArgumentException>(() => new RouteSourceCatalogue([first, duplicate], []));
    }
}
