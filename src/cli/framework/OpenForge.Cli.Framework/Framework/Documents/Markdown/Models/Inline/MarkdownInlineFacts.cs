namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

internal sealed record MarkdownInlineFacts
{
    internal MarkdownInlineFacts(
        IEnumerable<MarkdownVisibleTextFact> visibleText,
        IEnumerable<MarkdownOpaqueSpan> opaqueSpans,
        IEnumerable<MarkdownLinkFact> links,
        IEnumerable<MarkdownLinkFact> images)
    {
        ArgumentNullException.ThrowIfNull(visibleText);
        ArgumentNullException.ThrowIfNull(opaqueSpans);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(images);
        VisibleText = Array.AsReadOnly(visibleText.ToArray());
        OpaqueSpans = Array.AsReadOnly(opaqueSpans.ToArray());
        Links = Array.AsReadOnly(links.ToArray());
        Images = Array.AsReadOnly(images.ToArray());
    }

    internal IReadOnlyList<MarkdownVisibleTextFact> VisibleText { get; }

    internal IReadOnlyList<MarkdownOpaqueSpan> OpaqueSpans { get; }

    internal IReadOnlyList<MarkdownLinkFact> Links { get; }

    internal IReadOnlyList<MarkdownLinkFact> Images { get; }
}
