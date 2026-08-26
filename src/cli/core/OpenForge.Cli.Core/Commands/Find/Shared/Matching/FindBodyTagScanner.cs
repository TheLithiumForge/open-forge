using System.Buffers;
using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Locations;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindBodyTagScanner
{
    private const string GeneratedStart = "<!-- open-forge:generated-index:start -->";
    private const string GeneratedEnd = "<!-- open-forge:generated-index:end -->";

    internal FindBodyTagFacts Scan(FindBodyTagInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var document = input.Document;
        if (document.BodySpan is null || !TryReadGeneratedSpan(document, out var generatedSpan))
        {
            return new FindBodyTagFacts(FindBodyTagAvailability.Unavailable, []);
        }

        var excludedSpans = document.OpaqueSpans
            .Select(opaqueSpan => opaqueSpan.Span)
            .Append(generatedSpan)
            .OrderBy(span => span.Start)
            .ThenBy(span => span.End)
            .ToArray();
        var occurrences = new List<FindBodyTagOccurrence>();
        var seen = new HashSet<(int Start, int Length)>();

        foreach (var visibleText in document.VisibleText)
        {
            foreach (var visibleRange in ReadVisibleRanges(visibleText.Span, excludedSpans))
            {
                ScanRange(document.Source, visibleRange.Start, visibleRange.End, occurrences, seen);
            }
        }

        return new FindBodyTagFacts(FindBodyTagAvailability.Complete, occurrences);
    }

    private static bool TryReadGeneratedSpan(
        MarkdownDocumentFacts document,
        out MarkdownTextSpan generatedSpan)
    {
        var markers = document.OpaqueSpans
            .Select(span => (Span: span.Span, Kind: ReadMarkerKind(document.Source, span.Span)))
            .Where(marker => marker.Kind != GeneratedMarkerKind.None)
            .OrderBy(marker => marker.Span.Start)
            .ThenBy(marker => marker.Span.End)
            .ToArray();

        if (markers.Length == 0)
        {
            generatedSpan = new MarkdownTextSpan(0, 0);
            return true;
        }

        if (markers.Length != 2
            || markers[0].Kind != GeneratedMarkerKind.Start
            || markers[1].Kind != GeneratedMarkerKind.End
            || markers[0].Span.End > markers[1].Span.Start)
        {
            generatedSpan = new MarkdownTextSpan(0, 0);
            return false;
        }

        generatedSpan = new MarkdownTextSpan(
            markers[0].Span.Start,
            checked(markers[1].Span.End - markers[0].Span.Start));
        return true;
    }

    private static GeneratedMarkerKind ReadMarkerKind(string source, MarkdownTextSpan span)
    {
        var value = source[span.Start..span.End];
        if (value.EndsWith("\r\n", StringComparison.Ordinal))
        {
            value = value[..^2];
        }
        else if (value.EndsWith('\n') || value.EndsWith('\r'))
        {
            value = value[..^1];
        }

        return value switch
        {
            GeneratedStart => GeneratedMarkerKind.Start,
            GeneratedEnd => GeneratedMarkerKind.End,
            _ => GeneratedMarkerKind.None,
        };
    }

    private static IEnumerable<(int Start, int End)> ReadVisibleRanges(
        MarkdownTextSpan visibleSpan,
        IReadOnlyList<MarkdownTextSpan> excludedSpans)
    {
        var start = visibleSpan.Start;
        foreach (var excludedSpan in excludedSpans)
        {
            if (excludedSpan.End <= start)
            {
                continue;
            }

            if (excludedSpan.Start >= visibleSpan.End)
            {
                break;
            }

            if (excludedSpan.Start > start)
            {
                yield return (start, Math.Min(excludedSpan.Start, visibleSpan.End));
            }

            start = Math.Max(start, excludedSpan.End);
            if (start >= visibleSpan.End)
            {
                yield break;
            }
        }

        if (start < visibleSpan.End)
        {
            yield return (start, visibleSpan.End);
        }
    }

    private static void ScanRange(
        string source,
        int start,
        int end,
        ICollection<FindBodyTagOccurrence> occurrences,
        ISet<(int Start, int Length)> seen)
    {
        for (var index = start; index < end; index++)
        {
            if (source[index] != '#'
                || IsEscaped(source, index)
                || !HasTokenBoundaryBefore(source, index))
            {
                continue;
            }

            if (!TryReadTagName(source, index + 1, end, out var nameEnd)
                || !HasTokenBoundaryAfter(source, nameEnd)
                || !seen.Add((index + 1, nameEnd - index - 1)))
            {
                continue;
            }

            occurrences.Add(new FindBodyTagOccurrence(
                source[(index + 1)..nameEnd],
                MapLocation(source, index + 1, nameEnd - index - 1)));
            index = nameEnd - 1;
        }
    }

    private static bool TryReadTagName(
        string source,
        int start,
        int end,
        out int nameEnd)
    {
        nameEnd = start;
        if (start >= end || !TryReadRune(source, start, end, out var firstRune, out var firstLength)
            || !Rune.IsLetter(firstRune))
        {
            return false;
        }

        nameEnd += firstLength;
        var previousWasHyphen = false;
        while (nameEnd < end && TryReadRune(source, nameEnd, end, out var rune, out var length))
        {
            if (Rune.IsLetterOrDigit(rune))
            {
                previousWasHyphen = false;
                nameEnd += length;
                continue;
            }

            if (rune.Value == '-'
                && !previousWasHyphen
                && nameEnd + length < end)
            {
                previousWasHyphen = true;
                nameEnd += length;
                continue;
            }

            break;
        }

        return !previousWasHyphen;
    }

    private static bool TryReadRune(
        string source,
        int start,
        int end,
        out Rune rune,
        out int length)
    {
        var status = Rune.DecodeFromUtf16(source.AsSpan(start, end - start), out rune, out length);
        return status == OperationStatus.Done;
    }

    private static bool IsEscaped(string source, int index)
    {
        var slashCount = 0;
        for (var preceding = index - 1; preceding >= 0 && source[preceding] == '\\'; preceding--)
        {
            slashCount++;
        }

        return (slashCount & 1) != 0;
    }

    private static bool HasTokenBoundaryBefore(string source, int index)
    {
        if (index == 0)
        {
            return true;
        }

        return !IsTokenBoundaryCharacter(source, index - 1);
    }

    private static bool HasTokenBoundaryAfter(string source, int index)
    {
        return index >= source.Length || !IsTokenBoundaryCharacter(source, index);
    }

    private static bool IsTokenBoundaryCharacter(string source, int index)
    {
        if (source[index] is '#' or '-' or '_')
        {
            return true;
        }

        var status = Rune.DecodeLastFromUtf16(source.AsSpan(0, index + 1), out var rune, out _);
        return status == OperationStatus.Done && Rune.IsLetterOrDigit(rune);
    }

    private static SourceLocation MapLocation(string source, int start, int length)
        => new Utf8SourceMap(source).Map(start, length);

    private enum GeneratedMarkerKind
    {
        None,
        Start,
        End,
    }
}
