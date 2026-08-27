using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Documents.Markdown;

public sealed class MarkdownDocumentParserRedTests
{
    [Fact(DisplayName = "Markdown pipeline is cached and combines precise locations with GitHub identifiers")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void PipelineIsCachedPreciseAndUsesGitHubIdentifiers()
    {
        var first = MarkdownPipelineFactory.Get();
        var second = MarkdownPipelineFactory.Get();
        var heading = Assert.IsType<HeadingBlock>(Assert.Single(Markdig.Markdown.Parse("# Heading\n", first)));

        Assert.Same(first, second);
        Assert.Equal(0, heading.Span.Start);
        Assert.Equal("# Heading".Length - 1, heading.Span.End);
        Assert.IsType<AutoIdentifierExtension>(Assert.Single(first.Extensions));
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
        Assert.Equal(
            ["one", "two", "three", "four", "five", "six", null, null],
            facts.Headings.Select(heading => heading.FragmentIdentifier));

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

    [Fact(DisplayName = "Markdown GitHub heading identifiers preserve Unicode and deterministic duplicate suffixes")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void GitHubHeadingIdentifiersPreserveUnicodeAndDuplicateSuffixes()
    {
        const string source =
            "# Café 工作\n"
            + "# Café 工作\n"
            + "# Visible <span>unsupported</span>\n"
            + "Setext\n"
            + "======\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal("café-工作", facts.Headings[0].FragmentIdentifier);
        Assert.Equal("café-工作-1", facts.Headings[1].FragmentIdentifier);
        Assert.Null(facts.Headings[2].FragmentIdentifier);
        Assert.Null(facts.Headings[3].FragmentIdentifier);
    }

    [Fact(DisplayName = "Markdown link facts preserve inline, reference-definition, and explicit-autolink spans")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void LinksPreservePinnedMarkdigFormsDestinationsAndSpans()
    {
        const string inline = "[inline](folder/target%20one.md#Frag)";
        const string reference = "[reference][ref]";
        const string autolink = "<https://example.invalid/path?q=one>";
        const string referenceDestination = "ref target.md#Part";
        const string source =
            inline + "\n"
            + reference + "\n"
            + autolink + "\n"
            + "![image](image.png)\n"
            + "<a href=\"raw.html\">raw</a>\n"
            + "`[code](code.md)`\n"
            + "https://example.invalid/bare\n\n"
            + "[ref]: <" + referenceDestination + ">\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal(
            [MarkdownLinkForm.Inline, MarkdownLinkForm.Reference, MarkdownLinkForm.Autolink],
            facts.Links.Select(link => link.Form));
        Assert.Equal(
            ["folder/target%20one.md#Frag", referenceDestination, "https://example.invalid/path?q=one"],
            facts.Links.Select(link => link.RawDestination));
        Assert.Equal(SpanOf(source, inline), facts.Links[0].Span);
        Assert.Equal(SpanOf(source, "folder/target%20one.md#Frag"), facts.Links[0].DestinationSpan);
        Assert.Equal(SpanOf(source, reference), facts.Links[1].Span);
        Assert.Equal(SpanOf(source, $"<{referenceDestination}>"), facts.Links[1].DestinationSpan);
        Assert.Equal(SpanOf(source, autolink), facts.Links[2].Span);
        Assert.Null(facts.Links[2].DestinationSpan);
    }

    [Fact(DisplayName = "Markdown generated-region facts require one final Entries section and ordered marker pair")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void GeneratedRegionRequiresStrictFinalEntriesBoundary()
    {
        const string generatedLink = "- [Generated](generated.md)";
        const string source =
            "# Document\n\n"
            + "## Entries\n\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + generatedLink + "\n"
            + "<!-- open-forge:generated-index:end -->\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal(MarkdownGeneratedRegionState.Complete, facts.GeneratedRegion.State);
        var contentSpan = Assert.IsType<MarkdownTextSpan>(facts.GeneratedRegion.ContentSpan);
        Assert.Contains(generatedLink, source[contentSpan.Start..contentSpan.End], StringComparison.Ordinal);
        var omissionSpan = Assert.IsType<MarkdownTextSpan>(facts.GeneratedRegion.OmissionSpan);
        Assert.Equal(
            generatedLink + "\n",
            source[omissionSpan.Start..omissionSpan.End]);
        var regionSpan = Assert.IsType<MarkdownTextSpan>(facts.GeneratedRegion.RegionSpan);
        Assert.StartsWith("<!-- open-forge:generated-index:start -->", source[regionSpan.Start..regionSpan.End], StringComparison.Ordinal);
        Assert.EndsWith("<!-- open-forge:generated-index:end -->", source[regionSpan.Start..regionSpan.End], StringComparison.Ordinal);

        var absent = new MarkdownDocumentParser().Parse("# Document\n");
        Assert.Equal(MarkdownGeneratedRegionState.Absent, absent.GeneratedRegion.State);

        var malformed = new[]
        {
            source + "## Later\n",
            source.Replace(
                "<!-- open-forge:generated-index:end -->",
                "<!-- open-forge:generated-index:start -->\n<!-- open-forge:generated-index:end -->",
                StringComparison.Ordinal),
            source.Replace(
                "<!-- open-forge:generated-index:end -->\n",
                "<!-- open-forge:generated-index:end -->\nauthored prose\n",
                StringComparison.Ordinal),
        };
        Assert.All(
            malformed,
            value => Assert.Equal(
                MarkdownGeneratedRegionState.Invalid,
                new MarkdownDocumentParser().Parse(value).GeneratedRegion.State));
    }

    [Fact(DisplayName = "Markdown generated-region grammar distinguishes absent markers from unavailable candidates")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void GeneratedRegionCandidateStatesRemainDistinct()
    {
        const string start = "<!-- open-forge:generated-index:start -->";
        const string end = "<!-- open-forge:generated-index:end -->";

        Assert.Equal(
            MarkdownGeneratedRegionState.Absent,
            new MarkdownDocumentParser().Parse("## Entries\n").GeneratedRegion.State);
        Assert.Equal(
            MarkdownGeneratedRegionState.Invalid,
            new MarkdownDocumentParser().Parse("<!-- open-forge:generated-index:bogus -->\n").GeneratedRegion.State);
        foreach (var sourceWithMarkerPrefix in new[]
        {
            "---\nopen-forge: \"open-forge:generated-index:bogus\"\n---\n# Document\n",
            "[unused][ref]\n\n[ref]: <open-forge:generated-index:bogus>\n",
            "![image](open-forge:generated-index:bogus)\n",
        })
        {
            Assert.Equal(
                MarkdownGeneratedRegionState.Invalid,
                new MarkdownDocumentParser().Parse(sourceWithMarkerPrefix).GeneratedRegion.State);
        }
        Assert.Equal(
            MarkdownGeneratedRegionState.Invalid,
            new MarkdownDocumentParser().Parse($"## Entries   \n{start}\nbody\n{end}\n").GeneratedRegion.State);

        foreach (var markerPair in new[]
        {
            $"  {start}\nbody\n{end}",
            $"{start} trailing\nbody\n{end}",
            $"{start}\nbody\n{end} trailing",
        })
        {
            Assert.Equal(
                MarkdownGeneratedRegionState.Invalid,
                new MarkdownDocumentParser().Parse($"## Entries\n{markerPair}\n").GeneratedRegion.State);
        }

        Assert.Equal(
            MarkdownGeneratedRegionState.Absent,
            new MarkdownDocumentParser().Parse($"`{start}`\n```\n{end}\n```\n").GeneratedRegion.State);

        foreach (var lineEnding in new[] { "\n", "\r\n", "\r" })
        {
            var source = string.Join(
                lineEnding,
                "## Entries",
                "",
                start,
                "body",
                end,
                "");
            Assert.Equal(
                MarkdownGeneratedRegionState.Complete,
                new MarkdownDocumentParser().Parse(source).GeneratedRegion.State);
        }
    }

    [Fact(DisplayName = "Markdown generated-region facts classify malformed marker syntax as invalid")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void MalformedGeneratedRegionIsInvalid()
    {
        var facts = new MarkdownDocumentParser().Parse(
            "## Entries\n<!-- open-forge:generated-index:bogus -->\n");

        Assert.Equal(MarkdownGeneratedRegionState.Invalid, facts.GeneratedRegion.State);
    }

    [Fact(DisplayName = "Markdown generated-region facts retain authored content before the generated marker")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void GeneratedRegionAllowsAuthoredContentBeforeMarker()
    {
        const string start = "<!-- open-forge:generated-index:start -->";
        const string end = "<!-- open-forge:generated-index:end -->";
        const string authored = "Authored route notes remain part of the document.\n";
        var source = $"## Entries\n{authored}{start}\n- generated\n{end}\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal(MarkdownGeneratedRegionState.Complete, facts.GeneratedRegion.State);
    }

    [Theory(DisplayName = "Markdown generated-region facts reject horizontal whitespace after the end marker")]
    [InlineData(" ")]
    [InlineData("\t")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void GeneratedRegionRejectsHorizontalWhitespaceAfterEndMarker(string trailing)
    {
        const string sourcePrefix =
            "## Entries\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "body\n"
            + "<!-- open-forge:generated-index:end -->\n";

        var facts = new MarkdownDocumentParser().Parse(sourcePrefix + trailing);

        Assert.Equal(MarkdownGeneratedRegionState.Invalid, facts.GeneratedRegion.State);
    }

    [Fact(DisplayName = "Markdown generated-region facts keep an unparseable body boundary unavailable")]
    [Trait("Feature", "markdown-documents"), Trait("Evidence", "Unit")]
    public void GeneratedRegionRemainsUnavailableWhenFrontmatterIsUnterminated()
    {
        const string source = "---\nopen-forge:\n  tags: [One]\n# Body\n";

        var facts = new MarkdownDocumentParser().Parse(source);

        Assert.Equal(MarkdownGeneratedRegionState.Unavailable, facts.GeneratedRegion.State);
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

    private static MarkdownTextSpan SpanOf(string source, string value)
    {
        var start = source.IndexOf(value, StringComparison.Ordinal);
        Assert.True(start >= 0);
        return new MarkdownTextSpan(start, value.Length);
    }
}
