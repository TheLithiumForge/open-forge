using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Shared.Documents;

internal sealed record StripDocumentRewrite(ImmutableArray<byte> Bytes, ImmutableArray<StripRewritePart> Parts, string? Unsupported = null)
{
    internal bool Changed => !Parts.IsEmpty;
}

/// <summary>
/// Plans every selected part for one document against its original source, then
/// applies the non-overlapping edits from the end so spans never shift.
/// </summary>
internal sealed class StripDocumentEditor
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private readonly MarkdownDocumentParser _parser = new();

    internal StripDocumentRewrite Edit(StripDocument document, StripRequest request, StripLinkContext links)
    {
        var facts = _parser.Parse(document.Source);
        var planned = new List<StripTextEdit>();
        foreach (var edits in Read(document, request, links, facts))
        {
            if (edits.Unsupported is { } cause)
            {
                return new StripDocumentRewrite(document.Snapshot.Bytes, [], cause);
            }

            planned.AddRange(edits.Edits);
        }

        var ordered = planned.OrderBy(edit => edit.Start).ThenByDescending(edit => edit.End).ToList();
        var kept = new List<StripTextEdit>();
        foreach (var edit in ordered)
        {
            if (kept.Any(existing => existing.Contains(edit)))
            {
                continue;
            }

            if (kept.Count > 0 && kept[^1].End > edit.Start)
            {
                return new StripDocumentRewrite(document.Snapshot.Bytes, [], "Two planned edits overlap in this document.");
            }

            kept.Add(edit);
        }

        if (kept.Count == 0)
        {
            return new StripDocumentRewrite(document.Snapshot.Bytes, []);
        }

        var text = new StringBuilder(document.Source);
        for (var index = kept.Count - 1; index >= 0; index--)
        {
            var edit = kept[index];
            text.Remove(edit.Start, edit.End - edit.Start).Insert(edit.Start, edit.Replacement);
        }

        var bytes = Utf8.GetBytes(text.ToString());
        if (bytes.AsSpan().SequenceEqual(document.Snapshot.Bytes.AsSpan()))
        {
            return new StripDocumentRewrite(document.Snapshot.Bytes, []);
        }

        var parts = kept.Select(edit => edit.Part).Distinct().Order().ToImmutableArray();
        return new StripDocumentRewrite([.. bytes], parts);
    }

    private static IEnumerable<StripDocumentEdits> Read(
        StripDocument document,
        StripRequest request,
        StripLinkContext links,
        MarkdownDocumentFacts facts)
    {
        if (request.Includes(StripPart.Metadata))
        {
            yield return StripMetadataEdits.Read(document.Path, facts);
        }

        if (request.Includes(StripPart.Entries))
        {
            yield return StripEntriesEdits.Read(facts);
        }

        yield return StripLinkEdits.Read(links, facts);
    }
}
