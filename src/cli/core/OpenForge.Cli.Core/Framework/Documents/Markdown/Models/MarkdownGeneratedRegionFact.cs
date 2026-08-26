namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownGeneratedRegionState
{
    Absent,
    Complete,
    Unavailable,
}

internal sealed record MarkdownGeneratedRegionFact
{
    private MarkdownGeneratedRegionFact(
        MarkdownGeneratedRegionState state,
        MarkdownTextSpan? regionSpan,
        MarkdownTextSpan? contentSpan,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown generated-region state is not defined.");
        }

        if (state == MarkdownGeneratedRegionState.Absent
            && (regionSpan is not null || contentSpan is not null || cause is not null)
            || state == MarkdownGeneratedRegionState.Complete
                && (regionSpan is null || contentSpan is null || cause is not null)
            || state == MarkdownGeneratedRegionState.Unavailable
                && (regionSpan is not null || contentSpan is not null || string.IsNullOrWhiteSpace(cause)))
        {
            throw new ArgumentException("The Markdown generated-region facts do not match their state.");
        }

        if (regionSpan is not null && contentSpan is not null
            && (contentSpan.Start < regionSpan.Start || contentSpan.End > regionSpan.End))
        {
            throw new ArgumentException("Generated Markdown content must be contained by its marker region.", nameof(contentSpan));
        }

        State = state;
        RegionSpan = regionSpan;
        ContentSpan = contentSpan;
        Cause = cause;
    }

    internal MarkdownGeneratedRegionState State { get; }

    internal MarkdownTextSpan? RegionSpan { get; }

    internal MarkdownTextSpan? ContentSpan { get; }

    internal string? Cause { get; }

    internal static MarkdownGeneratedRegionFact Absent()
        => new(MarkdownGeneratedRegionState.Absent, null, null, null);

    internal static MarkdownGeneratedRegionFact Complete(
        MarkdownTextSpan regionSpan,
        MarkdownTextSpan contentSpan)
        => new(MarkdownGeneratedRegionState.Complete, regionSpan, contentSpan, null);

    internal static MarkdownGeneratedRegionFact Unavailable(string cause)
        => new(MarkdownGeneratedRegionState.Unavailable, null, null, cause);
}
