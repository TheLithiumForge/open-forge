using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownEntriesSectionReader
{
    internal const string EntriesHeadingText = "Entries";
    internal const string EmptyEntry = "- none - No entries - #Empty";

    internal static MarkdownGeneratedRegionFact Parse(
        string source,
        MarkdownTextSpan body,
        IReadOnlyList<MarkdownHeadingFact> headings,
        MarkdownDocument document,
        int parseStart)
    {
        var sections = MarkdownSemanticSectionReader.Find(source, body, headings, EntriesHeadingText);
        if (sections.Count == 0)
        {
            return MarkdownGeneratedRegionFact.Absent();
        }

        if (sections.Count != 1)
        {
            return MarkdownGeneratedRegionFact.Invalid(MarkdownGeneratedRegionInvalidKind.Duplicate, "The document must contain exactly one ## Entries section.");
        }

        var section = sections[0];
        var content = new MarkdownTextSpan(section.Heading.Span.End, section.Span.End - section.Heading.Span.End);
        return MarkdownGeneratedRegionFact.Complete(
            section.Span,
            content,
            ReadBlock(source, content, document, parseStart));
    }

    private static MarkdownEntriesBlock ReadBlock(
        string source,
        MarkdownTextSpan content,
        MarkdownDocument document,
        int parseStart)
    {
        var list = FindFirstDashSpaceList(source, content, document, parseStart);
        if (list is null)
        {
            return new MarkdownEntriesBlock(ReadInsertionPoint(source, content), ReadSectionLineEnding(source, content));
        }

        var span = ReadFirstListSpan(source, content, list, parseStart);
        var lineEnding = ReadLineEnding(source, span.Start, span.End);
        return new MarkdownEntriesBlock(span, lineEnding ?? ReadSectionLineEnding(source, content));
    }

    private static ListBlock? FindFirstDashSpaceList(
        string source,
        MarkdownTextSpan content,
        MarkdownDocument document,
        int parseStart)
    {
        foreach (var block in document)
        {
            if (block is not ListBlock list || list.IsOrdered)
            {
                continue;
            }

            var start = checked(parseStart + list.Span.Start);
            if (start < content.Start || start >= content.End || !IsDashSpaceLine(source, start, content.End))
            {
                continue;
            }

            return list;
        }

        return null;
    }

    private static MarkdownTextSpan ReadFirstListSpan(
        string source,
        MarkdownTextSpan content,
        ListBlock list,
        int parseStart)
    {
        int? start = null;
        var end = content.Start;
        var previousWasGenerated = false;
        foreach (var block in list)
        {
            if (block is not ListItemBlock item)
            {
                break;
            }

            var itemStart = checked(parseStart + item.Span.Start);
            if (itemStart < content.Start
                || itemStart >= content.End
                || !IsDashSpaceLine(source, itemStart, content.End))
            {
                break;
            }

            var lineEnd = ReadLineEnd(source, itemStart, content.End);
            var next = AfterLineEnding(source, lineEnd, content.End);
            var isGenerated = IsGeneratedRouteEntry(source, itemStart, lineEnd, item);
            if (start is null)
            {
                start = itemStart;
            }
            else if (itemStart < end)
            {
                break;
            }
            else if (itemStart != end
                && (!source.AsSpan(end, itemStart - end).IsWhiteSpace()
                    || !previousWasGenerated
                    || !isGenerated))
            {
                break;
            }

            end = next;
            previousWasGenerated = isGenerated;
        }

        return start is { } listStart
            ? new MarkdownTextSpan(listStart, end - listStart)
            : new MarkdownTextSpan(ReadInsertionPoint(source, content).Start, 0);
    }

    private static bool IsGeneratedRouteEntry(
        string source,
        int itemStart,
        int lineEnd,
        ListItemBlock item)
    {
        if (source.AsSpan(itemStart, lineEnd - itemStart).SequenceEqual(EmptyEntry.AsSpan()))
        {
            return true;
        }

        foreach (var block in item)
        {
            if (block is not ParagraphBlock paragraph)
            {
                return false;
            }

            var firstInline = paragraph.Inline?.FirstChild;
            return firstInline is LinkInline link
                && !link.IsImage
                && !string.IsNullOrWhiteSpace(link.Url);
        }

        return false;
    }

    private static bool IsDashSpaceLine(string source, int start, int end)
        => start + 2 <= end && source.AsSpan(start, 2).SequenceEqual("- ".AsSpan());

    private static MarkdownTextSpan ReadInsertionPoint(string source, MarkdownTextSpan content)
    {
        var insertion = content.Start;
        for (var offset = content.Start; offset < content.End;)
        {
            var lineEnd = ReadLineEnd(source, offset, content.End);
            var next = AfterLineEnding(source, lineEnd, content.End);
            if (!source.AsSpan(offset, lineEnd - offset).IsWhiteSpace())
            {
                break;
            }

            insertion = next;
            offset = next;
        }

        return new MarkdownTextSpan(insertion, 0);
    }

    private static string? ReadLineEnding(string source, int start, int end)
    {
        var lineEnd = ReadLineEnd(source, start, end);
        var next = AfterLineEnding(source, lineEnd, end);
        return next > lineEnd ? source[lineEnd..next] : null;
    }

    internal static IReadOnlyList<MarkdownTextSpan> ReadRetiredGuardSpans(string source, MarkdownTextSpan content)
    {
        var guards = new List<MarkdownTextSpan>();
        for (var offset = content.Start; offset < content.End;)
        {
            var lineEnd = ReadLineEnd(source, offset, content.End);
            var next = AfterLineEnding(source, lineEnd, content.End);
            if (IsRetiredGuard(source[offset..lineEnd]))
            {
                guards.Add(new MarkdownTextSpan(offset, lineEnd - offset));
            }

            offset = next;
        }

        return guards;
    }

    private static string ReadSectionLineEnding(string source, MarkdownTextSpan content)
    {
        var lineEnd = ReadLineEnd(source, content.Start, content.End);
        var next = AfterLineEnding(source, lineEnd, content.End);
        if (next > lineEnd)
        {
            return source[lineEnd..next];
        }

        return source.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
    }

    private static int ReadLineEnd(string source, int start, int end)
    {
        while (start < end && source[start] is not ('\r' or '\n'))
        {
            start++;
        }

        return start;
    }

    private static int AfterLineEnding(string source, int position, int end)
    {
        if (position == end) return end;
        return source[position] == '\r' && position + 1 < end && source[position + 1] == '\n'
            ? position + 2
            : position + 1;
    }

    // Migration input only: these comments never establish a section boundary.
    // Index removes the obsolete text while retaining its line-ending separator.
    internal static bool IsRetiredGuard(string line)
        => line.Trim(' ', '\t') is "<!-- open-forge:generated-index:start -->" or "<!-- open-forge:generated-index:end -->";
}
