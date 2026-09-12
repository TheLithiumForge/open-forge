using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownInlineFactCollector
{
    internal static void Collect(
        Inline inline,
        int bodyStart,
        MarkdownInlineFactCollections facts)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case CodeInline:
                    AddOpaqueSpan(facts.OpaqueSpans, current.Span, bodyStart, isCode: true);
                    break;
                case HtmlInline:
                    AddOpaqueSpan(facts.OpaqueSpans, current.Span, bodyStart);
                    break;
                case LiteralInline:
                    AddVisibleSpan(facts.VisibleText, current.Span, bodyStart);
                    break;
                case AutolinkInline autolink:
                    AddAutolinkFact(facts.Links, autolink, bodyStart);
                    break;
                case LinkInline link:
                    AddLinkFact(link.IsImage ? facts.Images : facts.Links, link, bodyStart);
                    if (link.FirstChild is not null)
                    {
                        Collect(link.FirstChild, bodyStart, facts);
                    }

                    break;
                case ContainerInline container when container.FirstChild is not null:
                    Collect(container.FirstChild, bodyStart, facts);
                    break;
            }
        }
    }

    internal static void AddOpaqueSpan(
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

    internal static MarkdownTextSpan? ToDocumentSpan(
        SourceSpan span,
        int bodyStart)
    {
        if (span.Start < 0 || span.End < span.Start)
        {
            return null;
        }

        return new MarkdownTextSpan(
            checked(bodyStart + span.Start),
            checked(span.End - span.Start + 1));
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
            _ => throw new ArgumentOutOfRangeException(
                nameof(link),
                form,
                "The Markdown link form is not defined."),
        };
        var destinationSpan = parserDestinationSpan is { } sourceSpan
            ? ToDocumentSpan(sourceSpan, bodyStart)
            : null;
        links.Add(new MarkdownLinkFact(
            syntax: new MarkdownLinkSyntax(form, link.IsImage),
            rawDestination: link.Url ?? string.Empty,
            span: span,
            destinationSpan: destinationSpan,
            label: MarkdownInlineTextReader.ReadLinkLabel(link)));
    }

    private static void AddAutolinkFact(
        ICollection<MarkdownLinkFact> links,
        AutolinkInline autolink,
        int bodyStart)
    {
        if (ToDocumentSpan(autolink.Span, bodyStart) is { } span)
        {
            links.Add(new MarkdownLinkFact(
                syntax: new MarkdownLinkSyntax(MarkdownLinkForm.Autolink, isImage: false),
                rawDestination: autolink.Url,
                span: span,
                destinationSpan: null,
                label: MarkdownInlineTextReader.ReadAutolinkLabel(autolink.Url)));
        }
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
}
