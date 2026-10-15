using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;

internal sealed record RenameContent
{
    public required ImmutableArray<byte> Bytes { get; init; }
    public required string Hash { get; init; }
    public required string NormalizedHash { get; init; }
    public required int NormalizedByteLength { get; init; }
    public required MarkdownDocumentFacts Document { get; init; }
    public required bool HasAuthoredContent { get; init; }
    public required ImmutableArray<RenameContentLine> Lines { get; init; }
}

internal sealed record RenameContentLine(string Text, int ByteLength, bool IsAuthored);

internal sealed record RenameContentComparison(int? Similarity, bool SameContent, bool HasCommonAuthoredLine)
{
    internal bool IsStrong => Similarity >= RenameContentComparer.MinimumSimilarity && HasCommonAuthoredLine;
}
