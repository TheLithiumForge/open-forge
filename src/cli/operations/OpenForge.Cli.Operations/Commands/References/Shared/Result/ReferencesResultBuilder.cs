using OpenForge.Cli.Core.Commands.References.Models.Inspection;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Result;

internal sealed class ReferencesResultBuilder
{
    internal ReferencesResult Build(ReferencesResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(input.Request);
        ArgumentNullException.ThrowIfNull(input.IncomingOccurrences);
        ArgumentNullException.ThrowIfNull(input.OutgoingOccurrences);
        ArgumentNullException.ThrowIfNull(input.Findings);

        var findings = input.Findings
            .Select(finding => finding ?? throw new ArgumentException("References findings cannot contain null members.", nameof(input)))
            .OrderBy(finding => (int)finding.Code)
            .ThenBy(finding => ReadFindingScope(finding))
            .ThenBy(finding => finding.SelectorRole == SourceUniverseSelectorRole.Include ? 0 : 1)
            .ThenBy(finding => finding.SelectorOccurrence ?? int.MaxValue)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadLayerRank(finding.Layer))
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ThenBy(finding => finding.DestinationLocation?.ByteOffset ?? long.MaxValue)
            .ThenBy(
                finding => string.Join(
                    "\u001f",
                    finding.Candidates.Select(candidate => $"{candidate.Id}\u001e{candidate.Path}")),
                StringComparer.Ordinal)
            .ToArray();

        var incoming = input.Request.RequestedDirection is ReferencesDirection.In or ReferencesDirection.Both
            ? new ReferencesSection(
                ReadSectionCoverage(input.IncomingCoverage, findings, ReferencesDirection.In),
                ReadSectionStatus(input.IncomingCoverage, findings, ReferencesDirection.In),
                SortIncoming(input.IncomingOccurrences))
            : null;
        var outgoing = input.Request.RequestedDirection is ReferencesDirection.Out or ReferencesDirection.Both
            ? new ReferencesSection(
                ReadSectionCoverage(input.OutgoingCoverage, findings, ReferencesDirection.Out),
                ReadSectionStatus(input.OutgoingCoverage, findings, ReferencesDirection.Out),
                SortOutgoing(input.OutgoingOccurrences))
            : null;

