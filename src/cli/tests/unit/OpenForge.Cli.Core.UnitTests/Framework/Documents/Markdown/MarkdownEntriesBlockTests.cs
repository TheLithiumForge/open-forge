using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownEntriesBlockTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Entries owns only the first top-level dash-space list inside its section")]
    [InlineData("## Entries\n\n- one\n- two\n", "- one\n- two\n", "\n")]
    [InlineData("## Entries\n\nBefore.\n\n- one\n", "- one\n", "\n")]
    [InlineData("## Entries\n\n- one\nAfter.\n", "- one\n", "\n")]
    [InlineData("## Entries\n\nBefore.\n\n- one\n\nAfter.\n", "- one\n", "\n")]
    [InlineData("## Entries\n\n\n- one\n\n\n", "- one\n", "\n")]
    [InlineData("## Entries\n\n- none - No entries - #Empty\n", "- none - No entries - #Empty\n", "\n")]
    [InlineData("## Entries\n\n- one\n\n- authored\n", "- one\n", "\n")]
    [InlineData("## Entries\r\n\r\nBefore.\r\n- one\r\nAfter.\r\n", "- one\r\n", "\r\n")]
    [InlineData("## Entries\n\n### Detail\n\n- one\n", "- one\n", "\n")]
    [InlineData("## Entries\n\n- one\n\n## Other\n- authored\n", "- one\n", "\n")]
    [InlineData("## Entries\n\n- one", "- one", "\n")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void FirstListBoundary(string source, string expected, string lineEnding)
    {
        var document = new MarkdownDocumentParser().Parse(source);
        var block = Assert.IsType<MarkdownEntriesBlock>(document.GeneratedRegion.EntriesBlock);

        Assert.True(block.Exists);
        Assert.Equal(expected, source[block.Span.Start..block.Span.End]);
        Assert.Equal(source.IndexOf(expected, StringComparison.Ordinal), block.Span.Start);
        Assert.Equal(lineEnding, block.LineEnding);
        Assert.Equal(block.Span, document.GeneratedRegion.OmissionSpan);
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "An Entries section without a list has an empty insertion point before authored text")]
    [InlineData("## Entries", 10)]
    [InlineData("## Entries\n\nAuthored.\n", 12)]
    [InlineData("## Entries\r\n\r\nAuthored.\r\n", 14)]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void MissingListHasInsertionPoint(string source, int expectedStart)
    {
        var block = Assert.IsType<MarkdownEntriesBlock>(new MarkdownDocumentParser().Parse(source).GeneratedRegion.EntriesBlock);

        Assert.False(block.Exists);
        Assert.Equal(expectedStart, block.Span.Start);
        Assert.Equal(0, block.Span.Length);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Loader and generated-entry readers ignore prose and later authored lists")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ReadersUseTheSameFirstList()
    {
        const string source = "# Loader\n\n## Entries\n\nBefore.\n\n- [Root](root/_root.md) - #Root\n\nAfter.\n\n- [Authored](authored/_authored.md) - #Authored\n";

        var loader = SourceLoaderEntriesParser.Parse(source);
        var generated = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(source));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, loader.State);
        Assert.Equal(".agents/root/_root.md", Assert.Single(loader.Destinations).CanonicalPath);
        Assert.Equal(SourceGeneratedEntriesState.Complete, generated.State);
        Assert.Equal("root/_root.md", Assert.Single(generated.Entries).Destination);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "First generated list spans blank-separated rows and preserves authored exterior")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void FirstListSpansBlankSeparatedRowsAndPreservesExterior(string lineEnding)
    {
        var prefix = "# Loader" + lineEnding + lineEnding + "## Entries" + lineEnding + lineEnding + "Before." + lineEnding + lineEnding;
        var generated = "- [Root](root/_root.md) - #Root" + lineEnding + lineEnding
            + "- [Child](child/_child.md) - #Child" + lineEnding;
        var suffix = lineEnding + "After." + lineEnding + lineEnding
            + "- [Authored](authored/_authored.md) - #Authored" + lineEnding;
        var source = prefix + generated + suffix;
        var document = new MarkdownDocumentParser().Parse(source);
        var block = Assert.IsType<MarkdownEntriesBlock>(document.GeneratedRegion.EntriesBlock);

        Assert.Equal(generated, source[block.Span.Start..block.Span.End]);
        Assert.Equal(prefix, source[..block.Span.Start]);
        Assert.Equal(suffix, source[block.Span.End..]);

        var loader = SourceLoaderEntriesParser.Parse(source);
        var generatedEntries = SourceGeneratedEntriesParser.Parse(document);
        Assert.Equal(SourceLoaderEntriesParseState.Valid, loader.State);
        Assert.Equal(2, loader.Destinations.Count);
        Assert.Equal(SourceGeneratedEntriesState.Complete, generatedEntries.State);
        Assert.Equal(2, generatedEntries.Entries.Count);
        Assert.Equal(["root/_root.md", "child/_child.md"], generatedEntries.Entries.Select(entry => entry.Destination));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "A blank-separated plain authored row remains outside generated Entries")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void BlankSeparatedPlainAuthoredRowPreservesExterior(string lineEnding)
    {
        var prefix = "# Loader" + lineEnding + lineEnding + "## Entries" + lineEnding + lineEnding;
        var generated = "- [Root](root/_root.md) - #Root" + lineEnding;
        var authored = "- authored prose" + lineEnding;
        var source = prefix + generated + lineEnding + authored;
        var document = new MarkdownDocumentParser().Parse(source);
        var block = Assert.IsType<MarkdownEntriesBlock>(document.GeneratedRegion.EntriesBlock);

        Assert.Equal(generated, source[block.Span.Start..block.Span.End]);
        Assert.Equal(prefix, source[..block.Span.Start]);
        Assert.Equal(lineEnding + authored, source[block.Span.End..]);

        var loader = SourceLoaderEntriesParser.Parse(source);
        var generatedEntries = SourceGeneratedEntriesParser.Parse(document);
        Assert.Equal(SourceLoaderEntriesParseState.Valid, loader.State);
        Assert.Equal([".agents/root/_root.md"], loader.Destinations.Select(destination => destination.CanonicalPath));
        Assert.Equal(SourceGeneratedEntriesState.Complete, generatedEntries.State);
        Assert.Equal(["root/_root.md"], generatedEntries.Entries.Select(entry => entry.Destination));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Prose-only Entries sections establish no generated declarations")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void ProseOnlyHasNoDeclarations()
    {
        const string source = "# Loader\n\n## Entries\nAuthored prose.\n";
        var loader = SourceLoaderEntriesParser.Parse(source);
        var generated = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(source));

        Assert.Equal(SourceLoaderEntriesParseState.Valid, loader.State);
        Assert.Empty(loader.Destinations);
        Assert.Equal(SourceGeneratedEntriesState.Complete, generated.State);
        Assert.Empty(generated.Entries);
    }
}
