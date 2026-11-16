using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Shared.Documents;

/// <summary>
/// Removes the generated Entries section. User text in the section keeps its heading.
/// The strip Interface contract's entries part defines the boundary.
/// </summary>
internal static class StripEntriesEdits
{
    internal static StripDocumentEdits Read(MarkdownDocumentFacts facts)
    {
        var region = facts.GeneratedRegion;
        if (region.State == MarkdownGeneratedRegionState.Absent)
        {
            return StripDocumentEdits.None;
        }

        if (region is not { State: MarkdownGeneratedRegionState.Complete, RegionSpan: { } section, ContentSpan: { } content, EntriesBlock: { } block })
        {
            return StripDocumentEdits.Refuse(region.Cause ?? "The Entries section is malformed or duplicated.");
        }

        var source = facts.Source;
        var userText = !StripSourceLines.IsBlank(source, content.Start, block.Span.Start)
            || !StripSourceLines.IsBlank(source, block.Span.End, content.End);
        if (userText)
        {
            return block.Exists
                ? new StripDocumentEdits([new StripTextEdit(block.Span.Start, block.Span.End, string.Empty, StripRewritePart.Entries)])
                : StripDocumentEdits.None;
        }

        var start = section.Start;
        var end = section.End;
        if (StripSourceLines.IsBlank(source, end, source.Length))
        {
            end = source.Length;
            start = StripSourceLines.BackOverBlankLines(source, start, facts.BodySpan?.Start ?? 0);
        }

        return new StripDocumentEdits([new StripTextEdit(start, end, string.Empty, StripRewritePart.Entries)]);
    }
}