        var status = ReadAggregateStatus(findings, incoming, outgoing, input.Request.RequestedDirection);
        var next = ReferencesDefinitions.ReadNextAction(status, findings);
        return new ReferencesResult(
            input.Request.Workspace,
            input.Source,
            input.Request.RequestedDirection,
            input.IncomingSelection,
            incoming,
            outgoing,
            findings,
            status,
            next);
    }

    private static ReferencesCoverage ReadSectionCoverage(
        ReferencesCoverage requestedCoverage,
        IReadOnlyList<ReferencesFinding> findings,
        ReferencesDirection direction)
    {
        var relevant = findings.Where(finding => finding.Direction is null || finding.Direction == direction);
        if (relevant.Any(finding => finding.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Invalid))
        {
            return ReferencesCoverage.Blocked;
        }

        if (requestedCoverage == ReferencesCoverage.Blocked)
        {
            return ReferencesCoverage.Blocked;
        }

        if (requestedCoverage == ReferencesCoverage.Incomplete
            || relevant.Any(finding => finding.Status is CliSemanticStatus.Incomplete or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted))
        {
            return ReferencesCoverage.Incomplete;
        }

        return ReferencesCoverage.Complete;
    }

    private static CliSemanticStatus ReadSectionStatus(
        ReferencesCoverage requestedCoverage,
        IReadOnlyList<ReferencesFinding> findings,
        ReferencesDirection direction)
    {
        var relevant = findings
            .Where(finding => finding.Direction is null || finding.Direction == direction)
            .Select(finding => finding.Status)
            .ToArray();
        if (relevant.Length == 0)
        {
            return requestedCoverage switch
            {
                ReferencesCoverage.Complete => CliSemanticStatus.Complete,
                ReferencesCoverage.Incomplete => CliSemanticStatus.Incomplete,
                ReferencesCoverage.Blocked => CliSemanticStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(requestedCoverage), requestedCoverage, "The References coverage is not defined."),
            };
        }

        return ReadMostSevere(relevant.Append(
            requestedCoverage switch
            {
                ReferencesCoverage.Complete => CliSemanticStatus.Complete,
                ReferencesCoverage.Incomplete => CliSemanticStatus.Incomplete,
                ReferencesCoverage.Blocked => CliSemanticStatus.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(requestedCoverage), requestedCoverage, "The References coverage is not defined."),
            }));
    }

    private static CliSemanticStatus ReadAggregateStatus(
        IReadOnlyList<ReferencesFinding> findings,
        ReferencesSection? incoming,
        ReferencesSection? outgoing,
        ReferencesDirection? requestedDirection)
    {
        if (requestedDirection is null)
        {
            return findings.Count == 0 ? CliSemanticStatus.Invalid : ReadMostSevere(findings.Select(finding => finding.Status));
        }

        var statuses = findings.Select(finding => finding.Status).ToList();
        if (incoming is not null)
        {
            statuses.Add(incoming.Status);
        }

        if (outgoing is not null)
        {
            statuses.Add(outgoing.Status);
        }

        return statuses.Count == 0 ? CliSemanticStatus.Complete : ReadMostSevere(statuses);
    }

    private static CliSemanticStatus ReadMostSevere(IEnumerable<CliSemanticStatus> statuses)
    {
        var values = statuses.ToArray();
        if (values.Any(status => status == CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (values.Any(status => status == CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (values.Any(status => status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (values.Any(status => status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (values.Any(status => status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        if (values.Any(status => status == CliSemanticStatus.Attention))
        {
            return CliSemanticStatus.Attention;
        }

        return CliSemanticStatus.Complete;
    }

    private static IEnumerable<ReferencesOccurrence> SortOutgoing(IEnumerable<ReferencesOccurrence> occurrences)
        => occurrences
            .Select(value => value ?? throw new ArgumentException("References occurrences cannot contain null members.", nameof(occurrences)))
            .OrderBy(value => ReadLayerRank(value.Source.Layer))
            .ThenBy(value => value.Location.ByteOffset)
            .ThenBy(value => value.DestinationLocation?.ByteOffset ?? long.MaxValue)
            .ToArray();

    private static IEnumerable<ReferencesOccurrence> SortIncoming(IEnumerable<ReferencesOccurrence> occurrences)
        => occurrences
            .Select(value => value ?? throw new ArgumentException("References occurrences cannot contain null members.", nameof(occurrences)))
            .OrderBy(value => value.Source.Id, StringComparer.Ordinal)
            .ThenBy(value => ReadLogicalBasePath(value.Source), StringComparer.Ordinal)
            .ThenBy(value => ReadLayerRank(value.Source.Layer))
            .ThenBy(value => value.Source.Path, StringComparer.Ordinal)
            .ThenBy(value => value.Location.ByteOffset)
            .ThenBy(value => value.DestinationLocation?.ByteOffset ?? long.MaxValue)
            .ToArray();

    private static string ReadLogicalBasePath(ReferencesOccurrenceSource source)
        => source.Layer == SourceLayerKind.Overwrite
            ? source.Path[..^".overwrite.md".Length] + ".md"
            : source.Path;

    private static int ReadFindingScope(ReferencesFinding finding)
    {
        if (finding.Direction is null && finding.SelectorRole is null)
        {
            return 0;
        }

        if (finding.SelectorRole is not null)
        {
            return 1;
        }

        return finding.Direction == ReferencesDirection.In ? 2 : 3;
    }

    internal static int ReadLayerRank(SourceLayerKind? layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            null => 2,
            _ => throw new ArgumentOutOfRangeException(
                nameof(layer),
                layer,
                "The source layer kind is not defined."),
        };
}
