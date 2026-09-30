namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

internal enum MarkdownManagedHostState
{
    Absent,
    Present,
    Invalid,
}

internal sealed record MarkdownManagedHostFact
{
    private MarkdownManagedHostFact(
        MarkdownManagedHostState state,
        MarkdownTextSpan? span,
        bool isLegacy,
        string? cause)
    {
        State = state;
        Span = span;
        IsLegacy = isLegacy;
        Cause = cause;
    }

    internal MarkdownManagedHostState State { get; }

    internal MarkdownTextSpan? Span { get; }

    internal bool IsLegacy { get; }

    internal string? Cause { get; }

    internal static MarkdownManagedHostFact Absent()
        => new(MarkdownManagedHostState.Absent, span: null, isLegacy: false, cause: null);

    internal static MarkdownManagedHostFact Present(MarkdownTextSpan span, bool isLegacy)
        => new(MarkdownManagedHostState.Present, span, isLegacy, cause: null);

    internal static MarkdownManagedHostFact Invalid(string cause)
        => new(MarkdownManagedHostState.Invalid, span: null, isLegacy: false, cause);
}
