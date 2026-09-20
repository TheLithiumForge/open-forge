using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Models.Matching;

internal sealed record FindSourceMatchInput(
    FindRequest Request,
    FindSourceLayerFacts LayerFacts);

internal sealed record FindSourceMatchFacts
{
    internal FindSourceMatchFacts(
        SourceLogicalSource source,
        string? description,
        bool match,
        IEnumerable<FindEvidence> evidence,
        IEnumerable<FindFinding> findings,
        FindCoverageState coverage)
    {
        Source = source;
        Description = description;
        Match = match;
        Evidence = Array.AsReadOnly(evidence.ToArray());
        Findings = Array.AsReadOnly(findings.ToArray());
        Coverage = coverage;
    }

    internal SourceLogicalSource Source { get; }

    internal string? Description { get; }

    internal bool Match { get; }

    internal IReadOnlyList<FindEvidence> Evidence { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal FindCoverageState Coverage { get; }
}

internal sealed record FindMatchingResultInput
{
    internal FindMatchingResultInput(
        IEnumerable<FindSourceMatchFacts> sourceMatches,
        int? candidateCount)
    {
        SourceMatches = Array.AsReadOnly(sourceMatches.ToArray());
        CandidateCount = candidateCount;
    }

    internal IReadOnlyList<FindSourceMatchFacts> SourceMatches { get; }

    internal int? CandidateCount { get; }
}
