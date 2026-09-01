using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultFindingBuilder
{
    internal IReadOnlyList<FindFinding> Build(FindResultFindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var findings = new List<FindFinding>(input.Findings);
        foreach (var inspection in input.Inspections)
        {
            foreach (var finding in inspection.Findings)
            {
                if (!findings.Any(existing => ReferenceEquals(existing, finding)))
                {
                    findings.Add(finding);
                }
            }
        }

        AddProjectionFindings(findings, input.Matches, input.ContentRequested);
        AddStageFindings(findings, input);
        return Array.AsReadOnly(findings.ToArray());
    }

    internal IReadOnlyList<FindFinding> Finalize(FindResultFindingFinalizationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var findings = RetainAllowedFindings(input.Status, input.Findings);
        EnsureStatusFinding(
            input.Status,
            findings,
            input.TerminalEvent,
            input.ProjectionCoverage);
        return OrderFindings(findings);
    }

    private static void AddProjectionFindings(
        ICollection<FindFinding> findings,
        IReadOnlyList<FindMatch> matches,
        bool contentRequested)
    {
        if (!contentRequested)
        {
            return;
        }

        foreach (var match in matches)
        {
            foreach (var projection in match.Projections)
            {
                if (projection.State == FindProjectionState.Missing
                    && !HasProjectionFinding(findings, match, projection, FindFindingCode.ProjectionMissing))
                {
                    findings.Add(CreateProjectionFinding(
                        match,
                        projection,
                        FindFindingCode.ProjectionMissing,
                        "The requested Find section is not present in the source layer."));
                }
                else if (projection.State == FindProjectionState.Ambiguous
                    && !HasProjectionFinding(findings, match, projection, FindFindingCode.SectionAmbiguous))
                {
                    findings.Add(CreateProjectionFinding(
                        match,
                        projection,
                        FindFindingCode.SectionAmbiguous,
                        "The requested Find section is ambiguous in the source layer."));
                }
                else if (projection.State == FindProjectionState.Unavailable
                    && !HasIncompleteSourceFinding(findings, match))
                {
                    findings.Add(CreateProjectionFinding(
                        match,
                        projection,
                        FindFindingCode.ProjectionUnavailable,
                        "The requested Find projection is unavailable."));
                }
            }
        }
    }

    private static bool HasProjectionFinding(
        IEnumerable<FindFinding> findings,
        FindMatch match,
        FindProjection projection,
        FindFindingCode code)
        => findings.Any(finding => finding.Code == code
            && finding.Source is { } source
            && string.Equals(source.Id, match.Id, StringComparison.Ordinal)
            && string.Equals(source.Path, match.Path, StringComparison.Ordinal)
            && (finding.Layer is null || finding.Layer == projection.Layer)
            && string.Equals(finding.Region?.Name, projection.Name, StringComparison.Ordinal));

    private static bool HasIncompleteSourceFinding(
        IEnumerable<FindFinding> findings,
        FindMatch match)
        => findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete
            && finding.Source is { } source
            && string.Equals(source.Id, match.Id, StringComparison.Ordinal)
            && string.Equals(source.Path, match.Path, StringComparison.Ordinal));

    private static FindFinding CreateProjectionFinding(
        FindMatch match,
        FindProjection projection,
        FindFindingCode code,
        string cause)
    {
        var source = new FindSourceIdentity(match.Id, match.Path);
        var region = projection.Part == FindContentPartKind.Section
            ? new FindRegion(
                FindRegionKind.Section,
                projection.Name,
                $"{FindDefinitions.SectionPrefix}{projection.Name}")
            : null;
        return new FindFinding(
            code,
            FindDefinitions.ReadFindingStatus(code),
            match.Id,
            cause,
            null,
            null,
            source,
            projection.Layer,
            projection.Path ?? match.Path,
            region,
            projection.Location,
            []);
    }

    private static void AddStageFindings(
        ICollection<FindFinding> findings,
        FindResultFindingInput input)
    {
        if (input.StageCompletion.Matching == FindCoverageState.Incomplete
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.InspectionUnavailable,
                "One or more Find matching facts are unavailable."));
        }

        if (input.ContentRequested
            && input.StageCompletion.Projection == FindProjectionCoverageState.Incomplete
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.ProjectionUnavailable,
                "One or more Find projections are unavailable."));
        }

        if (input.ContentRequested
            && input.StageCompletion.Projection == FindProjectionCoverageState.NotStarted
            && input.Matches.Count != 0
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.ProjectionUnavailable,
                "The requested Find projections were not established."));
        }

        if (input.StageCompletion.Matching == FindCoverageState.Failed
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.OperationFailed,
                "An unexpected failure prevented Find matching from completing."));
        }

        if (input.StageCompletion.Matching == FindCoverageState.Interrupted
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.Interrupted,
                "The Find invocation was interrupted before matching completed."));
        }

        if (input.StageCompletion.Projection == FindProjectionCoverageState.Failed
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.OperationFailed,
                "An unexpected failure prevented Find projection from completing."));
        }

        if (input.StageCompletion.Projection == FindProjectionCoverageState.Interrupted
            && !findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            findings.Add(CreateAggregateFinding(
                FindFindingCode.Interrupted,
                "The Find invocation was interrupted before projection completed."));
        }

        if (input.TerminalEvent is { } terminalEvent)
        {
            var code = terminalEvent.Kind == FindTerminalEventKind.Failed
                ? FindFindingCode.OperationFailed
                : FindFindingCode.Interrupted;
            if (!findings.Any(finding => finding.Code == code))
            {
                findings.Add(CreateAggregateFinding(code, terminalEvent.Cause));
            }
        }
    }

    private static FindFinding CreateAggregateFinding(FindFindingCode code, string cause)
        => new(
            code,
            FindDefinitions.ReadFindingStatus(code),
            null,
            cause,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);

    private static List<FindFinding> RetainAllowedFindings(
        CliSemanticStatus status,
        IEnumerable<FindFinding> findings)
    {
        CliSemanticStatus[] allowed = status switch
        {
            CliSemanticStatus.Complete => [],
            CliSemanticStatus.Attention => [CliSemanticStatus.Attention],
            CliSemanticStatus.Incomplete => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete],
            CliSemanticStatus.Invalid => [CliSemanticStatus.Invalid],
            CliSemanticStatus.Blocked => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Blocked],
            CliSemanticStatus.Failed => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Failed],
            CliSemanticStatus.Interrupted => [CliSemanticStatus.Attention, CliSemanticStatus.Incomplete, CliSemanticStatus.Interrupted],
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
        return findings
            .Where(finding => allowed.Contains(finding.Status))
            .ToList();
    }

    private static void EnsureStatusFinding(
        CliSemanticStatus status,
        ICollection<FindFinding> findings,
        FindTerminalEvent? terminalEvent,
        FindProjectionCoverageState projectionCoverage)
    {
        if (status == CliSemanticStatus.Complete
            || findings.Any(finding => finding.Status == status))
        {
            return;
        }

        var (code, cause) = status switch
        {
            CliSemanticStatus.Attention => (
                FindFindingCode.IdentityCollision,
                "The Find result contains an established attention condition."),
            CliSemanticStatus.Incomplete when projectionCoverage == FindProjectionCoverageState.Incomplete => (
                FindFindingCode.ProjectionUnavailable,
                "One or more Find projections are unavailable."),
            CliSemanticStatus.Incomplete => (
                FindFindingCode.InspectionUnavailable,
                "One or more Find source facts are unavailable."),
            CliSemanticStatus.Invalid => (
                FindFindingCode.InvalidInput,
                "The Find input is invalid."),
            CliSemanticStatus.Blocked => (
                FindFindingCode.WorkspaceUnavailable,
                "The Find workspace or source boundary is unavailable."),
            CliSemanticStatus.Failed => (
                FindFindingCode.OperationFailed,
                terminalEvent?.Cause ?? "An unexpected failure prevented Find from completing."),
            CliSemanticStatus.Interrupted => (
                FindFindingCode.Interrupted,
                terminalEvent?.Cause ?? "The Find invocation was interrupted."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
        findings.Add(CreateAggregateFinding(code, cause));
    }

    private static IReadOnlyList<FindFinding> OrderFindings(IEnumerable<FindFinding> findings)
        => findings
            .OrderBy(finding => ReadFindingRank(finding.Code))
            .ThenBy(finding => ReadSelectorRoleRank(finding.SelectorRole))
            .ThenBy(finding => finding.SelectorOccurrence ?? int.MaxValue)
            .ThenBy(finding => finding.Source?.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Source?.Path, StringComparer.Ordinal)
            .ThenBy(finding => ReadOptionalLayerRank(finding.Layer))
            .ThenBy(finding => ReadOptionalRegionRank(finding.Region))
            .ThenBy(finding => finding.Location?.ByteOffset ?? long.MaxValue)
            .ThenBy(finding => finding.Location?.ByteLength ?? long.MaxValue)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();

    private static int ReadFindingRank(FindFindingCode code)
        => code switch
        {
            FindFindingCode.InvalidInput => 0,
            FindFindingCode.InvalidSelector => 1,
            FindFindingCode.WorkspaceUnavailable => 2,
            FindFindingCode.WorkspaceUnsafe => 3,
            FindFindingCode.SelectorAmbiguous => 4,
            FindFindingCode.SelectorUnsafe => 5,
            FindFindingCode.IdentityCollision => 6,
            FindFindingCode.CandidateUnsafe => 7,
            FindFindingCode.LayerUnresolved => 8,
            FindFindingCode.InspectionUnavailable => 9,
            FindFindingCode.InvalidEncoding => 10,
            FindFindingCode.FrontmatterUnavailable => 11,
            FindFindingCode.SectionAmbiguous => 12,
            FindFindingCode.ProjectionMissing => 13,
            FindFindingCode.ProjectionUnavailable => 14,
            FindFindingCode.OperationFailed => 15,
            FindFindingCode.Interrupted => 16,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };

    private static int ReadSelectorRoleRank(FindSelectorRole? role)
        => role switch
        {
            FindSelectorRole.Include => 0,
            FindSelectorRole.Exclude => 1,
            null => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Find selector role is not defined."),
        };

    private static int ReadOptionalLayerRank(SourceLayerKind? layer)
        => layer switch
        {
            SourceLayerKind.Base => 0,
            SourceLayerKind.Overwrite => 1,
            null => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private static int ReadOptionalRegionRank(FindRegion? region)
        => region?.Kind switch
        {
            FindRegionKind.Frontmatter => 0,
            FindRegionKind.Body or FindRegionKind.Section => 1,
            FindRegionKind.Document => 2,
            null => 3,
            _ => throw new ArgumentOutOfRangeException(nameof(region), region?.Kind, "The Find region kind is not defined."),
        };
}
