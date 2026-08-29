using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Index.Models.Result;

internal sealed record IndexFinding
{
    private const int MaximumCauseLength = 256;

    internal IndexFinding(
        IndexFindingCode code,
        int? sourceOccurrence,
        IndexLogicalSource? source,
        string cause,
        IEnumerable<IndexLogicalSource> candidates)
    {
        if (sourceOccurrence is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sourceOccurrence), sourceOccurrence, "An Index source occurrence must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        ArgumentNullException.ThrowIfNull(candidates);
        var ordered = candidates
            .Select(candidate => candidate ?? throw new ArgumentException(
                "Index finding candidates cannot contain null members.",
                nameof(candidates)))
            .Distinct()
            .OrderBy(candidate => candidate.Path, StringComparer.Ordinal)
            .ThenBy(candidate => candidate.Id, StringComparer.Ordinal)
            .ToArray();
        Code = code;
        Status = IndexDefinitions.ReadStatus(code);
        SourceOccurrence = sourceOccurrence;
        Source = source;
        Cause = cause.Length <= MaximumCauseLength ? cause : cause[..MaximumCauseLength];
        Candidates = new ReadOnlyCollection<IndexLogicalSource>(ordered);
    }

    internal IndexFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal int? SourceOccurrence { get; }

    internal IndexLogicalSource? Source { get; }

    internal string Cause { get; }

    internal IReadOnlyList<IndexLogicalSource> Candidates { get; }
}
