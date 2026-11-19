using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

namespace OpenForge.Cli.Core.Framework.Sources.Models.References;

internal sealed record SourceAuthoredLink(MarkdownLinkFact Link, bool IsComment = false)
{
    internal string RawDestination => Link.RawDestination;
    internal MarkdownTextSpan Span => Link.Span;
    internal MarkdownTextSpan? DestinationSpan => Link.DestinationSpan;
    internal MarkdownLinkLabelFact Label => Link.Label;
}
