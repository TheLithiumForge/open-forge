using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownEntriesMigrationTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Retired guard lines remain readable only inside a heading-owned Entries section")]
    [InlineData("<!-- open-forge:generated-index:start -->", "<!-- open-forge:generated-index:end -->")]
    [InlineData("  <!-- open-forge:generated-index:start -->  ", "\t<!-- open-forge:generated-index:end -->")]
    [InlineData("<!-- open-forge:generated-index:end -->", "<!-- open-forge:generated-index:start -->")]
    [InlineData("<!-- open-forge:generated-index:start -->", "")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void ReadersIgnoreRetiredGuardLines(string before, string after)
    {
        var source = $"# Loader\n\n## Entries\n\n{before}\n\n- [Root](root/_root.md) - #Root\n\n{after}\n";
        var document = new MarkdownDocumentParser().Parse(source);
        Assert.Equal(MarkdownGeneratedRegionState.Complete, document.GeneratedRegion.State);
        var loader = SourceLoaderEntriesParser.Parse(source);
        Assert.Equal(SourceLoaderEntriesParseState.Valid, loader.State);
        Assert.Equal(".agents/root/_root.md", Assert.Single(loader.Destinations).CanonicalPath);
        var entries = SourceGeneratedEntriesParser.Parse(document);
        Assert.Equal(SourceGeneratedEntriesState.Complete, entries.State);
        Assert.Equal("root/_root.md", Assert.Single(entries.Entries).Destination);
        var noHeading = new MarkdownDocumentParser().Parse($"{before}\n- [Root](root/_root.md) - #Root\n{after}\n");
        Assert.Equal(MarkdownGeneratedRegionState.Absent, noHeading.GeneratedRegion.State);
    }
}
