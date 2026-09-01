using OpenForge.Cli.Core.Commands.Find.Models.Matching;
namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindMatcher
{
    private readonly FindLayerFactsBuilder _layerFactsBuilder = new();
    private readonly FindSourceMatcher _sourceMatcher = new(new FindPredicateMatcher());
    private readonly FindMatchingResultBuilder _resultBuilder = new();

    internal FindMatchingFacts Match(FindMatchingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var sourceGroups = input.Inspections
            .GroupBy(inspection => inspection.Source.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .OrderBy(group => group.First().Source.Identity.AutomaticId, StringComparer.Ordinal)
            .ThenBy(group => group.Key, StringComparer.Ordinal);
        var sourceMatches = new List<FindSourceMatchFacts>();
        foreach (var sourceGroup in sourceGroups)
        {
            var source = sourceGroup.First().Source;
            var layerFacts = _layerFactsBuilder.Build(new FindLayerFactsInput(source, sourceGroup));
            sourceMatches.Add(_sourceMatcher.Match(new FindSourceMatchInput(
                input.Request,
                layerFacts)));
        }

        return _resultBuilder.Build(new FindMatchingResultInput(
            sourceMatches,
            input.Universe.CandidateCount));
    }
}
