namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

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
        VisibleText = visibleText.ToArray();
        OpaqueSpans = opaqueSpans.ToArray();
        Links = links.ToArray();
        Images = images.ToArray();
    }

    internal IReadOnlyList<MarkdownVisibleTextFact> VisibleText { get; }

    internal IReadOnlyList<MarkdownOpaqueSpan> OpaqueSpans { get; }

    internal IReadOnlyList<MarkdownLinkFact> Links { get; }

    internal IReadOnlyList<MarkdownLinkFact> Images { get; }
}
