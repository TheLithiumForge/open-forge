namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownHeadingForm
{
    Atx,
    Setext,
}

internal sealed record MarkdownHeadingFact
{
    internal MarkdownHeadingFact(
        string visibleText,
        int level,
        MarkdownHeadingForm form,
        bool isCanonical,
        MarkdownTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(visibleText);
        if (level is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, "A Markdown heading level must be between 1 and 6.");
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The Markdown heading form is not defined.");
        }

        if (isCanonical && form != MarkdownHeadingForm.Atx)
        {
            throw new ArgumentException("Only an ATX Markdown heading can be canonical.", nameof(isCanonical));
        }

        ArgumentNullException.ThrowIfNull(span);
        VisibleText = visibleText;
        Level = level;
        Form = form;
        IsCanonical = isCanonical;
        Span = span;
    }

    internal string VisibleText { get; }

    internal int Level { get; }

    internal MarkdownHeadingForm Form { get; }

    internal bool IsCanonical { get; }

    internal MarkdownTextSpan Span { get; }
}
