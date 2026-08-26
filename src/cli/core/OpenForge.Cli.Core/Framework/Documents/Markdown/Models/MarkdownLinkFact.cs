namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownLinkForm
{
    Inline,
    Reference,
    Autolink,
}

internal sealed record MarkdownLinkFact
{
    internal MarkdownLinkFact(
        MarkdownLinkForm form,
        string rawDestination,
        MarkdownTextSpan span,
        MarkdownTextSpan? destinationSpan)
    {
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Markdown link form is not defined.");
        }

        ArgumentNullException.ThrowIfNull(rawDestination);
        ArgumentNullException.ThrowIfNull(span);
        if (form == MarkdownLinkForm.Autolink && destinationSpan is not null)
        {
            throw new ArgumentException("An explicit Markdown autolink has no independent destination span.", nameof(destinationSpan));
        }

        Form = form;
        RawDestination = rawDestination;
        Span = span;
        DestinationSpan = destinationSpan;
    }

    internal MarkdownLinkForm Form { get; }

    internal string RawDestination { get; }

    internal MarkdownTextSpan Span { get; }

    internal MarkdownTextSpan? DestinationSpan { get; }
}
