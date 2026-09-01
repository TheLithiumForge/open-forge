using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultUniverseBuilder
{
    private readonly FindResultInspectedCountBuilder _countBuilder = new();

    internal FindUniverse Build(FindResultUniverseInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Universe is null)
        {
            var include = input.Request.UniverseFilter.Include
                .Select(CreateUnresolvedSelector)
                .ToArray();
            var exclude = input.Request.UniverseFilter.Exclude
                .Select(CreateUnresolvedSelector)
                .ToArray();
            var mode = include.Length == 0 && exclude.Length == 0
                ? FindUniverseMode.Default
                : FindUniverseMode.Filtered;
            return new FindUniverse(
                mode,
                include,
                exclude,
                null,
                null,
                input.MatchCount == 0 ? null : input.MatchCount);
        }

        var inspectedCount = input.Universe.InspectedCount
            ?? _countBuilder.Read(new FindResultInspectedCountInput(
                input.Request,
                input.Inspections,
                input.Universe.CandidateCount,
                input.MatchingCoverage));
        return new FindUniverse(
            input.Universe.Mode,
            input.Universe.Include,
            input.Universe.Exclude,
            input.Universe.CandidateCount,
            inspectedCount,
            input.MatchCount);
    }

    private static FindSelector CreateUnresolvedSelector(string value)
        => new(
            value,
            null,
            FindSelectorResolution.Invalid,
            null,
            null,
            null,
            []);
}
