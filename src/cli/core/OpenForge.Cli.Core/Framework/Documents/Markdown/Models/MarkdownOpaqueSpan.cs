namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal sealed record MarkdownOpaqueSpan
{
    internal MarkdownOpaqueSpan(MarkdownTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(span);
        Span = span;
    }

    internal MarkdownTextSpan Span { get; }
}
