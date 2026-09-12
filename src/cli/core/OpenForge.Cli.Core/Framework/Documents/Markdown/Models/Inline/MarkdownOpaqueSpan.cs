namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

internal sealed record MarkdownOpaqueSpan
{
    internal MarkdownOpaqueSpan(MarkdownTextSpan span, bool isCode = false)
    {
        ArgumentNullException.ThrowIfNull(span);
        Span = span;
        IsCode = isCode;
    }

    internal MarkdownTextSpan Span { get; }

    internal bool IsCode { get; }
}
