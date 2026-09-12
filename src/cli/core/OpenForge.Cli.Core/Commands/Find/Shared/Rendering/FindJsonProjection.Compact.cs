using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static partial class FindJsonProjection
{
    internal static FindCompactJsonResult CreateCompact(FindResult result)
        => new()
        {
            Universe = Universe(result.Universe, result.Coverage.Matching),
            Query = Query(result.Query),
            Presentation = Presentation(result.Presentation),
            Coverage = Coverage(result.Coverage),
            Findings = result.Findings.Select(Finding).ToArray(),
            Matches = result.Matches.Select(CompactMatch).ToArray(),
        };

    private static FindCompactJsonMatch CompactMatch(FindMatch match)
    {
        return new FindCompactJsonMatch
        {
            Position = match.Position,
            Id = match.Id,
            Path = match.Path,
            Description = match.Description,
            Projections = match.Projections.Select(Projection).ToArray(),
        };
    }
}
