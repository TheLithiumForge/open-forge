using OpenForge.Cli.Core.Commands.Repair.Models.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Request;

internal static class RepairRelinkNormalizer
{
    internal static RepairRelinkRequest[] Distinct(IEnumerable<RepairRelinkRequest> relinks)
        => [.. relinks.DistinctBy(relink => (
            relink.SourceLocation,
            relink.ExpectedDestination,
            relink.SelectedTargetPath,
            relink.SelectedTargetFragment))];

    internal static bool HasContradictions(IReadOnlyList<RepairRelinkRequest> distinct)
        => Contradiction(distinct) is not null;

    // The finding has to name the link it is refusing, so the first occurrence that two values
    // disagree about is returned rather than a bare yes or no.
    internal static RepairRelinkRequest? Contradiction(IReadOnlyList<RepairRelinkRequest> distinct)
        => distinct.GroupBy(relink => relink.SourceLocation)
            .FirstOrDefault(group => group.Count() != 1)?.First();
}
