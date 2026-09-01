using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Projection;

internal static class FindProjectionFindingPolicy
{
    internal static void Add(
        ICollection<FindFinding> findings,
        FindProjectionFindingInput input)
        => findings.Add(Create(input));

    internal static IReadOnlyList<FindFinding> BuildMissingSections(
        FindMissingSectionFindingInput input)
    {
        var findings = new List<FindFinding>();
        foreach (var part in input.RequestedParts.Where(
                     value => value.Kind == FindContentPartKind.Section))
        {
            var sections = input.MatchProjections
                .Where(projection => projection.Part == FindContentPartKind.Section
                    && string.Equals(
                        projection.Name,
                        part.Name,
                        StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (sections.Length == 0
                || sections.Any(projection => projection.State != FindProjectionState.Missing))
            {
                continue;
            }

            findings.Add(Create(new FindProjectionFindingInput(
                FindFindingCode.ProjectionMissing,
                input.SourceIdentity,
                null,
                new FindRegion(
                    FindRegionKind.Section,
                    part.Name,
                    part.CanonicalValue),
                part.Kind,
                "The requested section is absent from every completely inspected source layer.")));
        }

        return findings;
    }

    internal static IReadOnlyList<FindFinding> Order(IEnumerable<FindFinding> findings)
        => findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadLayerRank(finding.Layer))
            .ThenBy(finding => finding.Region?.CanonicalValue, StringComparer.Ordinal)
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ToArray();

    internal static int ReadLayerRank(SourceLayerKind? layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            null => 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(layer),
                layer,
                "The Find layer kind is not defined."),
        };

    private static FindFinding Create(FindProjectionFindingInput input)
        => new(
            input.Code,
            FindDefinitions.ReadFindingStatus(input.Code),
            input.Part == FindContentPartKind.Section
                ? input.Region?.Name
                : input.Part.ToString().ToLowerInvariant(),
            input.Cause,
            null,
            null,
            input.Source,
            input.Layer?.Kind,
            input.Layer?.Path,
            input.Region,
            null,
            []);
}
