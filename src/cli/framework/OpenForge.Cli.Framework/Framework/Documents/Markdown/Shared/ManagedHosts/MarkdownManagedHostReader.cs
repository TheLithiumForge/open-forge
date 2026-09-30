using System.Text;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.ManagedHosts;

internal static class MarkdownManagedHostReader
{
    private const string HeadingText = "Open Forge";
    private const string FooterText = "End of Open Forge managed section.";
    private const string LegacyStart = "<!-- open-forge:start -->";
    private const string LegacyEnd = "<!-- open-forge:end -->";
    private const string InvalidBoundary = "The managed host contains an incomplete, duplicate, reversed, or mixed Open Forge boundary.";

    internal static MarkdownManagedHostFact Read(string source, MarkdownDocument document, int parseStart)
    {
        var openings = new List<Block>();
        var footers = new List<Block>();
        var legacyStarts = new List<Block>();
        var legacyEnds = new List<Block>();

        foreach (var block in document)
        {
            if (block is HeadingBlock { Level: 1, IsSetext: false } heading
                && IsLiteral(heading.Inline, HeadingText))
            {
                openings.Add(block);
            }
            else if (block is ParagraphBlock { Inline.FirstChild: EmphasisInline { DelimiterCount: 2 } emphasis }
                && emphasis.NextSibling is null
                && IsFooterText(emphasis))
            {
                footers.Add(block);
            }
            else if (block is HtmlBlock { Type: HtmlBlockType.Comment } html && html.Lines.Count == 1)
            {
                var text = html.Lines.Lines[0].Slice.ToString().Trim();
                if (text == LegacyStart)
                {
                    legacyStarts.Add(block);
                }
                else if (text == LegacyEnd)
                {
                    legacyEnds.Add(block);
                }
            }
        }

        if (legacyStarts.Count != 0 || legacyEnds.Count != 0)
        {
            if (legacyStarts.Count != 1 || legacyEnds.Count != 1
                || legacyStarts[0].Span.Start >= legacyEnds[0].Span.Start
                || footers.Count != 0
                || openings.Any(heading => heading.Span.Start < legacyStarts[0].Span.Start
                    || heading.Span.Start > legacyEnds[0].Span.End))
            {
                return MarkdownManagedHostFact.Invalid(InvalidBoundary);
            }

            return CreatePresent(source, legacyStarts[0], legacyEnds[0], parseStart, isLegacy: true);
        }

        if (openings.Count == 0 && footers.Count == 0)
        {
            return MarkdownManagedHostFact.Absent();
        }

        if (openings.Count != 1 || footers.Count != 1
            || openings[0].Span.Start >= footers[0].Span.Start)
        {
            return MarkdownManagedHostFact.Invalid(InvalidBoundary);
        }

        if (document.Any(block => block is HeadingBlock { Level: 1 }
            && block.Span.Start > openings[0].Span.Start
            && block.Span.Start < footers[0].Span.Start))
        {
            return MarkdownManagedHostFact.Invalid(
                "The managed host contains another level-1 heading before its closing boundary.");
        }

        return CreatePresent(source, openings[0], footers[0], parseStart, isLegacy: false);
    }

    private static bool IsLiteral(ContainerInline? inline, string text)
        => inline?.FirstChild is LiteralInline literal
            && literal.NextSibling is null
            && literal.Content.ToString() == text;

    private static bool IsFooterText(EmphasisInline emphasis)
    {
        var text = new StringBuilder();
        for (var child = emphasis.FirstChild; child is not null; child = child.NextSibling)
        {
            if (child is LiteralInline literal)
            {
                text.Append(literal.Content.ToString());
            }
            else if (child is LineBreakInline { IsHard: false })
            {
                text.Append(' ');
            }
            else
            {
                return false;
            }
        }

        return text.ToString() == FooterText;
    }

    private static MarkdownManagedHostFact CreatePresent(
        string source,
        Block opening,
        Block closing,
        int parseStart,
        bool isLegacy)
    {
        var start = checked(parseStart + opening.Span.Start);
        var end = checked(parseStart + closing.Span.End + 1);
        if (end < source.Length && source[end] == '\r'
            && end + 1 < source.Length && source[end + 1] == '\n')
        {
            end += 2;
        }
        else if (end < source.Length && source[end] == '\n')
        {
            end++;
        }

        return MarkdownManagedHostFact.Present(new MarkdownTextSpan(start, end - start), isLegacy);
    }
}
