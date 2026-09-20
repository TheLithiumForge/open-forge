using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

public sealed class RouteSourceCatalogueModelTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route projection sets order projections, identity collisions, and projected paths deterministically")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProjectionSetUsesOrdinalCanonicalOrdering()
    {
        var leaf = RouteSourceTestData.LogicalSource(".agents/collision.md", "collision");
        var entrypoint = RouteSourceTestData.LogicalSource(
            ".agents/collision/_collision.md",
            "collision",
            SourceDocumentForm.CanonicalEntrypoint);
        var compatibility = RouteSourceTestData.LogicalSource(
            ".agents/collision/index.md",
            "collision",
            SourceDocumentForm.IndexEntrypoint);
        var projections = new[]
        {
            RouteSourceTestData.Projection(compatibility),
            RouteSourceTestData.Projection(leaf),
            RouteSourceTestData.Projection(entrypoint),
        };
        var projectionSet = new RouteSourceProjectionSet(projections, []);

        Assert.Equal(
            [
                ".agents/collision.md",
                ".agents/collision/_collision.md",
                ".agents/collision/index.md",
            ],
            projectionSet.Sources.Select(source => source.CanonicalPath));
        var collision = Assert.Single(projectionSet.IdentityCollisions);
        Assert.Equal("collision", collision.Id);
        Assert.Equal(
            projectionSet.Sources.Select(source => source.CanonicalPath),
            collision.Paths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route projection sets retain paired overwrite facts and projected source reference identity")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void PairedOverwriteFactMatchesItsExactNeutralLayer()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(
            ".agents/guidance/style.md",
            withOverwrite: true);
        var projection = RouteSourceTestData.Projection(logicalSource);
        var source = Assert.IsType<RouteSource>(projection.Source);
        var overwrite = Assert.IsType<RouteSourceDocument>(source.Overwrite);
        var paired = new RouteOverwriteFact(
            RouteOverwriteState.Paired,
            overwrite,
            [logicalSource.Identity.CanonicalBasePath]);
        var projectionSet = new RouteSourceProjectionSet([projection], [paired]);

        Assert.Same(projection.Source, Assert.Single(projectionSet.Sources));
        Assert.Same(paired, Assert.Single(projectionSet.OverwriteFacts));
        Assert.Equal(RouteOverwriteState.Paired, projectionSet.OverwriteFacts[0].State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route projection sets retain paired orphan and ambiguous overwrite facts for command-local mapping")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void OverwriteFactsRemainProjectionEvidence()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(
            ".agents/root/leaf.md",
            withOverwrite: true);
        var projection = RouteSourceTestData.Projection(logicalSource);
        var source = Assert.IsType<RouteSource>(projection.Source);
        var paired = new RouteOverwriteFact(
            RouteOverwriteState.Paired,
            Assert.IsType<RouteSourceDocument>(source.Overwrite),
            [logicalSource.Identity.CanonicalBasePath]);
        var orphanDocument = RouteSourceTestData.Document(
            ".agents/orphan.overwrite.md",
            SourceDocumentForm.OverwriteCompanion);
        var orphan = new RouteOverwriteFact(RouteOverwriteState.Orphan, orphanDocument, []);
        var ambiguousDocument = RouteSourceTestData.Document(
            ".agents/shared.overwrite.md",
            SourceDocumentForm.OverwriteCompanion);
        var ambiguous = new RouteOverwriteFact(
            RouteOverwriteState.Ambiguous,
            ambiguousDocument,
            [".agents/a.md", ".agents/b.md"]);
        var projectionSet = new RouteSourceProjectionSet(
            [projection],
            [ambiguous, orphan, paired]);

        Assert.Equal(
            [RouteOverwriteState.Orphan, RouteOverwriteState.Paired, RouteOverwriteState.Ambiguous],
            projectionSet.OverwriteFacts.Select(fact => fact.State));
        Assert.Equal(
            [".agents/a.md", ".agents/b.md"],
            projectionSet.OverwriteFacts.Single(fact => fact.State == RouteOverwriteState.Ambiguous).CandidateBasePaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route projection set lookup preserves the existing ID and exact path meanings")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProjectionLookupsResolveBasesPairedOverwritesAndIDs()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(
            ".agents/guidance/style.md",
            withOverwrite: true);
        var projection = RouteSourceTestData.Projection(logicalSource);
        var source = Assert.IsType<RouteSource>(projection.Source);
        var paired = new RouteOverwriteFact(
            RouteOverwriteState.Paired,
            Assert.IsType<RouteSourceDocument>(source.Overwrite),
            [logicalSource.Identity.CanonicalBasePath]);
        var projectionSet = new RouteSourceProjectionSet([projection], [paired]);

        Assert.Same(projection.Source, Assert.Single(projectionSet.FindAllById("guidance/style")));
        Assert.Same(
            projection.Source,
            projectionSet.FindByPath(".agents/guidance/style.md"));
        Assert.Same(
            paired,
            projectionSet.FindOverwriteByPath(".agents/guidance/style.overwrite.md"));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route projection build results associate every orphan or ambiguous read with one source-less overwrite fact")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProjectionResultRetainsTypedSourceLessReads()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(".agents/root/leaf.md");
        var projection = RouteSourceTestData.Projection(logicalSource);
        var orphanLayer = new SourceLayer(
            ".agents/root/orphan.overwrite.md",
            RouteSourceTestData.PhysicalPath(".agents/root/orphan.overwrite.md"),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        var orphanRead = RouteSourceTestData.Read(
            logicalSource,
            orphanLayer,
            "orphan");
        var orphan = new RouteOverwriteFact(
            RouteOverwriteState.Orphan,
            RouteSourceTestData.Document(
                ".agents/root/orphan.overwrite.md",
                SourceDocumentForm.OverwriteCompanion),
            []);
        var projectionSet = new RouteSourceProjectionSet([projection], [orphan]);
        var result = new RouteSourceProjectionBuildResult(
            projectionSet,
            [projection],
            [orphanRead],
            isCancelled: false);

        Assert.Same(projectionSet, result.ProjectionSet);
        Assert.Same(projection, Assert.Single(result.Projections));
        Assert.Same(orphanRead, Assert.Single(result.ReadResults));
        Assert.False(result.IsCancelled);
    }
}
