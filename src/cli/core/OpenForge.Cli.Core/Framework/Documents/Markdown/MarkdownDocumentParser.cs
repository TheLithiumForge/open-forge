using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal sealed class MarkdownDocumentParser
{
    private readonly MarkdownFrontmatterParser _frontmatterParser = new();

    internal MarkdownDocumentFacts Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var frontmatter = _frontmatterParser.Parse(source);
        if (frontmatter.State == MarkdownFrontmatterState.Unavailable)
        {
            return new MarkdownDocumentFacts(source, frontmatter, null, [], [], [], []);
        }

        var bodyStart = frontmatter.BodyStart ?? throw new InvalidOperationException(
            "A Markdown document with an established frontmatter state must establish its body start.");
        var bodySpan = new MarkdownTextSpan(bodyStart, source.Length - bodyStart);
        var document = Markdig.Markdown.Parse(source[bodyStart..], MarkdownPipelineFactory.Get());
        var headings = new List<MarkdownHeadingFact>();
        var visibleText = new List<MarkdownVisibleTextFact>();
        var opaqueSpans = new List<MarkdownOpaqueSpan>();

        foreach (var block in EnumerateBlocks(document))
        {
            if (block is HeadingBlock heading)
            {
                headings.Add(CreateHeadingFact(heading, bodyStart));
            }

            if (block is CodeBlock or HtmlBlock)
            {
                AddOpaqueSpan(opaqueSpans, block.Span, bodyStart);
                continue;
            }

            if (block is LeafBlock { Inline: not null } leaf)
            {
                CollectInlineFacts(leaf.Inline!, bodyStart, visibleText, opaqueSpans);
            }
        }

        var sections = CreateSections(headings, bodySpan);

        return new MarkdownDocumentFacts(
            source,
            frontmatter,
            bodySpan,
            headings,
            sections,
            visibleText,
            opaqueSpans);
    }

    private static MarkdownHeadingFact CreateHeadingFact(HeadingBlock heading, int bodyStart)
    {
        var span = ToDocumentSpan(heading.Span, bodyStart)
            ?? throw new InvalidOperationException("Markdig returned a heading without a source span.");
        return new MarkdownHeadingFact(
            ReadHeadingText(heading.Inline),
            heading.Level,
            heading.IsSetext ? MarkdownHeadingForm.Setext : MarkdownHeadingForm.Atx,
            !heading.IsSetext,
            span);
    }

    private static IReadOnlyList<MarkdownSectionFact> CreateSections(
        IReadOnlyList<MarkdownHeadingFact> headings,
        MarkdownTextSpan bodySpan)
    {
        var sections = new MarkdownSectionFact[headings.Count];
        for (var index = 0; index < headings.Count; index++)
        {
            var end = bodySpan.End;
            for (var next = index + 1; next < headings.Count; next++)
            {
                if (headings[next].Level <= headings[index].Level)
                {
                    end = headings[next].Span.Start;
                    break;
                }
            }

            sections[index] = new MarkdownSectionFact(
                headings[index],
                new MarkdownTextSpan(headings[index].Span.Start, end - headings[index].Span.Start));
        }

        return sections;
    }

    private static void CollectInlineFacts(
        Inline inline,
        int bodyStart,
        ICollection<MarkdownVisibleTextFact> visibleText,
        ICollection<MarkdownOpaqueSpan> opaqueSpans)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case CodeInline:
                case HtmlInline:
                    AddOpaqueSpan(opaqueSpans, current.Span, bodyStart);
                    break;
                case LiteralInline:
                    AddVisibleSpan(visibleText, current.Span, bodyStart);
                    break;
                case ContainerInline container:
                    if (container.FirstChild is not null)
                    {
                        CollectInlineFacts(container.FirstChild, bodyStart, visibleText, opaqueSpans);
                    }

                    break;
            }
        }
    }

    private static string ReadHeadingText(ContainerInline? inline)
    {
        var builder = new StringBuilder();
        if (inline?.FirstChild is not null)
        {
            AppendHeadingText(inline.FirstChild, builder);
        }

        return CollapseWhitespace(builder);
    }

    private static void AppendHeadingText(Inline inline, StringBuilder builder)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case LiteralInline literal:
                    builder.Append(literal.Content.ToString());
                    break;
                case HtmlEntityInline entity:
                    builder.Append(entity.Transcoded);
                    break;
                case CodeInline code:
                    builder.Append(code.Content.ToString());
                    break;
                case LineBreakInline:
                    builder.Append(' ');
                    break;
                case LinkInline link when link.FirstChild is not null:
                    AppendHeadingText(link.FirstChild, builder);
                    break;
                case ContainerInline container when container.FirstChild is not null:
                    AppendHeadingText(container.FirstChild, builder);
                    break;
            }
        }
    }

    private static string CollapseWhitespace(StringBuilder text)
    {
        var result = new StringBuilder(text.Length);
        var pendingSpace = false;
        foreach (var character in text.ToString())
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = result.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                result.Append(' ');
                pendingSpace = false;
            }

            result.Append(character);
        }

        return result.ToString();
    }

    private static void AddVisibleSpan(
        ICollection<MarkdownVisibleTextFact> visibleText,
        SourceSpan span,
        int bodyStart)
    {
        if (ToDocumentSpan(span, bodyStart) is { } documentSpan)
        {
            visibleText.Add(new MarkdownVisibleTextFact(documentSpan));
        }
    }

    private static void AddOpaqueSpan(
        ICollection<MarkdownOpaqueSpan> opaqueSpans,
        SourceSpan span,
        int bodyStart)
    {
        if (ToDocumentSpan(span, bodyStart) is { } documentSpan)
        {
            opaqueSpans.Add(new MarkdownOpaqueSpan(documentSpan));
        }
    }

    private static MarkdownTextSpan? ToDocumentSpan(SourceSpan span, int bodyStart)
    {
        if (span.Start < 0 || span.End < span.Start)
        {
            return null;
        }

        return new MarkdownTextSpan(
            checked(bodyStart + span.Start),
            checked(span.End - span.Start + 1));
    }

    private static IEnumerable<Block> EnumerateBlocks(ContainerBlock container)
    {
        foreach (var block in container)
        {
            yield return block;
            if (block is ContainerBlock child)
            {
                foreach (var descendant in EnumerateBlocks(child))
                {
                    yield return descendant;
                }
            }
        }
    }

}
