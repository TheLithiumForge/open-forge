using System.Text;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

/// <summary>
/// Forms the operation-time identity for one admitted Markdown byte sequence.
/// This reader has no persistence or mutation responsibility.
/// </summary>
internal sealed class MarkdownFingerprintReader
{
    internal const string Policy = MarkdownFingerprintPolicy.Name;

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly MarkdownDocumentParser _documentParser = new();

    internal MarkdownFingerprintFacts Read(
        ReadOnlySpan<byte> bytes,
        bool supportedMarkdown = true)
    {
        if (!supportedMarkdown)
        {
            return ExactFallback(bytes, "The package path is not an admitted Markdown path.");
        }

        if (bytes.IndexOf((byte)0) >= 0)
        {
            return ExactFallback(bytes, "The Markdown bytes contain a NUL boundary.");
        }

        string source;
        try
        {
            source = StrictUtf8.GetString(bytes);
            _ = _documentParser.Parse(source);
        }
        catch (Exception exception) when (exception is DecoderFallbackException or ArgumentException or InvalidOperationException)
        {
            return ExactFallback(bytes, "The Markdown source could not be parsed safely.");
        }

        var normalized = NormalizeLineEndings(source);
        MarkdownDocumentFacts facts;
        try
        {
            facts = _documentParser.Parse(normalized);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return ExactFallback(bytes, "The normalized Markdown source could not be parsed safely.");
        }

        var region = FindGeneratedRegion(normalized, facts);
        if (region.State is MarkdownFingerprintRegionState.Invalid
            or MarkdownFingerprintRegionState.Ambiguous
            or MarkdownFingerprintRegionState.Unavailable)
        {
            return ExactFallback(bytes, "The generated Markdown boundary is not a valid final Entries region.", region);
        }

        var output = region.State == MarkdownFingerprintRegionState.Valid
            ? RemoveInterior(normalized, region)
            : normalized;
        var hash = Convert.ToHexStringLower(SHA256.HashData(StrictUtf8.GetBytes(output)));
        return new MarkdownFingerprintFacts(
            MarkdownFingerprintState.Semantic,
            Policy,
            hash,
            region,
            null);
    }

