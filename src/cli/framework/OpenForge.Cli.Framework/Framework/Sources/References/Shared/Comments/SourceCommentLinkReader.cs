using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;

// @open-forge .agents/memory/crystallized/documents/cli/contracts/shared/source-references/interface.md#comment-links defines the marker grammar
internal static class SourceCommentLinkReader
{
    internal const string OpenForgeMarker = "@open-forge";
    internal const string RuneMarker = "@rune";
    private const string HtmlOpen = "<!--";
    private const string HtmlClose = "-->";
    private const string BlockOpen = "/*";
    private const string BlockClose = "*/";

    internal static IReadOnlyList<SourceAuthoredLink> Read(string path, string source, MarkdownDocumentFacts? markdown = null)
    {
        if (!source.Contains(OpenForgeMarker, StringComparison.Ordinal) && !source.Contains(RuneMarker, StringComparison.Ordinal)) return [];
        var links = new List<SourceAuthoredLink>();
        var syntax = SourceCommentSyntax.Read(path);
        string? blockEnd = null;
        char quote = '\0';
        var quoteWidth = 0;
        var verbatim = false;
        var lineStart = true;
        for (var index = 0; index < source.Length; index++)
        {
            if (markdown?.OpaqueSpans.Any(span => span.IsCode && index >= span.Span.Start && index < span.Span.End) == true)
                continue;
            var current = source[index];
            if (current == '\n')
            {
                lineStart = true;
                if (quoteWidth == 1 && quote != '`' && !verbatim) quote = '\0';
                continue;
            }
            if (blockEnd is not null)
            {
                if (source.AsSpan(index).StartsWith(blockEnd, StringComparison.Ordinal))
                {
                    index += blockEnd.Length - 1;
                    blockEnd = null;
                    lineStart = false;
                }
                else if (lineStart)
                {
                    if (char.IsWhiteSpace(current) || current == '*') continue;
                    ReadMarker(index, blockEnd);
                    lineStart = false;
                }
                continue;
            }
            if (markdown is null && quote != '\0')
            {
                if (!verbatim && current == '\\') { index++; continue; }
                if (current != quote) continue;
                if (verbatim && index + 1 < source.Length && source[index + 1] == quote) { index++; continue; }
                var width = 1;
                while (index + width < source.Length && source[index + width] == quote) width++;
                if (width >= quoteWidth) { index += quoteWidth - 1; quote = '\0'; }
                continue;
            }
            if (syntax.HtmlBlocks && source.AsSpan(index).StartsWith(HtmlOpen, StringComparison.Ordinal)
                || syntax.SlashBlocks && markdown is null && source.AsSpan(index).StartsWith(BlockOpen, StringComparison.Ordinal))
            {
                var html = source.AsSpan(index).StartsWith(HtmlOpen, StringComparison.Ordinal);
                var length = html ? HtmlOpen.Length : BlockOpen.Length;
                blockEnd = html ? HtmlClose : BlockClose;
                ReadMarker(index + length, blockEnd);
                index += length - 1;
                lineStart = false;
                continue;
            }
            if (markdown is not null) continue;
            var delimiter = syntax.Lines.FirstOrDefault(value => source.AsSpan(index).StartsWith(value, StringComparison.Ordinal));
            if (delimiter is not null)
            {
                var start = index + delimiter.Length;
                while (start < source.Length && source[start] == '/') start++;
                ReadMarker(start, null);
                var end = source.IndexOf('\n', start);
                index = end < 0 ? source.Length : end - 1;
                continue;
            }
            if (current is '"' or '\'' or '`')
            {
                quote = current;
                quoteWidth = 1;
                while (index + quoteWidth < source.Length && source[index + quoteWidth] == quote) quoteWidth++;
                if (quoteWidth < 3) quoteWidth = 1;
                verbatim = index > 0 && source[index - 1] == '@';
                index += quoteWidth - 1;
            }
            if (!char.IsWhiteSpace(current)) lineStart = false;
        }
        return links;

        void ReadMarker(int offset, string? terminator)
        {
            var end = source.IndexOf('\n', offset);
            if (end < 0) end = source.Length;
            if (terminator is not null)
            {
                var close = source.IndexOf(terminator, offset, StringComparison.Ordinal);
                if (close >= 0 && close < end) end = close;
            }
            while (offset < end && (char.IsWhiteSpace(source[offset]) || source[offset] == '*')) offset++;
            var marker = new[] { OpenForgeMarker, RuneMarker }.FirstOrDefault(value =>
                source.AsSpan(offset, end - offset).StartsWith(value, StringComparison.Ordinal)
                && offset + value.Length < end && char.IsWhiteSpace(source[offset + value.Length]));
            if (marker is null) return;
            var start = offset + marker.Length;
            while (start < end && char.IsWhiteSpace(source[start])) start++;
            var targetEnd = start;
            while (targetEnd < end && !char.IsWhiteSpace(source[targetEnd])) targetEnd++;
            if (targetEnd == start) return;
            var reason = source[targetEnd..end].Trim();
            var fact = new MarkdownLinkFact(new(MarkdownLinkForm.Inline, false), source[start..targetEnd],
                new MarkdownTextSpan(offset, end - offset), new MarkdownTextSpan(start, targetEnd - start),
                reason.Length == 0 ? MarkdownLinkLabelFact.Unsupported() : MarkdownLinkLabelFact.Supported(reason));
            links.Add(new SourceAuthoredLink(fact, IsComment: true));
        }
    }
}
