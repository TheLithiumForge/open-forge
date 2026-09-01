using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindMatchingResultBuilder
{
    internal FindMatchingFacts Build(FindMatchingResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var findings = new List<FindFinding>();
        var coverage = FindCoverageState.Complete;
        foreach (var sourceMatch in input.SourceMatches)
        {
            findings.AddRange(sourceMatch.Findings);
            coverage = FindMatchingFindingPolicy.MergeCoverage(
                coverage,
                sourceMatch.Coverage);
        }

        if (input.CandidateCount is { } candidateCount
            && candidateCount != input.SourceMatches.Count)
        {
            coverage = FindMatchingFindingPolicy.MergeCoverage(
                coverage,
                FindCoverageState.Incomplete);
        }

        var matches = input.SourceMatches
            .Where(sourceMatch => sourceMatch.Match)
            .OrderBy(sourceMatch => sourceMatch.Source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(sourceMatch => sourceMatch.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .Select((sourceMatch, index) => new FindMatch(
                index + 1,
                sourceMatch.Source.Identity.AutomaticId,
                sourceMatch.Source.Identity.CanonicalBasePath,
                sourceMatch.Description,
                sourceMatch.Evidence,
                []))
            .ToArray();

        return new FindMatchingFacts(
            matches,
            FindMatchingFindingPolicy.Order(findings),
            coverage);
    }
}
