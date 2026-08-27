namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

internal enum MarkdownGeneratedRegionState
{
    Absent,
    Complete,
    Invalid,
    Unavailable,
}

internal sealed record MarkdownGeneratedRegionFact
{
    private MarkdownGeneratedRegionFact(
        MarkdownGeneratedRegionState state,
        MarkdownTextSpan? regionSpan,
        MarkdownTextSpan? contentSpan,
        MarkdownTextSpan? omissionSpan,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown generated-region state is not defined.");
        }

        if (state == MarkdownGeneratedRegionState.Absent
            && (regionSpan is not null || contentSpan is not null || omissionSpan is not null || cause is not null)
            || state == MarkdownGeneratedRegionState.Complete
                && (regionSpan is null || contentSpan is null || omissionSpan is null || cause is not null)
            || state is MarkdownGeneratedRegionState.Invalid or MarkdownGeneratedRegionState.Unavailable
                && (regionSpan is not null || contentSpan is not null || omissionSpan is not null || string.IsNullOrWhiteSpace(cause)))
        {
            throw new ArgumentException("The Markdown generated-region facts do not match their state.");
        }

        if (regionSpan is not null && contentSpan is not null
            && (contentSpan.Start < regionSpan.Start || contentSpan.End > regionSpan.End))
        {
            throw new ArgumentException("Generated Markdown content must be contained by its marker region.", nameof(contentSpan));
        }

        if (regionSpan is not null && omissionSpan is not null
            && (omissionSpan.Start < regionSpan.Start || omissionSpan.End > regionSpan.End))
        {
            throw new ArgumentException("Generated Markdown omission must be contained by its marker region.", nameof(omissionSpan));
        }

        if (contentSpan is not null && omissionSpan is not null
            && (omissionSpan.Start < contentSpan.Start || omissionSpan.End > contentSpan.End))
        {
            throw new ArgumentException("Generated Markdown omission must be contained by its content.", nameof(omissionSpan));
        }

        State = state;
        RegionSpan = regionSpan;
        ContentSpan = contentSpan;
        OmissionSpan = omissionSpan;
        Cause = cause;
    }

    internal MarkdownGeneratedRegionState State { get; }

    internal MarkdownTextSpan? RegionSpan { get; }

    internal MarkdownTextSpan? ContentSpan { get; }

    internal MarkdownTextSpan? OmissionSpan { get; }

    internal string? Cause { get; }

    internal static MarkdownGeneratedRegionFact Absent()
        => new(
            state: MarkdownGeneratedRegionState.Absent,
            regionSpan: null,
            contentSpan: null,
            omissionSpan: null,
            cause: null);

    internal static MarkdownGeneratedRegionFact Complete(
        MarkdownTextSpan regionSpan,
        MarkdownTextSpan contentSpan,
        MarkdownTextSpan omissionSpan)
        => new(
            state: MarkdownGeneratedRegionState.Complete,
            regionSpan: regionSpan,
            contentSpan: contentSpan,
            omissionSpan: omissionSpan,
            cause: null);

    internal static MarkdownGeneratedRegionFact Invalid(string cause)
        => new(
            state: MarkdownGeneratedRegionState.Invalid,
            regionSpan: null,
            contentSpan: null,
            omissionSpan: null,
            cause: cause);

    internal static MarkdownGeneratedRegionFact Unavailable(string cause)
        => new(
            state: MarkdownGeneratedRegionState.Unavailable,
            regionSpan: null,
            contentSpan: null,
            omissionSpan: null,
            cause: cause);
}
