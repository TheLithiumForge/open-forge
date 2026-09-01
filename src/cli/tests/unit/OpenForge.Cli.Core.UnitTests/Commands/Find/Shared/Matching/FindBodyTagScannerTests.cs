using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Matching;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Matching;

public sealed class FindBodyTagScannerTests
{
    [Fact(DisplayName = "Find body tags use whole Unicode token grammar and preserve authored locations")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void VisibleBareTagsUseWholeUnicodeTokenGrammar()
    {
        const string source =
            "#Alpha #éclair #工作\n"
            + "#A1 #a-b #alpha_beta ##Double\n"
            + "#-bad #bad- #a--b #9bad #good#\n";
        var document = Document(source, [new MarkdownTextSpan(0, source.Length)]);

        var facts = new FindBodyTagScanner().Scan(new FindBodyTagInput(document));

        var expected = new[]
        {
            (Value: "Alpha", Line: 1, Column: 2),
            (Value: "éclair", Line: 1, Column: 9),
            (Value: "工作", Line: 1, Column: 17),
            (Value: "A1", Line: 2, Column: 2),
            (Value: "a-b", Line: 2, Column: 6),
        };
        Assert.Equal(FindBodyTagAvailability.Complete, facts.Availability);
        Assert.Equal(expected.Select(value => value.Value), facts.Occurrences.Select(occurrence => occurrence.Authored));
        Assert.Equal(expected.Length, facts.Occurrences.Count);
        for (var index = 0; index < expected.Length; index++)
        {
            var marker = $"#{expected[index].Value}";
            var markerStart = source.IndexOf(marker, StringComparison.Ordinal);
            var authoredStart = markerStart + 1;
            Assert.Equal(
                ExpectedLocation(
                    source,
                    authoredStart,
                    expected[index].Value.Length,
                    expected[index].Line,
                    expected[index].Column),
                facts.Occurrences[index].Location);
        }
    }

    [Fact(DisplayName = "Find body tags exclude code, HTML, destinations, escapes, entities, and generated entries")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void CodeHtmlDestinationsEscapesEntitiesAndGeneratedEntriesDoNotMatch()
    {
        const string source =
            "Before #visible [#label](https://example.test/#destination) \\#escaped &#35;entity\n"
            + "```text\n"
            + "#code\n"
            + "```\n"
            + "<div>#html</div>\n"
            + "<!-- open-forge:generated-index:start -->\n"
            + "- generated #hidden\n"
            + "<!-- open-forge:generated-index:end -->\n"
            + "After #tail\n";
        var document = Document(
            source,
            [
                SpanOf(source, "Before #visible "),
                SpanOf(source, "#label"),
                SpanOf(source, "\\#escaped &#35;entity"),
                SpanOf(source, "- generated #hidden"),
                SpanOf(source, "After #tail"),
            ],
            [
                SpanOf(source, "```text\n#code\n```"),
                SpanOf(source, "<div>#html</div>"),
                SpanOf(source, "<!-- open-forge:generated-index:start -->"),
                SpanOf(source, "<!-- open-forge:generated-index:end -->"),
            ]);

        var facts = new FindBodyTagScanner().Scan(new FindBodyTagInput(document));

        Assert.Equal(FindBodyTagAvailability.Complete, facts.Availability);
        Assert.Equal(["visible", "label", "tail"], facts.Occurrences.Select(occurrence => occurrence.Authored));
        Assert.Equal(
            ExpectedLocation(source, source.IndexOf("#visible", StringComparison.Ordinal) + 1, 7, 1, 9),
            facts.Occurrences[0].Location);
        Assert.Equal(
            ExpectedLocation(source, source.IndexOf("#label", StringComparison.Ordinal) + 1, 5, 1, 19),
            facts.Occurrences[1].Location);
        Assert.Equal(
            ExpectedLocation(source, source.IndexOf("#tail", StringComparison.Ordinal) + 1, 4, 9, 8),
            facts.Occurrences[2].Location);
    }

    private static MarkdownDocumentFacts Document(
        string source,
        IEnumerable<MarkdownTextSpan> visibleText,
        IEnumerable<MarkdownTextSpan>? opaqueSpans = null)
    {
        return new MarkdownDocumentFacts(
            source,
            new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, 0),
            new MarkdownTextSpan(0, source.Length),
            [],
            [],
            visibleText.Select(span => new MarkdownVisibleTextFact(span)),
            (opaqueSpans ?? []).Select(span => new MarkdownOpaqueSpan(span)),
            [],
            MarkdownGeneratedRegionFact.Absent());
    }

    private static MarkdownTextSpan SpanOf(string source, string value)
    {
        var start = source.IndexOf(value, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"The body-tag fixture does not contain '{value}'.");
        }

        return new MarkdownTextSpan(start, value.Length);
    }

    private static SourceLocation ExpectedLocation(
        string source,
        int start,
        int length,
        int line,
        int column)
    {
        return new SourceLocation(
            line,
            column,
            Encoding.UTF8.GetByteCount(source[..start]),
            Encoding.UTF8.GetByteCount(source.Substring(start, length)));
    }
}
