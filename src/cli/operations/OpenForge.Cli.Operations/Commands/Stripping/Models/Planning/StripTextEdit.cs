namespace OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

/// <summary>One replacement of the source characters from Start up to End.</summary>
internal sealed record StripTextEdit(int Start, int End, string Replacement, StripRewritePart Part)
{
    internal bool IsRemoval => Replacement.Length == 0;

    internal bool Contains(StripTextEdit other) => IsRemoval && Start <= other.Start && other.End <= End;
}

/// <summary>The edits one part plans for a document, or why the document cannot be edited safely.</summary>
internal sealed record StripDocumentEdits(IReadOnlyList<StripTextEdit> Edits, string? Unsupported = null)
{
    internal static StripDocumentEdits None { get; } = new([]);

    internal static StripDocumentEdits Refuse(string cause) => new([], cause);
}
