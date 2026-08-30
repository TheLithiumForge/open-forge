using System.Text;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Markdig.Renderers.Html;
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
            return new MarkdownDocumentFacts(
                source,
                frontmatter,
                null,
                [],
                [],
                [],
                [],
                [],
                MarkdownGeneratedRegionFact.Unavailable("The Markdown body boundary is unavailable."));
        }

        var bodyStart = frontmatter.BodyStart ?? throw new InvalidOperationException(
            "A Markdown document with an established frontmatter state must establish its body start.");
        var bodySpan = new MarkdownTextSpan(bodyStart, source.Length - bodyStart);
        var document = Markdig.Markdown.Parse(source[bodyStart..], MarkdownPipelineFactory.Get());
        var headings = new List<MarkdownHeadingFact>();
        var visibleText = new List<MarkdownVisibleTextFact>();
        var opaqueSpans = new List<MarkdownOpaqueSpan>();
        var links = new List<MarkdownLinkFact>();

        foreach (var block in EnumerateBlocks(document))
        {
            if (block is HeadingBlock heading)
            {
                headings.Add(CreateHeadingFact(heading, bodyStart));
            }

            if (block is CodeBlock or HtmlBlock)
            {
                AddOpaqueSpan(opaqueSpans, block.Span, bodyStart, block is CodeBlock);
                continue;
            }

            if (block is LeafBlock { Inline: not null } leaf)
            {
                CollectInlineFacts(leaf.Inline!, bodyStart, visibleText, opaqueSpans, links);
            }
        }

        var sections = CreateSections(headings, bodySpan);
        var generatedRegion = MarkdownGeneratedRegionParser.Parse(
            new MarkdownGeneratedRegionParseInput(
                Source: source,
                BodySpan: bodySpan,
                Headings: headings,
                OpaqueSpans: opaqueSpans));

        return new MarkdownDocumentFacts(
            source,
            frontmatter,
            bodySpan,
            headings,
            sections,
            visibleText,
            opaqueSpans,
            links,
            generatedRegion);
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
            ReadFragmentIdentifier(heading),
            span);
    }

    private static string? ReadFragmentIdentifier(HeadingBlock heading)
    {
        if (heading.IsSetext || !HasSupportedHeadingText(heading.Inline))
        {
            return null;
        }

        var identifier = heading.TryGetAttributes()?.Id;
        return string.IsNullOrWhiteSpace(identifier) ? null : identifier;
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
        ICollection<MarkdownOpaqueSpan> opaqueSpans,
        ICollection<MarkdownLinkFact> links)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case CodeInline:
                    AddOpaqueSpan(opaqueSpans, current.Span, bodyStart, isCode: true);
                    break;
                case HtmlInline:
                    AddOpaqueSpan(opaqueSpans, current.Span, bodyStart);
                    break;
                case LiteralInline:
                    AddVisibleSpan(visibleText, current.Span, bodyStart);
                    break;
                case AutolinkInline autolink:
                    AddAutolinkFact(links, autolink, bodyStart);
                    break;
                case LinkInline { IsImage: false } link:
                    AddLinkFact(links, link, bodyStart);
                    if (link.FirstChild is not null)
                    {
                        CollectInlineFacts(link.FirstChild, bodyStart, visibleText, opaqueSpans, links);
                    }

                    break;
                case ContainerInline container:
                    if (container.FirstChild is not null)
                    {
                        CollectInlineFacts(container.FirstChild, bodyStart, visibleText, opaqueSpans, links);
                    }

                    break;
            }
        }
    }

    private static void AddLinkFact(
        ICollection<MarkdownLinkFact> links,
        LinkInline link,
        int bodyStart)
    {
        var span = ToDocumentSpan(link.Span, bodyStart);
        if (span is null)
        {
            return;
        }

        var form = MarkdownLinkForm.Inline;
        if (link.Reference is not null)
        {
            form = MarkdownLinkForm.Reference;
        }
        else if (link.IsAutoLink)
        {
            form = MarkdownLinkForm.Autolink;
        }
        var parserDestinationSpan = form switch
        {
            MarkdownLinkForm.Reference => link.Reference?.UrlSpan,
            MarkdownLinkForm.Inline => link.UrlSpan,
            MarkdownLinkForm.Autolink => null,
            _ => throw new ArgumentOutOfRangeException(nameof(link), form, "The Markdown link form is not defined."),
        };
        var destinationSpan = parserDestinationSpan is { } sourceSpan
            ? ToDocumentSpan(sourceSpan, bodyStart)
            : null;
        var rawDestination = link.Url ?? string.Empty;
        links.Add(new MarkdownLinkFact(
            form: form,
            rawDestination: rawDestination,
            span: span,
            destinationSpan: destinationSpan,
            label: ReadLinkLabel(link)));
    }

    private static void AddAutolinkFact(
        ICollection<MarkdownLinkFact> links,
        AutolinkInline autolink,
        int bodyStart)
    {
        if (ToDocumentSpan(autolink.Span, bodyStart) is { } span)
        {
            links.Add(new MarkdownLinkFact(
                form: MarkdownLinkForm.Autolink,
                rawDestination: autolink.Url,
                span: span,
                destinationSpan: null,
                label: ReadAutolinkLabel(autolink.Url)));
        }
    }

    private static MarkdownLinkLabelFact ReadLinkLabel(LinkInline link)
    {
        if (link.FirstChild is null)
        {
            return MarkdownLinkLabelFact.Unsupported();
        }

        var text = new StringBuilder();
        if (!TryAppendLinkLabelText(link.FirstChild, text))
        {
            return MarkdownLinkLabelFact.Unsupported();
        }

        return CreateLinkLabel(text);
    }

    private static MarkdownLinkLabelFact ReadAutolinkLabel(string visibleText)
        => CreateLinkLabel(new StringBuilder(visibleText));

    private static MarkdownLinkLabelFact CreateLinkLabel(StringBuilder text)
    {
        var collapsedText = CollapseWhitespace(text);
        return string.IsNullOrWhiteSpace(collapsedText)
            ? MarkdownLinkLabelFact.Unsupported()
            : MarkdownLinkLabelFact.Supported(collapsedText);
    }

    private static bool TryAppendLinkLabelText(Inline inline, StringBuilder text)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case LiteralInline literal:
                    text.Append(literal.Content.ToString());
                    break;
                case HtmlEntityInline entity:
                    text.Append(entity.Transcoded);
                    break;
                case CodeInline code:
                    text.Append(code.Content.ToString());
                    break;
                case LineBreakInline:
                    text.Append(' ');
                    break;
                case EmphasisInline { FirstChild: not null } emphasis:
                    if (!TryAppendLinkLabelText(emphasis.FirstChild, text))
                    {
                        return false;
                    }

                    break;
                case EmphasisInline:
                    return false;
                default:
                    return false;
            }
        }

        return true;
    }

    private static bool HasSupportedHeadingText(ContainerInline? inline)
        => inline?.FirstChild is null || HasSupportedHeadingText(inline.FirstChild);

    private static bool HasSupportedHeadingText(Inline inline)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case LiteralInline:
                case HtmlEntityInline:
                case CodeInline:
                case LineBreakInline:
                    break;
                case LinkInline link when link.FirstChild is not null:
                    if (!HasSupportedHeadingText(link.FirstChild))
                    {
                        return false;
                    }

                    break;
                case ContainerInline container when container.FirstChild is not null:
                    if (!HasSupportedHeadingText(container.FirstChild))
                    {
                        return false;
                    }

                    break;
                default:
                    return false;
            }
        }

        return true;
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
        int bodyStart,
        bool isCode = false)
    {
        if (ToDocumentSpan(span, bodyStart) is { } documentSpan)
        {
            opaqueSpans.Add(new MarkdownOpaqueSpan(documentSpan, isCode));
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
