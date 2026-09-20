namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

internal enum MarkdownGeneratedRegionState
{
    Absent,
    Complete,
    Invalid,
    Unavailable,
}

internal enum MarkdownGeneratedRegionInvalidKind
{
    Malformed,
    Misplaced,
    Duplicate,
}

internal sealed record MarkdownGeneratedRegionFact
{
    private MarkdownGeneratedRegionFact(
        MarkdownGeneratedRegionState state,
        MarkdownTextSpan? regionSpan,
        MarkdownTextSpan? contentSpan,
        MarkdownEntriesBlock? entriesBlock,
        string? cause,
        MarkdownGeneratedRegionInvalidKind? invalidKind = null)
    {
        var omissionSpan = entriesBlock?.Span;
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

        if ((state == MarkdownGeneratedRegionState.Invalid) != (invalidKind is not null))
        {
            throw new ArgumentException("Only an invalid generated region carries one exact invalid kind.", nameof(invalidKind));
        }

        if (regionSpan is not null && contentSpan is not null
            && (contentSpan.Start < regionSpan.Start || contentSpan.End > regionSpan.End))
        {
            throw new ArgumentException("Generated Markdown content must be contained by its heading section.", nameof(contentSpan));
        }

        if (regionSpan is not null && omissionSpan is not null
            && (omissionSpan.Start < regionSpan.Start || omissionSpan.End > regionSpan.End))
        {
            throw new ArgumentException("Generated Markdown omission must be contained by its heading section.", nameof(omissionSpan));
        }

        if (contentSpan is not null && omissionSpan is not null
            && (omissionSpan.Start < contentSpan.Start || omissionSpan.End > contentSpan.End))
        {
            throw new ArgumentException("Generated Markdown omission must be contained by its content.", nameof(omissionSpan));
        }

        State = state;
        RegionSpan = regionSpan;
        ContentSpan = contentSpan;
        EntriesBlock = entriesBlock;
        Cause = cause;
        InvalidKind = invalidKind;
    }

    internal MarkdownGeneratedRegionState State { get; }

    internal MarkdownTextSpan? RegionSpan { get; }

    internal MarkdownTextSpan? ContentSpan { get; }

    internal MarkdownEntriesBlock? EntriesBlock { get; }

    internal MarkdownTextSpan? OmissionSpan => EntriesBlock?.Span;

    internal string? Cause { get; }

    internal MarkdownGeneratedRegionInvalidKind? InvalidKind { get; }

    internal static MarkdownGeneratedRegionFact Absent()
        => new(
            state: MarkdownGeneratedRegionState.Absent,
            regionSpan: null,
            contentSpan: null,
            entriesBlock: null,
            cause: null);

    internal static MarkdownGeneratedRegionFact Complete(
        MarkdownTextSpan regionSpan,
        MarkdownTextSpan contentSpan,
        MarkdownEntriesBlock entriesBlock)
        => new(
            state: MarkdownGeneratedRegionState.Complete,
            regionSpan: regionSpan,
            contentSpan: contentSpan,
            entriesBlock: entriesBlock,
            cause: null);

    internal static MarkdownGeneratedRegionFact Invalid(
        MarkdownGeneratedRegionInvalidKind kind,
        string cause)
        => new(
            state: MarkdownGeneratedRegionState.Invalid,
            regionSpan: null,
            contentSpan: null,
            entriesBlock: null,
            cause: cause,
            invalidKind: kind);

    internal static MarkdownGeneratedRegionFact Unavailable(string cause)
        => new(
            state: MarkdownGeneratedRegionState.Unavailable,
            regionSpan: null,
            contentSpan: null,
            entriesBlock: null,
            cause: cause);
}