    private static MarkdownFingerprintFacts ExactFallback(
        ReadOnlySpan<byte> bytes,
        string cause,
        MarkdownFingerprintRegion? region = null)
        => new(
            MarkdownFingerprintState.ExactBytes,
            Policy,
            Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(bytes)),
            region ?? MarkdownFingerprintRegion.Invalid(MarkdownFingerprintRegionState.Unavailable),
            cause);

    private static string NormalizeLineEndings(string source)
    {
        if (!source.Contains('\r'))
        {
            return source;
        }

        var builder = new StringBuilder(source.Length);
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] == '\r')
            {
                builder.Append('\n');
                if (index + 1 < source.Length && source[index + 1] == '\n')
                {
                    index++;
                }
            }
            else
            {
                builder.Append(source[index]);
            }
        }

        return builder.ToString();
    }

    private static MarkdownFingerprintRegion FindGeneratedRegion(
        string source,
        MarkdownDocumentFacts facts)
    {
        var lines = ReadLines(source);
        var markerCandidates = new List<MarkerCandidate>();
        var malformedMarker = false;
        foreach (var line in lines)
        {
            var content = source[line.Start..line.End];
            if (string.Equals(content, MarkdownGeneratedRegionSyntax.StartMarker, StringComparison.Ordinal)
                && ContainsOutsideCode(
                    facts,
                    source,
                    line.Start,
                    line.End,
                    MarkdownGeneratedRegionSyntax.StartMarker))
            {
                markerCandidates.Add(new MarkerCandidate(MarkerKind.Start, line));
            }
            else if (string.Equals(content, MarkdownGeneratedRegionSyntax.EndMarker, StringComparison.Ordinal)
                && ContainsOutsideCode(
                    facts,
                    source,
                    line.Start,
                    line.End,
                    MarkdownGeneratedRegionSyntax.EndMarker))
            {
                markerCandidates.Add(new MarkerCandidate(MarkerKind.End, line));
            }
            else if (content.Contains(MarkdownGeneratedRegionSyntax.MarkerPrefix, StringComparison.Ordinal)
                && ContainsOutsideCode(
                    facts,
                    source,
                    line.Start,
                    line.End,
                    MarkdownGeneratedRegionSyntax.MarkerPrefix))
            {
                malformedMarker = true;
            }
        }

        var entries = facts.Headings
            .Where(heading => heading.Level == 2
                && heading.IsCanonical
                && string.Equals(
                    heading.VisibleText,
                    MarkdownGeneratedRegionSyntax.EntriesHeadingText,
                    StringComparison.Ordinal)
                && lines.Any(line => line.Start == heading.Span.Start
                    && string.Equals(
                        source[line.Start..line.End],
                        MarkdownGeneratedRegionSyntax.EntriesHeadingLine,
                        StringComparison.Ordinal)))
            .ToArray();
        if (malformedMarker)
        {
            return MarkdownFingerprintRegion.Invalid(MarkdownFingerprintRegionState.Invalid);
        }

        if (markerCandidates.Count == 0)
        {
            return MarkdownFingerprintRegion.Absent();
        }

        if (entries.Length != 1
            || markerCandidates.Count != 2
            || markerCandidates[0].Kind != MarkerKind.Start
            || markerCandidates[1].Kind != MarkerKind.End)
        {
            return MarkdownFingerprintRegion.Invalid(MarkdownFingerprintRegionState.Invalid);
        }

        var heading = entries[0];
        var start = markerCandidates[0].Line;
        var end = markerCandidates[1].Line;
        if (facts.Headings.Any(candidate => candidate.Span.Start > heading.Span.Start && candidate.Level <= 2)
            || start.Start < heading.Span.End
            || end.Start < start.NextStart
            || lines.Any(line => line.Start >= end.NextStart && line.End > line.Start))
        {
            return MarkdownFingerprintRegion.Invalid(MarkdownFingerprintRegionState.Invalid);
        }

        var startOffset = ByteCount(source, start.NextStart);
        var endOffset = ByteCount(source, end.Start);
        return new MarkdownFingerprintRegion(
            MarkdownFingerprintRegionState.Valid,
            MarkdownGeneratedRegionSyntax.StartMarker,
            MarkdownGeneratedRegionSyntax.EndMarker,
            startOffset,
            endOffset,
            checked(endOffset - startOffset),
            markerLinesRetained: true);
    }

    private static bool ContainsOutsideCode(
        MarkdownDocumentFacts facts,
        string source,
        int lineStart,
        int lineEnd,
        string value)
    {
        var content = source[lineStart..lineEnd];
        var offset = content.IndexOf(value, StringComparison.Ordinal);
        while (offset >= 0)
        {
            var valueStart = lineStart + offset;
            var valueEnd = valueStart + value.Length;
            if (!facts.OpaqueSpans.Any(opaque => opaque.IsCode
                && opaque.Span.Start <= valueStart
                && opaque.Span.End >= valueEnd))
            {
                return true;
            }

            var nextOffset = offset + value.Length;
            offset = nextOffset < content.Length
                ? content[nextOffset..].IndexOf(value, StringComparison.Ordinal)
                : -1;
            if (offset >= 0)
            {
                offset += nextOffset;
            }
        }

        return false;
    }

    private static string RemoveInterior(string source, MarkdownFingerprintRegion region)
    {
        var normalizedStart = CharOffsetForUtf8ByteCount(source, region.StartByteOffset
            ?? throw new InvalidOperationException("A valid region must establish a start offset."));
        var normalizedEnd = CharOffsetForUtf8ByteCount(source, region.EndByteOffset
            ?? throw new InvalidOperationException("A valid region must establish an end offset."));
        return string.Concat(source.AsSpan(0, normalizedStart), source.AsSpan(normalizedEnd));
    }

    private static int ByteCount(string source, int charCount)
        => checked((int)StrictUtf8.GetByteCount(source.AsSpan(0, charCount)));

    private static int CharOffsetForUtf8ByteCount(string source, int byteCount)
    {
        if (byteCount == 0)
        {
            return 0;
        }

        var offset = 0;
        var index = 0;
        foreach (var rune in source.EnumerateRunes())
        {
            offset = checked(offset + rune.Utf8SequenceLength);
            if (offset == byteCount)
            {
                return index + rune.Utf16SequenceLength;
            }

            if (offset > byteCount)
            {
                throw new InvalidOperationException("A UTF-8 byte offset split a scalar value.");
            }

            index += rune.Utf16SequenceLength;
        }

        throw new InvalidOperationException("A UTF-8 byte offset was outside the normalized source.");
    }

    private static IReadOnlyList<SourceLine> ReadLines(string source)
    {
        var lines = new List<SourceLine>();
        var start = 0;
        while (start < source.Length || (source.Length == 0 && lines.Count == 0))
        {
            var end = start;
            while (end < source.Length && source[end] != '\n')
            {
                end++;
            }

            var next = end < source.Length ? end + 1 : end;
            lines.Add(new SourceLine(start, end, next));
            if (next == start)
            {
                break;
            }

            start = next;
            if (start == source.Length)
            {
                break;
            }
        }

        return lines;
    }

    private readonly record struct SourceLine(int Start, int End, int NextStart);

    private readonly record struct MarkerCandidate(MarkerKind Kind, SourceLine Line);

    private enum MarkerKind
    {
        Start,
        End,
    }
}
