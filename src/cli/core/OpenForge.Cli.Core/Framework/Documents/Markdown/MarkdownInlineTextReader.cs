using System.Text;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownInlineTextReader
{
    internal static MarkdownLinkLabelFact ReadLinkLabel(LinkInline link)
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

    internal static MarkdownLinkLabelFact ReadAutolinkLabel(string visibleText)
        => CreateLinkLabel(new StringBuilder(visibleText));

    internal static bool HasSupportedHeadingText(ContainerInline? inline)
        => inline?.FirstChild is null || HasSupportedHeadingText(inline.FirstChild);

    internal static string ReadHeadingText(ContainerInline? inline)
    {
        var builder = new StringBuilder();
        if (inline?.FirstChild is not null)
        {
            AppendHeadingText(inline.FirstChild, builder);
        }

        return CollapseWhitespace(builder);
    }

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
}
