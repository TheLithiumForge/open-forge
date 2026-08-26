using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownGeneratedRegionParser
{
    private const string StartMarker = "<!-- open-forge:generated-index:start -->";
    private const string EndMarker = "<!-- open-forge:generated-index:end -->";

    internal static MarkdownGeneratedRegionFact Parse(
        string source,
        MarkdownTextSpan bodySpan,
        IReadOnlyList<MarkdownHeadingFact> headings,
        IReadOnlyList<MarkdownOpaqueSpan> opaqueSpans)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(bodySpan);
        ArgumentNullException.ThrowIfNull(headings);
        ArgumentNullException.ThrowIfNull(opaqueSpans);

        var entriesHeadings = headings
            .Where(heading => heading.Level == 2
                && heading.IsCanonical
                && string.Equals(heading.VisibleText, "Entries", StringComparison.Ordinal))
            .ToArray();
        var markers = opaqueSpans
            .Select(opaque => (opaque.Span, Kind: ReadMarkerKind(source, opaque.Span)))
            .Where(marker => marker.Kind != GeneratedMarkerKind.None)
            .OrderBy(marker => marker.Span.Start)
            .ThenBy(marker => marker.Span.End)
            .ToArray();
        if (entriesHeadings.Length == 0 && markers.Length == 0)
        {
            return MarkdownGeneratedRegionFact.Absent();
        }

        if (entriesHeadings.Length != 1
            || markers.Length != 2
            || markers[0].Kind != GeneratedMarkerKind.Start
            || markers[1].Kind != GeneratedMarkerKind.End)
        {
            return Unavailable();
        }

        var heading = entriesHeadings[0];
        var start = markers[0].Span;
        var end = markers[1].Span;
        if (headings.Any(candidate => candidate.Span.Start > heading.Span.Start && candidate.Level <= 2)
            || start.Start < heading.Span.End
            || end.Start < start.End
            || !ContainsOnlyWhitespace(source, heading.Span.End, start.Start)
            || !ContainsOnlyWhitespace(source, end.End, bodySpan.End))
        {
            return Unavailable();
        }

        return MarkdownGeneratedRegionFact.Complete(
            new MarkdownTextSpan(start.Start, checked(end.End - start.Start)),
            new MarkdownTextSpan(start.End, checked(end.Start - start.End)));
    }

    private static MarkdownGeneratedRegionFact Unavailable()
        => MarkdownGeneratedRegionFact.Unavailable(
            "The document does not contain exactly one final Entries section with one complete ordered marker pair.");

    private static bool ContainsOnlyWhitespace(string source, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            if (!char.IsWhiteSpace(source[index]))
            {
                return false;
            }
        }

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
            StartMarker => GeneratedMarkerKind.Start,
            EndMarker => GeneratedMarkerKind.End,
            _ => GeneratedMarkerKind.None,
        };
    }

    private enum GeneratedMarkerKind
    {
        None,
        Start,
        End,
    }
}
