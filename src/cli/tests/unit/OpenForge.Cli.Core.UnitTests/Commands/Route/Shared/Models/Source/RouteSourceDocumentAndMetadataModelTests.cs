using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models.Source;

public sealed class RouteSourceDocumentAndMetadataModelTests
{
    [Fact(DisplayName = "Route projections retain neutral forms and complete or unavailable document reads")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DocumentsPreserveReadStateAndNeutralLayerAssociation()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(".agents/root/leaf.md");
        var complete = RouteSourceTestData.Projection(logicalSource);
        var unavailable = RouteSourceTestData.Document(
            ".agents/root/missing.md",
            SourceDocumentForm.Markdown,
            FileReadState.Missing);

        Assert.Equal(SourceDocumentForm.Markdown, logicalSource.Base.Form);
        var read = Assert.IsType<FileReadResult<string>>(complete.BaseRead.Read);
        Assert.Equal(FileReadState.Complete, read.State);
        Assert.Equal("body", read.Value);
        Assert.Equal(FileReadState.Missing, unavailable.ReadState);
        Assert.Null(unavailable.Body);
        Assert.Equal(logicalSource.Base.PhysicalPath, complete.BaseRead.Layer.PhysicalPath);
    }

    [Fact(DisplayName = "Route projections retain Loader, entrypoint, compatibility, Markdown, and Skill intrinsic forms")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IntrinsicFormsRemainNeutralInputsToRouteProjection()
    {
        var loader = RouteSourceTestData.Projection(RouteSourceTestData.LogicalSource(
            ".agents/loader.md",
            "loader",
            SourceDocumentForm.Loader));
        var entrypoint = RouteSourceTestData.Projection(RouteSourceTestData.LogicalSource(
            ".agents/root/_root.md",
            "root",
            SourceDocumentForm.CanonicalEntrypoint));
        var compatibility = RouteSourceTestData.Projection(RouteSourceTestData.LogicalSource(
            ".agents/compat/index.md",
            "compat",
            SourceDocumentForm.IndexEntrypoint));
        var markdown = RouteSourceTestData.Projection(RouteSourceTestData.LogicalSource(
            ".agents/root/leaf.md",
            "root/leaf"));
        var skill = RouteSourceTestData.Projection(RouteSourceTestData.LogicalSource(
            ".agents/skills/native/SKILL.md",
            "skills/native",
            SourceDocumentForm.Skill));

        var loaderSource = Assert.IsType<RouteSource>(loader.Source);
        var entrypointSource = Assert.IsType<RouteSource>(entrypoint.Source);
        var compatibilitySource = Assert.IsType<RouteSource>(compatibility.Source);
        var markdownSource = Assert.IsType<RouteSource>(markdown.Source);
        var skillSource = Assert.IsType<RouteSource>(skill.Source);
        Assert.Equal(SourceDocumentForm.Loader, loaderSource.Base.Form);
        Assert.Equal(SourceDocumentForm.CanonicalEntrypoint, entrypointSource.Base.Form);
        Assert.Equal(SourceDocumentForm.IndexEntrypoint, compatibilitySource.Base.Form);
        Assert.Equal(SourceDocumentForm.Markdown, markdownSource.Base.Form);
        Assert.Equal(SourceDocumentForm.Skill, skillSource.Base.Form);
        Assert.True(compatibilitySource.Metadata.IsCompatibilityEntrypoint);
        Assert.False(skillSource.Metadata.IsCompatibilityEntrypoint);
    }

    [Fact(DisplayName = "Route metadata retains every unavailable state without authored values")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void MetadataStatesRemainDistinct()
    {
        foreach (var state in new[]
        {
            RouteSourceMetadataState.NotApplicable,
            RouteSourceMetadataState.Missing,
            RouteSourceMetadataState.Malformed,
            RouteSourceMetadataState.ReadUnavailable,
        })
        {
            var metadata = RouteSourceMetadata.WithoutValues(state, false, false);

            Assert.Equal(state, metadata.State);
            Assert.Null(metadata.Description);
            Assert.Empty(metadata.Tags);
        }
    }

    [Fact(DisplayName = "Route metadata snapshots complete descriptions and strict tags")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void CompleteMetadataOwnsItsValues()
    {
        var tags = new List<string> { "Route-List", "Évidence2" };
        var metadata = RouteSourceMetadata.Complete("  Exact description  ", tags, true, true);
        tags.Clear();

        Assert.Equal(RouteSourceMetadataState.Complete, metadata.State);
        Assert.Equal("  Exact description  ", metadata.Description);
        Assert.Equal(["Route-List", "Évidence2"], metadata.Tags);
        Assert.True(metadata.IsCompatibilityEntrypoint);
        Assert.True(metadata.IsOverwritePresent);
    }

    [Fact(DisplayName = "Route metadata overwrite presence follows the selected projection layers rather than neutral availability")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void BaseOnlyProjectionSuppressesOverwriteMetadata()
    {
        var logicalSource = RouteSourceTestData.LogicalSource(
            ".agents/root/leaf.md",
            withOverwrite: true);
        var exact = RouteSourceTestData.Projection(
            logicalSource,
            mode: RouteSourceLayerProjection.ExactLayers);
        var baseOnly = RouteSourceTestData.Projection(
            logicalSource,
            mode: RouteSourceLayerProjection.BaseOnly);

        Assert.NotNull(exact.OverwriteRead);
        var exactSource = Assert.IsType<RouteSource>(exact.Source);
        var baseOnlySource = Assert.IsType<RouteSource>(baseOnly.Source);
        Assert.True(exactSource.Metadata.IsOverwritePresent);
        Assert.Null(baseOnly.OverwriteRead);
        Assert.False(baseOnlySource.Metadata.IsOverwritePresent);
    }
}
