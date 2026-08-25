using Markdig;
using Markdig.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownDocumentParserRedTests
{
    [Fact(DisplayName = "Markdown pipeline is cached, precise, and extension-free")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void PipelineIsCachedPreciseAndExtensionFree()
    {
        var first = MarkdownPipelineFactory.Get();
        var second = MarkdownPipelineFactory.Get();
        var heading = Assert.IsType<HeadingBlock>(Assert.Single(Markdig.Markdown.Parse("# Heading\n", first)));

        Assert.Same(first, second);
        Assert.Equal(0, heading.Span.Start);
        Assert.Equal("# Heading".Length - 1, heading.Span.End);
        Assert.Empty(first.Extensions);
    }

    [Fact(DisplayName = "Markdown frontmatter boundaries preserve line endings and unavailable input")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void FrontmatterBoundariesPreserveLfCrLfMissingAndUnterminatedInput()
    {
        AssertCompleteFrontmatter(
            "---\nopen-forge:\n  description: Example\n---\n# Body\n",
            new MarkdownTextSpan(0, 42),
            new MarkdownTextSpan(4, 35),
            43,
            new MarkdownTextSpan(43, 7));
        AssertCompleteFrontmatter(
            "---\r\nopen-forge:\r\n  description: Example\r\n---\r\n# Body\r\n",
            new MarkdownTextSpan(0, 45),
            new MarkdownTextSpan(5, 37),
            47,
            new MarkdownTextSpan(47, 8));

        var missingSource = "# Body\r\n";
        var missing = new MarkdownDocumentParser().Parse(missingSource);
        Assert.Equal(missingSource, missing.Source);
        Assert.Equal(MarkdownFrontmatterState.Missing, missing.Frontmatter.State);
        Assert.Null(missing.Frontmatter.BlockSpan);
        Assert.Null(missing.Frontmatter.YamlSpan);
        Assert.Equal(0, missing.Frontmatter.BodyStart);
        Assert.Equal(new MarkdownTextSpan(0, 8), missing.BodySpan);

        var unterminatedSource = "---\nopen-forge:\n  tags: [One]\n# Body\n";
        var unterminated = new MarkdownDocumentParser().Parse(unterminatedSource);
        Assert.Equal(unterminatedSource, unterminated.Source);
        Assert.Equal(MarkdownFrontmatterState.Unavailable, unterminated.Frontmatter.State);
        Assert.Null(unterminated.Frontmatter.BlockSpan);
        Assert.Null(unterminated.Frontmatter.YamlSpan);
        Assert.Null(unterminated.Frontmatter.BodyStart);
        Assert.Null(unterminated.BodySpan);
    }

    private static void AssertCompleteFrontmatter(
        string source,
        MarkdownTextSpan blockSpan,
        MarkdownTextSpan yamlSpan,
        int bodyStart,
        MarkdownTextSpan bodySpan)
    {
        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal(source, facts.Source);
        Assert.Equal(MarkdownFrontmatterState.Complete, facts.Frontmatter.State);
        Assert.Equal(blockSpan, facts.Frontmatter.BlockSpan);
        Assert.Equal(yamlSpan, facts.Frontmatter.YamlSpan);
        Assert.Equal(bodyStart, facts.Frontmatter.BodyStart);
        Assert.Equal(bodySpan, facts.BodySpan);
    }

    [Fact(DisplayName = "Markdown headings retain levels, source forms, complete spans, and canonicality")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void AtxAndSetextHeadingsPreserveLevelFormAndCanonicality()
    {
        const string source =
            "# One\n"
            + "## Two\n"
            + "### Three\n"
            + "#### Four\n"
            + "##### Five\n"
            + "###### Six\n"
            + "Setext One\n"
            + "==========\n"
            + "Setext Two\n"
            + "----------\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal(
            ["One", "Two", "Three", "Four", "Five", "Six", "Setext One", "Setext Two"],
            facts.Headings.Select(heading => heading.VisibleText));
        Assert.Equal([1, 2, 3, 4, 5, 6, 1, 2], facts.Headings.Select(heading => heading.Level));
        Assert.Equal(
            [
                MarkdownHeadingForm.Atx,
                MarkdownHeadingForm.Atx,
                MarkdownHeadingForm.Atx,
                MarkdownHeadingForm.Atx,
                MarkdownHeadingForm.Atx,
                MarkdownHeadingForm.Atx,
                MarkdownHeadingForm.Setext,
                MarkdownHeadingForm.Setext,
            ],
            facts.Headings.Select(heading => heading.Form));
        Assert.Equal(
            [true, true, true, true, true, true, false, false],
            facts.Headings.Select(heading => heading.IsCanonical));

        Assert.Equal(new MarkdownTextSpan(0, "# One".Length), facts.Headings[0].Span);
        Assert.Equal(
            new MarkdownTextSpan(
                source.IndexOf("Setext One", StringComparison.Ordinal),
                source.IndexOf("==========", StringComparison.Ordinal)
                    + "==========".Length
                    - source.IndexOf("Setext One", StringComparison.Ordinal)),
            facts.Headings[6].Span);
        Assert.Equal(
            new MarkdownTextSpan(
                source.IndexOf("Setext Two", StringComparison.Ordinal),
                source.IndexOf("----------", StringComparison.Ordinal)
                    + "----------".Length
                    - source.IndexOf("Setext Two", StringComparison.Ordinal)),
            facts.Headings[7].Span);
    }

    [Fact(DisplayName = "Markdown heading text keeps reader-visible inline content only")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void VisibleHeadingTextUsesOnlyReaderVisibleInlineContent()
    {
        const string source =
            "#   **Cafe\u0301**\u00A0 `code` [label](https://example.invalid/destination \"title\") "
            + "![alt](image.png \"image title\") &amp; <br>\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        var heading = Assert.Single(facts.Headings);
        Assert.Equal("Cafe\u0301 code label alt &", heading.VisibleText);
        Assert.DoesNotContain("destination", heading.VisibleText, StringComparison.Ordinal);
        Assert.DoesNotContain("title", heading.VisibleText, StringComparison.Ordinal);
        Assert.DoesNotContain("br", heading.VisibleText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Markdown sections and visible literal spans stay within exact document boundaries")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void SectionsAndVisibleLiteralSpansPreserveDocumentBoundaries()
    {
        const string source =
            "---\n"
            + "open-forge:\n"
            + "  tags: [One]\n"
            + "---\n"
            + "# Parent\n"
            + "Parent literal\n\n"
            + "## Child\n"
            + "[linked label](https://example.invalid/destination)\n\n"
            + "### Grandchild\n"
            + "Grandchild literal\n\n"
            + "```text\n"
            + "# hidden\n"
            + "```\n\n"
            + "<div>#raw-html</div>\n\n"
            + "# Sibling\n"
            + "Sibling literal\n";

        var facts = new MarkdownDocumentParser().Parse(source);
        var bodyStart = source.IndexOf("# Parent", StringComparison.Ordinal);
        var siblingStart = source.IndexOf("# Sibling", StringComparison.Ordinal);
        var childStart = source.IndexOf("## Child", StringComparison.Ordinal);
        var grandchildStart = source.IndexOf("### Grandchild", StringComparison.Ordinal);

        Assert.Equal(new MarkdownTextSpan(bodyStart, source.Length - bodyStart), facts.BodySpan);
        Assert.Equal(
            new MarkdownTextSpan(bodyStart, siblingStart - bodyStart),
            facts.Sections[0].Span);
        Assert.Equal(
            new MarkdownTextSpan(childStart, siblingStart - childStart),
            facts.Sections[1].Span);
        Assert.Equal(
            new MarkdownTextSpan(grandchildStart, siblingStart - grandchildStart),
            facts.Sections[2].Span);
        Assert.Equal(
            new MarkdownTextSpan(siblingStart, source.Length - siblingStart),
            facts.Sections[3].Span);

        Assert.All(
            facts.VisibleText,
            fact =>
            {
                Assert.InRange(fact.Span.Start, bodyStart, source.Length);
                Assert.InRange(fact.Span.End, fact.Span.Start, source.Length);
                Assert.DoesNotContain("open-forge", source[fact.Span.Start..fact.Span.End], StringComparison.Ordinal);
                Assert.DoesNotContain("destination", source[fact.Span.Start..fact.Span.End], StringComparison.Ordinal);
                Assert.DoesNotContain("# hidden", source[fact.Span.Start..fact.Span.End], StringComparison.Ordinal);
                Assert.DoesNotContain("#raw-html", source[fact.Span.Start..fact.Span.End], StringComparison.Ordinal);
            });
        Assert.Contains(
            facts.VisibleText,
            fact => source[fact.Span.Start..fact.Span.End].Contains("linked label", StringComparison.Ordinal));
        Assert.Contains(
            facts.OpaqueSpans,
            fact => source[fact.Span.Start..fact.Span.End].Contains("# hidden", StringComparison.Ordinal));
        Assert.Contains(
            facts.OpaqueSpans,
            fact => source[fact.Span.Start..fact.Span.End].Contains("#raw-html", StringComparison.Ordinal));
    }
}
