using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.List.Shared.Selection;

public sealed class RouteListSourceCatalogueTests
{
    [Fact(DisplayName = "Route-list source catalogue orders ID collisions by ordinal canonical path")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void IdCandidatesUseOrdinalCanonicalPathOrder()
    {
        var sources = new[]
        {
            Source("collision", ".agents/collision.md"),
            Source("collision", ".agents/collision/_collision.md", RouteListSourceKind.Entrypoint),
            Source("collision", ".agents/collision/index.md", RouteListSourceKind.Entrypoint),
        };
        var catalogue = new RouteListSourceCatalogue(sources);

        var candidates = catalogue.FindById("collision");

        Assert.Equal(
            [
                ".agents/collision.md",
                ".agents/collision/_collision.md",
                ".agents/collision/index.md",
            ],
            candidates.Select(source => source.CanonicalPath));
    }

    [Fact(DisplayName = "Route-list source catalogue resolves a base and overwrite path to one immutable logical source")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void BaseAndOverwritePathsShareOneLogicalSource()
    {
        var source = Source(
            "guidance/style",
            ".agents/guidance/style.md",
            overwritePath: ".agents/guidance/style.overwrite.md");
        var catalogue = new RouteListSourceCatalogue([source]);

        Assert.Same(source, catalogue.FindByPath(".agents/guidance/style.md"));
        Assert.Same(source, catalogue.FindByPath(".agents/guidance/style.overwrite.md"));
        Assert.Null(catalogue.FindByPath(".agents/guidance/missing.md"));
    }

    [Fact(DisplayName = "Route-list source catalogue snapshots source input and each collision result")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CatalogueAndCandidateResultsAreSnapshots()
    {
        var first = Source("stable", ".agents/stable.md");
        var second = Source(
            "stable",
            ".agents/stable/_stable.md",
            RouteListSourceKind.Entrypoint);
        var input = new List<RouteListSource> { first, second };
        var catalogue = new RouteListSourceCatalogue(input);
        input.Clear();

        var firstCandidates = catalogue.FindById("stable");
        var secondCandidates = catalogue.FindById("stable");

        Assert.Equal(2, firstCandidates.Count);
        Assert.Equal(
            [".agents/stable.md", ".agents/stable/_stable.md"],
            firstCandidates.Select(source => source.CanonicalPath));
        Assert.Equal(firstCandidates, secondCandidates);
    }

    [Fact(DisplayName = "Route-list source catalogue rejects duplicate canonical paths")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DuplicateCanonicalPathsAreRejected()
    {
        Assert.ThrowsAny<ArgumentException>(() => new RouteListSourceCatalogue([
            Source("one", ".agents/one.md"),
            Source("one", ".agents/one.md"),
        ]));
    }

    [Fact(DisplayName = "Route-list source catalogue rejects an overwrite alias that is its own base path")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SourceRejectsSelfAliasingOverwrite()
    {
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "one",
            ".agents/one.md",
            overwritePath: ".agents/one.md"));
    }

    [Fact(DisplayName = "Route-list source catalogue rejects forged IDs and non-adjacent overwrite aliases")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void SourceRejectsForgedIdentityAndOverwriteAliases()
    {
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "forged",
            ".agents/actual.md"));
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "one",
            ".agents/one.md",
            overwritePath: ".agents/other.overwrite.md"));
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "templates/value.yaml",
            ".agents/templates/value.yaml",
            overwritePath: ".agents/templates/value.overwrite.yaml"));
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "root/not-entrypoint",
            ".agents/root/not-entrypoint.md",
            RouteListSourceKind.Entrypoint));
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "root",
            ".agents/root/_root.overwrite.md",
            RouteListSourceKind.Entrypoint));
        Assert.ThrowsAny<ArgumentException>(() => Source(
            "loader",
            ".agents/loader.md",
            RouteListSourceKind.RoutedLeaf));
    }

    private static RouteListSource Source(
        string id,
        string canonicalPath,
        RouteListSourceKind kind = RouteListSourceKind.RoutedLeaf,
        string? overwritePath = null,
        bool isRouteAmbiguous = false)
    {
        var physicalPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        return new RouteListSource(id, canonicalPath, physicalPath, kind, overwritePath, isRouteAmbiguous);
    }
}
