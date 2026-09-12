namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;

internal enum MarkdownLinkLabelState
{
    Supported,
    Unsupported,
}

internal sealed record MarkdownLinkLabelFact
{
    private MarkdownLinkLabelFact(MarkdownLinkLabelState state, string? text)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown link label state is not defined.");
        }

        if (state == MarkdownLinkLabelState.Supported && string.IsNullOrWhiteSpace(text)
            || state == MarkdownLinkLabelState.Unsupported && text is not null)
        {
            throw new ArgumentException("The Markdown link label text must match its state.");
        }

        State = state;
        Text = text;
    }

    internal MarkdownLinkLabelState State { get; }

    internal string? Text { get; }

    internal static MarkdownLinkLabelFact Supported(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return new MarkdownLinkLabelFact(MarkdownLinkLabelState.Supported, text);
    }

    internal static MarkdownLinkLabelFact Unsupported()
        => new(MarkdownLinkLabelState.Unsupported, null);
}
