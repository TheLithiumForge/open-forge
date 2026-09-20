namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

internal sealed class MarkdownInlineFactCollections
{
    internal List<MarkdownVisibleTextFact> VisibleText { get; } = [];

    internal List<MarkdownOpaqueSpan> OpaqueSpans { get; } = [];

    internal List<MarkdownLinkFact> Links { get; } = [];

    internal List<MarkdownLinkFact> Images { get; } = [];
}
