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

        var generatedRegion = facts.GeneratedRegion;
        var region = MapGeneratedRegion(normalized, generatedRegion);
        if (region.State is MarkdownFingerprintRegionState.Invalid
            or MarkdownFingerprintRegionState.Ambiguous
            or MarkdownFingerprintRegionState.Unavailable)
        {
            return ExactFallback(
                bytes,
                generatedRegion.State == MarkdownGeneratedRegionState.Unavailable
                    ? generatedRegion.Cause ?? "The Markdown document is unavailable for semantic fingerprinting."
                    : "The generated Markdown boundary is not a valid final Entries region.",
                region);
        }

        var output = region.State == MarkdownFingerprintRegionState.Valid
            ? RemoveInterior(normalized, generatedRegion)
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

    private static MarkdownFingerprintRegion MapGeneratedRegion(
        string source,
        MarkdownGeneratedRegionFact generatedRegion)
    {
        if (generatedRegion.State == MarkdownGeneratedRegionState.Absent)
        {
            return MarkdownFingerprintRegion.Absent();
        }

        if (generatedRegion.State == MarkdownGeneratedRegionState.Invalid)
        {
            return MarkdownFingerprintRegion.Invalid(MarkdownFingerprintRegionState.Invalid);
        }

        if (generatedRegion.State == MarkdownGeneratedRegionState.Unavailable)
        {
            return MarkdownFingerprintRegion.Invalid(MarkdownFingerprintRegionState.Unavailable);
        }

        var omission = generatedRegion.OmissionSpan
            ?? throw new InvalidOperationException("A complete generated region must establish an omission span.");
        var startOffset = ByteCount(source, omission.Start);
        var endOffset = ByteCount(source, omission.End);
        return new MarkdownFingerprintRegion(
            MarkdownFingerprintRegionState.Valid,
            MarkdownGeneratedRegionSyntax.StartMarker,
            MarkdownGeneratedRegionSyntax.EndMarker,
            startOffset,
            endOffset,
            checked(endOffset - startOffset),
            markerLinesRetained: true);
    }

    private static string RemoveInterior(
        string source,
        MarkdownGeneratedRegionFact generatedRegion)
    {
        var omission = generatedRegion.OmissionSpan
            ?? throw new InvalidOperationException("A complete generated region must establish an omission span.");
        return string.Concat(source.AsSpan(0, omission.Start), source.AsSpan(omission.End));
    }

    private static int ByteCount(string source, int charCount)
        => checked((int)StrictUtf8.GetByteCount(source.AsSpan(0, charCount)));
}
