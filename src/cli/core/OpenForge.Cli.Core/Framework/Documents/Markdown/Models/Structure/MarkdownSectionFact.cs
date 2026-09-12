namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

internal sealed record MarkdownSectionFact
{
    internal MarkdownSectionFact(MarkdownHeadingFact heading, MarkdownTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(heading);
        ArgumentNullException.ThrowIfNull(span);
        if (span.Start != heading.Span.Start || span.End < heading.Span.End)
        {
            throw new ArgumentException("A Markdown section span must begin with and contain its heading span.", nameof(span));
        }

        Heading = heading;
        Span = span;
    }

    internal MarkdownHeadingFact Heading { get; }

    internal MarkdownTextSpan Span { get; }
}
