using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal static class FindMatchingFindingPolicy
{
    internal static FindFinding Create(FindMatchingFindingInput input)
        => new(
            input.Code,
            FindDefinitions.ReadFindingStatus(input.Code),
            input.Region?.Name,
            input.Cause,
            null,
            null,
            new FindSourceIdentity(
                input.Source.Identity.AutomaticId,
                input.Source.Identity.CanonicalBasePath),
            input.Layer.Kind,
            input.Layer.CanonicalPath,
            input.Region,
            null,
            []);

    internal static void Add(ICollection<FindFinding> findings, FindFinding finding)
    {
        if (findings.Any(existing =>
                existing.Code == finding.Code
                && existing.Layer == finding.Layer
                && string.Equals(existing.Path, finding.Path, StringComparison.Ordinal)
                && string.Equals(existing.Region?.CanonicalValue, finding.Region?.CanonicalValue, StringComparison.Ordinal)
                && string.Equals(existing.Source?.Id, finding.Source?.Id, StringComparison.Ordinal)))
        {
            return;
        }

        findings.Add(finding);
    }

    internal static IReadOnlyList<FindFinding> Order(IEnumerable<FindFinding> findings)
        => findings
            .OrderBy(finding => (int)finding.Code)
            .ThenBy(finding => finding.SelectorRole is FindSelectorRole.Include ? 0 : 1)
            .ThenBy(finding => finding.SelectorOccurrence ?? int.MaxValue)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadNullableLayerRank(finding.Layer))
            .ThenBy(finding => ReadNullableRegionRank(finding.Region))
            .ThenBy(finding => finding.Region?.CanonicalValue, StringComparer.Ordinal)
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();

    internal static FindCoverageState ReadCoverage(FindFindingCode code)
        => code switch
        {
            FindFindingCode.OperationFailed => FindCoverageState.Failed,
            FindFindingCode.Interrupted => FindCoverageState.Interrupted,
            FindFindingCode.WorkspaceUnavailable
                or FindFindingCode.WorkspaceUnsafe
                or FindFindingCode.SelectorAmbiguous
                or FindFindingCode.SelectorUnsafe => FindCoverageState.Blocked,
            FindFindingCode.CandidateUnsafe
                or FindFindingCode.LayerUnresolved
                or FindFindingCode.InspectionUnavailable
                or FindFindingCode.InvalidEncoding
                or FindFindingCode.FrontmatterUnavailable
                or FindFindingCode.SectionAmbiguous
                or FindFindingCode.ProjectionUnavailable => FindCoverageState.Incomplete,
            FindFindingCode.InvalidInput
                or FindFindingCode.InvalidSelector
                or FindFindingCode.IdentityCollision
                or FindFindingCode.ProjectionMissing => FindCoverageState.Complete,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Find finding code is not defined."),
        };

    internal static FindCoverageState MergeCoverage(
        FindCoverageState current,
        FindCoverageState candidate)
        => ReadCoverageRank(candidate) > ReadCoverageRank(current) ? candidate : current;

    internal static int ReadRegionRank(FindRegion region)
        => region.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body => 1,
            FindRegionKind.Section => 2,
            FindRegionKind.Document => throw new ArgumentException(
                "A Find evidence region cannot be document.",
                nameof(region)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(region),
                region.Kind,
                "The Find region kind is not defined."),
        };

    internal static int ReadLayerRank(SourceLayerKind layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            _ => throw new ArgumentOutOfRangeException(
                nameof(layer),
                layer,
                "The Find layer kind is not defined."),
        };

    private static int ReadCoverageRank(FindCoverageState state)
        => state switch
        {
            FindCoverageState.Complete => 0,
            FindCoverageState.Incomplete => 1,
            FindCoverageState.Blocked => 2,
            FindCoverageState.Failed => 3,
            FindCoverageState.Interrupted => 4,
            FindCoverageState.NotStarted => 5,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find coverage state is not defined."),
        };

    private static int ReadNullableRegionRank(FindRegion? region)
        => region is null ? int.MaxValue : ReadRegionRank(region);

    private static int ReadNullableLayerRank(SourceLayerKind? layer)
        => layer is { } value ? ReadLayerRank(value) : int.MaxValue;
}
