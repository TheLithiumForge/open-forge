using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindSourceMatcher(FindPredicateMatcher predicateMatcher)
{
    private readonly FindPredicateMatcher _predicateMatcher = predicateMatcher;

    internal FindSourceMatchFacts Match(FindSourceMatchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var request = input.Request;
        var layerFacts = input.LayerFacts;
        var predicates = request.Query.EffectivePredicates;
        if (predicates.Count == 0)
        {
            var hasUnavailableLayer = layerFacts.Layers.Any(
                layer => layer.Inspection?.Document?.BodySpan is null);
            var bareCoverage = hasUnavailableLayer
                ? FindCoverageState.Incomplete
                : FindCoverageState.Complete;
            return new FindSourceMatchFacts(
                layerFacts.Source,
                layerFacts.Description,
                true,
                [],
                layerFacts.Findings,
                FindMatchingFindingPolicy.MergeCoverage(
                    layerFacts.Coverage,
                    bareCoverage));
        }

        var evidence = Enumerable.Range(0, predicates.Count)
            .Select(_ => new List<FindEvidenceCandidate>())
            .ToArray();
        var unknown = new bool[predicates.Count];
        var predicateFindings = new List<FindFinding>();
        foreach (var layer in layerFacts.Layers)
        {
            if (layer.Inspection?.Document?.BodySpan is null)
            {
                Array.Fill(unknown, true);
                continue;
            }

            for (var predicateIndex = 0; predicateIndex < predicates.Count; predicateIndex++)
            {
                var predicate = predicates[predicateIndex];
                var layerResult = _predicateMatcher.Match(new FindPredicateMatchInput(
                    predicate,
                    predicateIndex + 1,
                    request.Query.Within,
                    layer));
                unknown[predicateIndex] |= layerResult.Unknown;
                evidence[predicateIndex].AddRange(layerResult.Evidence);
                foreach (var finding in layerResult.Findings)
                {
                    FindMatchingFindingPolicy.Add(predicateFindings, finding);
                }
            }
        }

        var predicateMatches = evidence.Select(values => values.Count > 0).ToArray();
        var matches = request.Query.Requirement == FindRequirement.All
            ? predicateMatches.All(value => value)
            : predicateMatches.Any(value => value);
        var matchingCoverage = unknown.Any(value => value)
            ? FindCoverageState.Incomplete
            : FindCoverageState.Complete;
        var orderedEvidence = evidence
            .SelectMany(values => values)
            .OrderBy(value => value.Predicate)
            .ThenBy(value => FindMatchingFindingPolicy.ReadRegionRank(value.Region))
            .ThenBy(value => FindMatchingFindingPolicy.ReadLayerRank(value.Layer.Kind))
            .ThenBy(value => value.Location.ByteOffset)
            .ThenBy(value => value.Occurrence)
            .ThenBy(value => value.Region.CanonicalValue, StringComparer.Ordinal)
            .Select(value => value.Create())
            .ToArray();
        var findings = layerFacts.Findings.Concat(predicateFindings).ToArray();

        return new FindSourceMatchFacts(
            layerFacts.Source,
            layerFacts.Description,
            matches,
            orderedEvidence,
            findings,
            FindMatchingFindingPolicy.MergeCoverage(
                layerFacts.Coverage,
                matchingCoverage));
    }
}
