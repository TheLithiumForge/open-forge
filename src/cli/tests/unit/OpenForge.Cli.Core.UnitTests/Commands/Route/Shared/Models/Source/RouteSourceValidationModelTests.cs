using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

public sealed class RouteSourceValidationModelTests
{
    [Fact(DisplayName = "Route projection rejects reads that do not reference the neutral logical source layers")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProjectionLayerAssociationIsStrict()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(
            ".agents/root/leaf.md",
            withOverwrite: true);
        var other = RouteSourceTestData.LogicalSource(".agents/root/other.md");
        var baseRead = RouteSourceTestData.Read(logicalSource, logicalSource.Base);
        var otherRead = RouteSourceTestData.Read(other, other.Base);
        var overwriteLayer = Assert.IsType<SourceLayer>(logicalSource.Overwrite);
        var overwriteRead = RouteSourceTestData.Read(logicalSource, overwriteLayer);

        Assert.Throws<ArgumentException>(() => new RouteSourceProjection(
            logicalSource,
            null,
            otherRead,
            overwriteRead));
        Assert.Throws<ArgumentException>(() => new RouteSourceProjection(
            logicalSource,
            null,
            baseRead,
            otherRead));
        Assert.Throws<ArgumentException>(() => new RouteSourceProjection(
            RouteSourceTestData.LogicalSource(".agents/root/no-overwrite.md"),
            null,
            RouteSourceTestData.Read(
                RouteSourceTestData.LogicalSource(".agents/root/no-overwrite.md"),
                RouteSourceTestData.LogicalSource(".agents/root/no-overwrite.md").Base),
            overwriteRead));
    }

    [Fact(DisplayName = "Route projection rejects Route output whose identity or projected overwrite presence disagrees with neutral facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProjectionOutputMustMatchNeutralIdentityAndSelectedLayers()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(
            ".agents/root/leaf.md",
            withOverwrite: true);
        var baseRead = RouteSourceTestData.Read(logicalSource, logicalSource.Base);
        var overwriteLayer = Assert.IsType<SourceLayer>(logicalSource.Overwrite);
        var overwriteRead = RouteSourceTestData.Read(logicalSource, overwriteLayer);
        var wrongIdentity = RouteSourceTestData.Source(
            RouteSourceTestData.LogicalSource(".agents/root/other.md"));

        Assert.Throws<ArgumentException>(() => new RouteSourceProjection(
            logicalSource,
            wrongIdentity,
            baseRead,
            overwriteRead));
        Assert.Throws<ArgumentException>(() => new RouteSourceProjection(
            logicalSource,
            RouteSourceTestData.Source(logicalSource, withOverwrite: true),
            baseRead,
            null));
    }

    [Fact(DisplayName = "Route projection sets reject duplicate projection paths and unmatched paired facts")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ProjectionSetValidatesReferenceAndOverwriteSetEquality()
    {
        var first = RouteSourceTestData.LogicalSource(".agents/root/one.md");
        var second = RouteSourceTestData.LogicalSource(".agents/root/one.md");
        var firstProjection = RouteSourceTestData.Projection(first);
        var secondProjection = RouteSourceTestData.Projection(second);
        Assert.Throws<ArgumentException>(() => new RouteSourceProjectionSet(
            [firstProjection, secondProjection],
            []));

        var overwriteSource = RouteSourceTestData.LogicalSource(
            ".agents/root/with-overwrite.md",
            withOverwrite: true);
        var projection = RouteSourceTestData.Projection(overwriteSource);
        var mismatchedFact = new RouteOverwriteFact(
            RouteOverwriteState.Paired,
            RouteSourceTestData.Document(
                ".agents/root/other.overwrite.md",
                SourceDocumentForm.OverwriteCompanion),
            [overwriteSource.Identity.CanonicalBasePath]);
        Assert.Throws<ArgumentException>(() => new RouteSourceProjectionSet(
            [projection],
            [mismatchedFact]));
    }

    [Fact(DisplayName = "Route projection result rejects a source-less read without a matching orphan or ambiguous fact")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void SourceLessReadsMustMatchProjectionFacts()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(".agents/root/leaf.md");
        var projection = RouteSourceTestData.Projection(logicalSource);
        var orphan = new RouteOverwriteFact(
            RouteOverwriteState.Orphan,
            RouteSourceTestData.Document(
                ".agents/root/orphan.overwrite.md",
                SourceDocumentForm.OverwriteCompanion),
            []);
        var projectionSet = new RouteSourceProjectionSet([projection], [orphan]);
        var wrongLayer = new SourceLayer(
            ".agents/root/different.overwrite.md",
            RouteSourceTestData.PhysicalPath(".agents/root/different.overwrite.md"),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        var wrongRead = RouteSourceTestData.Read(logicalSource, wrongLayer);

        Assert.Throws<ArgumentException>(() => new RouteSourceProjectionBuildResult(
            projectionSet,
            [projection],
            [wrongRead],
            false));
    }

    [Fact(DisplayName = "Route source model validation still rejects invalid intrinsic form, read, and overwrite shapes")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void ExistingRouteOutputInvariantsRemainGreen()
    {
        var markdown = RouteSourceTestData.Document(
            ".agents/root/leaf.md",
            SourceDocumentForm.Markdown);
        var metadata = RouteSourceMetadata.Complete("Leaf", ["Route"], false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(
            markdown,
            metadata,
            RouteSourceKind.Entrypoint));

        var companion = RouteSourceTestData.Document(
            ".agents/root/leaf.overwrite.md",
            SourceDocumentForm.OverwriteCompanion);
        Assert.Throws<ArgumentException>(() => new RouteSource(
            markdown,
            metadata,
            RouteSourceKind.Markdown,
            companion));

        Assert.Throws<ArgumentException>(() => new RouteSourceDocument(
            ".agents/root/leaf.md",
            RouteSourceTestData.PhysicalPath(".agents/root/leaf.md"),
            SourceDocumentForm.Markdown,
            FileReadState.Missing,
            "body"));
    }
}
