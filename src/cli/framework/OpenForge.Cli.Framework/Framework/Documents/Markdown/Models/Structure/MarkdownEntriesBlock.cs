namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

// G4's shared Entries contract owns exactly the first top-level dash-space list.
internal sealed record MarkdownEntriesBlock(MarkdownTextSpan Span, string LineEnding)
{
    internal bool Exists => Span.Length != 0;
}
