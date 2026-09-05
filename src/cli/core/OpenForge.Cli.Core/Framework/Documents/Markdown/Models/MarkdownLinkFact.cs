namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownLinkForm
{
    Inline,
    Reference,
    Autolink,
}

internal sealed record MarkdownLinkSyntax
{
    internal MarkdownLinkSyntax(MarkdownLinkForm form, bool isImage)
    {
        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Markdown link form is not defined.");
        }

        if (form == MarkdownLinkForm.Autolink && isImage)
        {
            throw new ArgumentException(
                "A Markdown autolink cannot be an image.",
                nameof(isImage));
        }

        Form = form;
        IsImage = isImage;
    }

    internal MarkdownLinkForm Form { get; }

    internal bool IsImage { get; }
}

internal sealed record MarkdownLinkFact
{
    internal MarkdownLinkFact(
        MarkdownLinkSyntax syntax,
        string rawDestination,
        MarkdownTextSpan span,
        MarkdownTextSpan? destinationSpan,
        MarkdownLinkLabelFact label)
    {
        ArgumentNullException.ThrowIfNull(syntax);
        ArgumentNullException.ThrowIfNull(rawDestination);
        ArgumentNullException.ThrowIfNull(span);
        ArgumentNullException.ThrowIfNull(label);
        if (syntax.Form == MarkdownLinkForm.Autolink && destinationSpan is not null)
        {
            throw new ArgumentException("An explicit Markdown autolink has no independent destination span.", nameof(destinationSpan));
        }

        Syntax = syntax;
        RawDestination = rawDestination;
        Span = span;
        DestinationSpan = destinationSpan;
        Label = label;
    }

    internal MarkdownLinkSyntax Syntax { get; }

    internal MarkdownLinkForm Form => Syntax.Form;

    internal bool IsImage => Syntax.IsImage;

    internal string RawDestination { get; }

    internal MarkdownTextSpan Span { get; }

    internal MarkdownTextSpan? DestinationSpan { get; }

    internal MarkdownLinkLabelFact Label { get; }
}
