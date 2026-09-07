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
        => distinct.GroupBy(relink => relink.SourceLocation).Any(group => group.Count() != 1);
}
