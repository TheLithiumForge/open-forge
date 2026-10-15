using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameContentReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static RenameContent Read(ReadOnlySpan<byte> bytes, MarkdownDocumentFacts? retainedDocument = null)
    {
        var text = StrictUtf8.GetString(bytes);
        if (text.Contains('\0')) throw new ArgumentException("Markdown content cannot contain NUL.", nameof(bytes));
        var document = retainedDocument is not null && string.Equals(retainedDocument.Source, text, StringComparison.Ordinal)
            ? retainedDocument : new MarkdownDocumentParser().Parse(text);
        var authoredSpans = ReadAuthoredSpans(document);
        var normalizedBytes = StrictUtf8.GetBytes(text.Replace("\r\n", "\n", StringComparison.Ordinal));
        return new RenameContent
        {
            Bytes = ImmutableArray.Create(bytes),
            Hash = FileExpectation.Hash(bytes),
            NormalizedHash = FileExpectation.Hash(normalizedBytes),
            NormalizedByteLength = normalizedBytes.Length,
            Document = document,
            HasAuthoredContent = authoredSpans.Any(span => !text.AsSpan(span.Start, span.Length).IsWhiteSpace()),
            Lines = bytes.Length <= RenameContentComparer.MaximumBytes ? ReadLines(document, authoredSpans) : default,
        };
    }

    private static ImmutableArray<MarkdownTextSpan> ReadAuthoredSpans(MarkdownDocumentFacts document)
    {
        if (document.BodySpan is not { } body
            || document.GeneratedRegion.State is MarkdownGeneratedRegionState.Invalid or MarkdownGeneratedRegionState.Unavailable)
        {
            return [];
        }

        if (body.Start == 0 && body.Length > 0 && document.Source[0] == '\uFEFF')
        {
            body = new MarkdownTextSpan(1, body.Length - 1);
        }

        if (document.GeneratedRegion.RegionSpan is not { } generated || document.GeneratedRegion.OmissionSpan is not { } interior)
        {
            return [body];
        }

        var heading = document.Headings.First(heading => heading.Span.Start == generated.Start).Span;
        var spans = ImmutableArray.CreateBuilder<MarkdownTextSpan>();
        var cursor = body.Start;
        foreach (var excluded in new[] { heading, interior }.OrderBy(span => span.Start))
        {
            if (cursor < excluded.Start) spans.Add(new MarkdownTextSpan(cursor, excluded.Start - cursor));
            cursor = Math.Max(cursor, excluded.End);
        }

        if (cursor < body.End) spans.Add(new MarkdownTextSpan(cursor, body.End - cursor));
        return spans.ToImmutable();
    }

    private static ImmutableArray<RenameContentLine> ReadLines(MarkdownDocumentFacts document, ImmutableArray<MarkdownTextSpan> authoredSpans)
    {
        var lines = ImmutableArray.CreateBuilder<RenameContentLine>();
        var source = document.Source;
        var start = 0;
        while (start < source.Length)
        {
            var newline = source.IndexOf('\n', start);
            var end = newline < 0 ? source.Length : newline + 1;
            if (lines.Count == RenameContentComparer.MaximumLines) return default;
            var text = source[start..end];
            var authoredText = text.AsSpan();
            if (start == 0 && !authoredText.IsEmpty && authoredText[0] == '\uFEFF') authoredText = authoredText[1..];
            var authoredStart = start == 0 && source[0] == '\uFEFF' ? 1 : start;
            var isAuthored = !authoredText.IsWhiteSpace() && authoredSpans.Any(span => authoredStart >= span.Start && end <= span.End);
            var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal);
            lines.Add(new RenameContentLine(normalized, StrictUtf8.GetByteCount(normalized), isAuthored));
            start = end;
        }

        return lines.ToImmutable();
    }
}
