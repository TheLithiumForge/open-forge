namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

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
        string? fragmentIdentifier,
        MarkdownTextSpan span,
        bool isTopLevel = true)
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

        if (fragmentIdentifier is not null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fragmentIdentifier);
            if (!isCanonical)
            {
                throw new ArgumentException("Only a canonical ATX heading can establish a fragment identifier.", nameof(fragmentIdentifier));
            }
        }

        ArgumentNullException.ThrowIfNull(span);
        VisibleText = visibleText;
        Level = level;
        Form = form;
        IsCanonical = isCanonical;
        FragmentIdentifier = fragmentIdentifier;
        Span = span;
        IsTopLevel = isTopLevel;
    }

    internal bool IsTopLevel { get; }

    internal string VisibleText { get; }

    internal int Level { get; }

    internal MarkdownHeadingForm Form { get; }

    internal bool IsCanonical { get; }

    internal string? FragmentIdentifier { get; }

    internal MarkdownTextSpan Span { get; }
}
