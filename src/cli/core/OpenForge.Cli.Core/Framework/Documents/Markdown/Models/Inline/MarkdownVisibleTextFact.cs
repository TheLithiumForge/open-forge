namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

internal sealed record MarkdownVisibleTextFact
{
    internal MarkdownVisibleTextFact(MarkdownTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(span);
        Span = span;
    }

    internal MarkdownTextSpan Span { get; }
}
