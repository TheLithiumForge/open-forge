using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownGeneratedRegionParser
{
    internal static MarkdownGeneratedRegionFact Parse(MarkdownGeneratedRegionParseInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var source = input.Source;
        var bodySpan = input.BodySpan;
        var headings = input.Headings;
        var opaqueSpans = input.OpaqueSpans;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(bodySpan);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(opaqueSpans);

        var entriesHeadings = headings
            .Where(heading => heading.Level == 2
                && heading.IsCanonical
                && string.Equals(
                    heading.VisibleText,
                    MarkdownGeneratedRegionSyntax.EntriesHeadingText,
                    StringComparison.Ordinal)
                && IsExactEntriesHeading(source, heading.Span))
            .ToArray();
        var markers = ReadMarkers(source, opaqueSpans)
            .OrderBy(marker => marker.Span.Start)
            .ThenBy(marker => marker.Span.End)
            .ToArray();
        if (HasMalformedMarker(source, opaqueSpans, markers))
        {
            return Invalid(MarkdownGeneratedRegionInvalidKind.Malformed);
        }

        if (entriesHeadings.Length == 0 && markers.Length == 0)
        {
            return MarkdownGeneratedRegionFact.Absent();
        }

        if (markers.Length == 0)
        {
            return MarkdownGeneratedRegionFact.Absent();
        }

        if (entriesHeadings.Length > 1 || markers.Length > 2)
        {
            return Invalid(MarkdownGeneratedRegionInvalidKind.Duplicate);
        }

        if (entriesHeadings.Length != 1
            || markers.Length != 2
            || markers[0].Kind != GeneratedMarkerKind.Start
            || markers[1].Kind != GeneratedMarkerKind.End)
        {
            return Invalid(MarkdownGeneratedRegionInvalidKind.Malformed);
        }

        var heading = entriesHeadings[0];
        var start = markers[0].Span;
        var end = markers[1].Span;
        if (headings.Any(candidate => candidate.Span.Start > heading.Span.Start && candidate.Level <= 2)
            || start.Start < heading.Span.End
            || end.Start < start.End
            || !ContainsOnlyLineEndings(source, end.End, bodySpan.End))
        {
            return Invalid(MarkdownGeneratedRegionInvalidKind.Misplaced);
        }

        return MarkdownGeneratedRegionFact.Complete(
            regionSpan: new MarkdownTextSpan(start.Start, checked(end.End - start.Start)),
            contentSpan: new MarkdownTextSpan(start.End, checked(end.Start - start.End)),
            omissionSpan: new MarkdownTextSpan(markers[0].NextStart, checked(end.Start - markers[0].NextStart)));
    }

    private static MarkdownGeneratedRegionFact Invalid(
        MarkdownGeneratedRegionInvalidKind kind)
        => MarkdownGeneratedRegionFact.Invalid(
            kind,
            "The document does not contain exactly one final Entries section with one complete ordered marker pair.");

    private static bool ContainsOnlyLineEndings(string source, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (source[index] is not '\r' and not '\n')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsExactEntriesHeading(string source, MarkdownTextSpan span)
    {
        return IsLineStart(source, span.Start)
            && string.Equals(
                source[span.Start..span.End],
                MarkdownGeneratedRegionSyntax.EntriesHeadingLine,
                StringComparison.Ordinal)
            && IsLineEndingOrEnd(source, span.End);
    }

    private static GeneratedMarker? ReadMarker(string source, MarkdownOpaqueSpan opaque)
    {
        if (opaque.IsCode || !IsLineStart(source, opaque.Span.Start))
        {
            return null;
        }

        var markerEnd = opaque.Span.End;
        if (markerEnd >= 2
            && source[markerEnd - 2] == '\r'
            && source[markerEnd - 1] == '\n')
        {
            markerEnd -= 2;
        }
        else if (markerEnd > 0 && source[markerEnd - 1] is '\r' or '\n')
        {
            markerEnd--;
        }

        var value = source[opaque.Span.Start..markerEnd];
        var kind = value switch
        {
            MarkdownGeneratedRegionSyntax.StartMarker => GeneratedMarkerKind.Start,
            MarkdownGeneratedRegionSyntax.EndMarker => GeneratedMarkerKind.End,
            _ => GeneratedMarkerKind.None,
        };
        if (kind == GeneratedMarkerKind.None || !IsLineEndingOrEnd(source, markerEnd))
        {
            return null;
        }

        var nextStart = markerEnd;
        if (nextStart < source.Length && source[nextStart] == '\r')
        {
            nextStart++;
            if (nextStart < source.Length && source[nextStart] == '\n')
            {
                nextStart++;
            }
        }
        else if (nextStart < source.Length && source[nextStart] == '\n')
        {
            nextStart++;
        }

        if (opaque.Span.End != markerEnd && opaque.Span.End != nextStart)
        {
            return null;
        }

        return new GeneratedMarker(
            kind,
            new MarkdownTextSpan(opaque.Span.Start, checked(markerEnd - opaque.Span.Start)),
            nextStart);
    }

    private static IEnumerable<GeneratedMarker> ReadMarkers(
        string source,
        IReadOnlyList<MarkdownOpaqueSpan> opaqueSpans)
    {
        foreach (var opaque in opaqueSpans)
        {
            if (ReadMarker(source, opaque) is { } marker)
            {
                yield return marker;
            }
        }
    }

    private static bool HasMalformedMarker(
        string source,
        IReadOnlyList<MarkdownOpaqueSpan> opaqueSpans,
        IReadOnlyList<GeneratedMarker> markers)
    {
        var offset = source.IndexOf(
            MarkdownGeneratedRegionSyntax.MarkerPrefix,
            StringComparison.Ordinal);
        while (offset >= 0)
        {
            var end = checked(offset + MarkdownGeneratedRegionSyntax.MarkerPrefix.Length);
            if (!IsWithinCode(opaqueSpans, offset, end)
                && !markers.Any(marker => marker.Span.Start <= offset && marker.Span.End >= end))
            {
                return true;
            }

            offset = end < source.Length
                ? source.IndexOf(MarkdownGeneratedRegionSyntax.MarkerPrefix, end, StringComparison.Ordinal)
                : -1;
        }

        return false;
    }

    private static bool IsWithinCode(
        IReadOnlyList<MarkdownOpaqueSpan> opaqueSpans,
        int start,
        int end)
        => opaqueSpans.Any(opaque => opaque.IsCode
            && opaque.Span.Start <= start
            && opaque.Span.End >= end);

    private static bool IsLineStart(string source, int offset)
        => offset == 0 || source[offset - 1] is '\r' or '\n';

    private static bool IsLineEndingOrEnd(string source, int offset)
        => offset == source.Length || source[offset] is '\r' or '\n';

    private readonly record struct GeneratedMarker(
        GeneratedMarkerKind Kind,
        MarkdownTextSpan Span,
        int NextStart);

    private enum GeneratedMarkerKind
    {
        None,
        Start,
        End,
    }
}
