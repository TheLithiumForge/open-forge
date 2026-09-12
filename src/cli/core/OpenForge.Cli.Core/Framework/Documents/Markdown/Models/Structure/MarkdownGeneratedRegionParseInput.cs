using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

internal sealed record MarkdownGeneratedRegionParseInput(
    string Source,
    MarkdownTextSpan BodySpan,
    IReadOnlyList<MarkdownHeadingFact> Headings,
    IReadOnlyList<MarkdownOpaqueSpan> OpaqueSpans);
