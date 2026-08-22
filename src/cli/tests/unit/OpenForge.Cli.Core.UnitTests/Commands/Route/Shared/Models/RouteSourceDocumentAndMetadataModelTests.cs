using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Shared.Models;

public sealed class RouteSourceDocumentAndMetadataModelTests
{
    [Fact(DisplayName = "Route source document retains a complete body and an unavailable document retains no body")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void DocumentsPreserveReadStateAndBodyShape()
    {
        var complete = RouteSourceTestData.Document(
            ".agents/root/leaf.md",
            RouteSourceForm.Markdown,
            FileReadState.Complete,
            "Exact body");
        var unavailable = RouteSourceTestData.Document(
            ".agents/root/missing.md",
            RouteSourceForm.Markdown,
            FileReadState.Missing);

        Assert.Equal(FileReadState.Complete, complete.ReadState);
        Assert.Equal("Exact body", complete.Body);
        Assert.Equal(FileReadState.Missing, unavailable.ReadState);
        Assert.Null(unavailable.Body);
        Assert.EndsWith(".agents" + Path.DirectorySeparatorChar + "root" + Path.DirectorySeparatorChar + "leaf.md", complete.PhysicalPath);
    }

    [Fact(DisplayName = "Route source models retain intrinsic Loader, entrypoint, compatibility, Markdown, and Skill forms")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    public void IntrinsicFormsAndKindsAreNotCommandPolicy()
    {
        var loader = RouteSourceTestData.Source(".agents/loader.md", RouteSourceKind.Loader);
        var entrypoint = RouteSourceTestData.Source(".agents/root/_root.md", RouteSourceKind.Entrypoint);
        var compatibility = RouteSourceTestData.Source(
            ".agents/compat/index.md",
            RouteSourceKind.Entrypoint,
            RouteSourceForm.IndexEntrypoint);
        var markdown = RouteSourceTestData.Source(".agents/root/leaf.md", RouteSourceKind.Markdown);
        var native = RouteSourceTestData.Source(".agents/skills/native/SKILL.md", RouteSourceKind.Native);

        Assert.Equal(("loader", RouteSourceForm.Loader, RouteSourceKind.Loader), (loader.Id, loader.Base.Form, loader.Kind));
        Assert.Equal(("root", RouteSourceForm.CanonicalEntrypoint, RouteSourceKind.Entrypoint), (entrypoint.Id, entrypoint.Base.Form, entrypoint.Kind));
        Assert.Equal(("compat", RouteSourceForm.IndexEntrypoint, RouteSourceKind.Entrypoint), (compatibility.Id, compatibility.Base.Form, compatibility.Kind));
        Assert.Equal(("root/leaf", RouteSourceForm.Markdown, RouteSourceKind.Markdown), (markdown.Id, markdown.Base.Form, markdown.Kind));
        Assert.Equal(("skills/native", RouteSourceForm.Skill, RouteSourceKind.Native), (native.Id, native.Base.Form, native.Kind));
        Assert.True(compatibility.Metadata.IsCompatibilityEntrypoint);
        Assert.False(native.Metadata.IsCompatibilityEntrypoint);
    }

    [Fact(DisplayName = "Route source metadata retains every unavailable state without authored values")]
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

    [Fact(DisplayName = "Route source metadata snapshots complete descriptions and strict tags")]
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
}
