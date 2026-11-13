using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.References.Shared.Comments;

[Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class SourceCommentLinkReaderTests
{
    private const string Target = "../docs/rules.md#choice";

    [Theory(DisplayName = "Both markers use one target word in common comment styles"),
        InlineData("sample.cs", "//", ""), InlineData("sample.cs", "///", ""), InlineData("sample.py", "#", ""), InlineData("sample.sql", "--", ""),
        InlineData("sample.ini", ";", ""), InlineData("sample.vb", "'", ""), InlineData("sample.tex", "%", ""), InlineData("sample.cs", "/*", "*/"),
        InlineData("sample.cs", "/**", "*/"), InlineData("sample.html", "<!--", "-->")]
    public void CommentStyles(string path, string open, string close)
    {
        foreach (var marker in new[] { SourceCommentLinkReader.OpenForgeMarker, SourceCommentLinkReader.RuneMarker })
        {
            var source = $"{open} {marker} {Target} explains this choice {close}";
            var link = Assert.Single(SourceCommentLinkReader.Read(path, source));
            Assert.True(link.IsComment);
            Assert.Equal(Target, link.RawDestination);
            Assert.NotNull(link.DestinationSpan);
            Assert.Equal(Target, source[link.DestinationSpan.Start..link.DestinationSpan.End]);
        }
    }

    [Fact(DisplayName = "Block continuation lines and inline comments retain their target spans")]
    public void BlockLines()
    {
        var source = $"/*\n * {SourceCommentLinkReader.OpenForgeMarker} {Target} explains this\n */\nvar value = 1; // {SourceCommentLinkReader.RuneMarker} ./target.cs#Value reason";
        var links = SourceCommentLinkReader.Read("sample.cs", source);
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(link.RawDestination,
            source[link.DestinationSpan!.Start..link.DestinationSpan.End]));
    }

    [Fact(DisplayName = "Prose mentions and quoted comment examples produce no occurrences")]
    public void Mentions()
    {
        var marker = SourceCommentLinkReader.OpenForgeMarker;
        var source = $"// Mention {marker} {Target}\nvar text = \"// {marker} {Target}\";\nvar raw = \"\"\"\n// {marker} {Target}\n\"\"\";\n# Mention {marker} {Target}";
        Assert.Empty(SourceCommentLinkReader.Read("sample.cs", source));
    }

    [Theory(DisplayName = "Trailing comments use the file syntax without consuming a decrement operator"),
        InlineData("sample.vb", "Dim x = 1 '", false), InlineData("sample.tex", "text %", true),
        InlineData("sample.cs", "i--; //", false)]
    public void TrailingCommentSyntax(string path, string prefix, bool rune)
    {
        var marker = rune ? SourceCommentLinkReader.RuneMarker : SourceCommentLinkReader.OpenForgeMarker;
        var source = $"{prefix} {marker} {Target} explains this choice";
        var link = Assert.Single(SourceCommentLinkReader.Read(path, source));
        Assert.Equal(Target, link.RawDestination);
        Assert.Equal(Target, source[link.DestinationSpan!.Start..link.DestinationSpan.End]);
    }

    [Fact(DisplayName = "Markdown reads HTML comments and native links while excluding code examples")]
    public void Markdown()
    {
        var marker = SourceCommentLinkReader.OpenForgeMarker;
        var source = $"<!-- {marker} {Target} reason -->\n[rule]({Target})\n`<!-- {marker} {Target} -->`\n```cs\n// {marker} {Target}\n<!-- {marker} {Target} -->\n```\n// {marker} {Target}";
        var document = new MarkdownDocumentParser().Parse(source);
        var links = SourceAuthoredLinkReader.Read("guide.md", document);
        Assert.Equal(2, links.Count);
        Assert.Single(links, link => link.IsComment);
        Assert.All(links, link => Assert.Equal(Target, link.RawDestination));
    }

    [Fact(DisplayName = "Generated and vendored sources do not contribute comment links")]
    public void GeneratedSources()
    {
        var source = $"// {SourceCommentLinkReader.OpenForgeMarker} {Target} reason\n";
        var document = new MarkdownDocumentParser().Parse(source);
        Assert.Empty(SourceAuthoredLinkReader.Read($"sample{SourceCommentDiscoveryPolicy.GeneratedSuffix}", document));
        foreach (var directory in SourceCommentDiscoveryPolicy.ExcludedDirectories)
            Assert.Empty(SourceAuthoredLinkReader.Read($"{directory}/sample.cs", document));
    }
}
