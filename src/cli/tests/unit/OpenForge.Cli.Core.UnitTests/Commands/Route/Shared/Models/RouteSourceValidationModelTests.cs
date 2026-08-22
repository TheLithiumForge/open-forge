using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

public sealed class RouteSourceValidationModelTests
{
    [Fact(DisplayName = "Route source models reject kind and intrinsic-form mismatches")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void KindAndFormMustAgree()
    {
        var markdown = RouteSourceTestData.Document(".agents/root/leaf.md", RouteSourceForm.Markdown);
        var metadata = RouteSourceMetadata.Complete("Leaf", ["Route"], false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(markdown, metadata, RouteSourceKind.Entrypoint));

        var loader = RouteSourceTestData.Document(".agents/loader.md", RouteSourceForm.Loader);
        var loaderMetadata = RouteSourceMetadata.WithoutValues(RouteSourceMetadataState.NotApplicable, false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(loader, loaderMetadata, RouteSourceKind.Markdown));

        var skill = RouteSourceTestData.Document(".agents/skills/native/SKILL.md", RouteSourceForm.Skill);
        Assert.Throws<ArgumentException>(() => new RouteSource(skill, metadata, RouteSourceKind.Native));
    }

    [Fact(DisplayName = "Route source models reject incompatible metadata compatibility and complete-value states")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MetadataMustAgreeWithTheSourceForm()
    {
        var compatibility = RouteSourceTestData.Document(".agents/root/index.md", RouteSourceForm.IndexEntrypoint);
        var nonCompatibility = RouteSourceMetadata.Complete("Index", ["Route"], false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(compatibility, nonCompatibility, RouteSourceKind.Entrypoint));

        var loader = RouteSourceTestData.Document(".agents/loader.md", RouteSourceForm.Loader);
        var loaderValues = RouteSourceMetadata.Complete("Loader", ["Route"], false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(loader, loaderValues, RouteSourceKind.Loader));

        var skill = RouteSourceTestData.Document(".agents/skills/native/SKILL.md", RouteSourceForm.Skill);
        var skillTags = RouteSourceMetadata.Complete("Skill", ["Route"], false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(skill, skillTags, RouteSourceKind.Native));

        var markdown = RouteSourceTestData.Document(".agents/root/leaf.md", RouteSourceForm.Markdown);
        var noTags = RouteSourceMetadata.Complete("Leaf", [], false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(markdown, noTags, RouteSourceKind.Markdown));
    }

    [Fact(DisplayName = "Route source models reject invalid read-body combinations and overwrite companions as bases")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DocumentAndOverwriteShapeIsStrict()
    {
        var path = ".agents/root/leaf.md";
        Assert.Throws<ArgumentException>(() => new RouteSourceDocument(
            path,
            RouteSourceTestData.PhysicalPath(path),
            RouteSourceForm.Markdown,
            FileReadState.Missing,
            "body"));

        var overwritePath = ".agents/root/leaf.overwrite.md";
        var overwrite = RouteSourceTestData.Document(overwritePath, RouteSourceForm.OverwriteCompanion);
        var metadata = RouteSourceMetadata.WithoutValues(RouteSourceMetadataState.Missing, false, false);
        Assert.Throws<ArgumentException>(() => new RouteSource(overwrite, metadata, RouteSourceKind.Markdown));
    }

    [Fact(DisplayName = "Route source models reject overwrite flag disagreement and nonadjacent companions")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void OverwriteShapeMustBeAdjacentAndDeclared()
    {
        var baseDocument = RouteSourceTestData.Document(".agents/root/leaf.md", RouteSourceForm.Markdown);
        var noOverwrite = RouteSourceMetadata.Complete("Leaf", ["Route"], false, false);
        var companion = RouteSourceTestData.Document(
            ".agents/root/leaf.overwrite.md",
            RouteSourceForm.OverwriteCompanion);
        Assert.Throws<ArgumentException>(() => new RouteSource(baseDocument, noOverwrite, RouteSourceKind.Markdown, companion));

        var declared = RouteSourceMetadata.Complete("Leaf", ["Route"], false, true);
        var nonAdjacent = RouteSourceTestData.Document(
            ".agents/root/other.overwrite.md",
            RouteSourceForm.OverwriteCompanion);
        Assert.Throws<ArgumentException>(() => new RouteSource(baseDocument, declared, RouteSourceKind.Markdown, nonAdjacent));
    }

    [Fact(DisplayName = "Route source catalogue retains a valid orphan fact and rejects an unmatched paired fact")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CatalogueOverwriteFactsMustMatchSources()
    {
        var source = RouteSourceTestData.Source(".agents/root/leaf.md", RouteSourceKind.Markdown);
        var orphan = new RouteOverwriteFact(
            RouteOverwriteState.Orphan,
            RouteSourceTestData.Document(".agents/root/leaf.overwrite.md", RouteSourceForm.OverwriteCompanion),
            []);
        var catalogue = new RouteSourceCatalogue([source], [orphan]);
        Assert.Same(orphan, catalogue.FindOverwriteByPath(orphan.CanonicalPath));
        Assert.Null(catalogue.FindById(source.Id)[0].OverwritePath);

        var unmatched = new RouteOverwriteFact(
            RouteOverwriteState.Paired,
            RouteSourceTestData.Document(".agents/root/other.overwrite.md", RouteSourceForm.OverwriteCompanion),
            [source.CanonicalPath]);
        Assert.Throws<ArgumentException>(() => new RouteSourceCatalogue([source], [unmatched]));
    }

    [Fact(DisplayName = "Route source rejects an overwrite alias that is its own base path")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void SourceRejectsSelfAliasingOverwrite()
    {
        Assert.Throws<ArgumentException>(() => RouteSourceTestData.Source(
            ".agents/one.md",
            RouteSourceKind.Markdown,
            overwritePath: ".agents/one.md"));
    }
}
