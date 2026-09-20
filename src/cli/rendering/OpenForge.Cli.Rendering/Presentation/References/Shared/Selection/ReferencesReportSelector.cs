using System.Globalization;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Presentation.References.Models;
using OpenForge.Cli.Core.Presentation.References.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.References.Shared.Selection;

internal static class ReferencesReportSelector
{
    internal static CliReport<ReferencesData> Select(ReferencesResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var showResolvedTargets = selection.Detail >= CliDetail.Standard;
        var showLayers = selection.Detail >= CliDetail.Full;
        var data = new ReferencesData
        {
            Source = result.Source is { } source
                ? new ReferencesDataSource { Id = source.Id, Path = source.Path }
                : null,
            Direction = result.RequestedDirection is { } direction
                ? ReferencesWording.WireDirection(direction)
                : null,
            Incoming = result.Incoming is { } incoming
                ? incoming.Occurrences.Select(occurrence => Incoming(occurrence, showLayers)).ToArray()
                : null,
            Outgoing = result.Outgoing is { } outgoing
                ? outgoing.Occurrences.Select(occurrence => Outgoing(occurrence, showLayers)).ToArray()
                : null,
            Coverage = showResolvedTargets ? Coverage(result) : null,
            Filters = showResolvedTargets ? Filters(result.IncomingSelection) : null,
            Scanned = showLayers ? Scanned(result.IncomingSelection) : null,
            SourcePath = result.Source?.Path,
            ShowResolvedTargets = showResolvedTargets,
            ShowLayers = showLayers,
        };
        return new CliReport<ReferencesData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } workspacePath
                ? new CliWorkspaceEcho(workspacePath, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(Finding).ToArray(),
            Effects = [],
            Counts = Counts(result),
            Data = data,
            Recovery = null,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug ? Diagnostics(result) : [],
        };
    }

    private static ReferencesDataIncoming Incoming(ReferencesOccurrence occurrence, bool showLayers)
        => new()
        {
            Path = occurrence.Source.Path,
            Location = Location(occurrence.LocationView.Line, occurrence.LocationView.Column),
            Layer = showLayers ? ReferencesWording.WireLayer(occurrence.Source.LayerKind) : null,
        };

    private static ReferencesDataOutgoing Outgoing(
        ReferencesOccurrence occurrence,
        bool showLayers)
        => new()
        {
            Location = Location(occurrence.LocationView.Line, occurrence.LocationView.Column),
            Destination = occurrence.RawDestination,
            // The catalogue lists resolvedPath in the minimal JSON shape, so the data always
            // carries it. Only the text renderer waits until `standard` to print it.
            ResolvedPath = occurrence.Target.Path,
            State = ReferencesWording.WireResolution(occurrence.Target.Resolution),
            Layer = showLayers ? ReferencesWording.WireLayer(occurrence.Source.LayerKind) : null,
            RowState = ReferencesWording.RowState(occurrence.Target.Resolution),
            TargetKind = occurrence.Target.Kind,
        };

    private static string Location(int line, int column)
        => string.Create(CultureInfo.InvariantCulture, $":{line}:{column}");

    private static ReferencesDataCoverage Coverage(ReferencesResult result)
        => new()
        {
            Incoming = result.Incoming is { } incoming ? WireCoverage(incoming.Coverage) : null,
            Outgoing = result.Outgoing is { } outgoing ? WireCoverage(outgoing.Coverage) : null,
        };

    private static string WireCoverage(ReferencesCoverage coverage) => coverage switch
    {
        ReferencesCoverage.Complete => "complete",
        ReferencesCoverage.Incomplete => "incomplete",
        ReferencesCoverage.Blocked => "blocked",
        _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The References coverage is not defined."),
    };

    private static ReferencesDataFilters? Filters(ReferencesIncomingSelection? selection)
    {
        if (selection is null)
        {
            return null;
        }

        return new ReferencesDataFilters
        {
            Include = Supplied(selection, ReferencesSelectorRole.Include),
            Exclude = Supplied(selection, ReferencesSelectorRole.Exclude),
        };
    }

    private static IReadOnlyList<string> Supplied(
        ReferencesIncomingSelection selection,
        ReferencesSelectorRole role)
        => selection.Supplied
            .Where(occurrence => occurrence.RoleKind == role)
            .Select(occurrence => occurrence.Value)
            .ToArray();

    private static IReadOnlyList<ReferencesDataScannedSource>? Scanned(ReferencesIncomingSelection? selection)
        => selection?.InspectedSources
            .Select(evidence => new ReferencesDataScannedSource
            {
                Id = evidence.Source.Id,
                Path = evidence.Path,
                Layer = ReferencesWording.WireLayer(evidence.LayerKind),
            })
            .ToArray();

    private static CliHeadline Headline(ReferencesResult result)
    {
        var finding = result.Findings.FirstOrDefault(value => value.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when HasOccurrences(result)
                => new(result.Source?.Path ?? string.Empty, CliHeadlineKind.Done),
            CliSemanticStatus.Complete
                => new(
                    ReferencesWording.NoLinks(
                        result.RequestedDirection ?? ReferencesDirection.Both,
                        result.Source?.Path ?? string.Empty),
                    CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(result.Source?.Path ?? string.Empty, CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(result.Source?.Path ?? string.Empty, CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(ReferencesWording.CannotList(Cause(finding)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked when finding is { Code: ReferencesFindingCode.PhysicalAlias }
                => new(ReferencesWording.CannotList(PhysicalAliasMessage(finding)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Blocked
                => new(ReferencesWording.CannotList(Cause(finding)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(ReferencesWording.Failed(Cause(finding)), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(ReferencesWording.Interrupted(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The References status is not defined."),
        };
    }

    private static bool HasOccurrences(ReferencesResult result)
        => (result.Incoming?.OccurrenceCount ?? 0) + (result.Outgoing?.OccurrenceCount ?? 0) > 0;

    private static string Cause(ReferencesFinding? finding)
        => finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTheResultDidNotContainAFinding();

    private static string? HeadlineFindingCode(ReferencesResult result)
    {
        var finding = result.Findings.FirstOrDefault(value => value.Status == result.Status);
        return result.Status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked && finding is not null
            ? ReferencesWireVocabulary.Name(finding.Code)
            : null;
    }

    private static IReadOnlyList<CliCount> Counts(ReferencesResult result)
    {
        var scanned = result.IncomingSelection?.InspectedSources.Count;
        return
        [
            new CliCount("incoming", global::OpenForge.Cli.OutputText.References.ReferencesText.LabelIncoming(), result.Incoming?.OccurrenceCount),
            new CliCount("outgoing", global::OpenForge.Cli.OutputText.References.ReferencesText.LabelOutgoing(), result.Outgoing?.OccurrenceCount),
            new CliCount("sourcesScanned", global::OpenForge.Cli.OutputText.References.ReferencesText.LabelSourcesScanned(), scanned),
        ];
    }

    private static CliFinding Finding(ReferencesFinding finding)
        => new()
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = ReferencesWireVocabulary.Name(finding.Code),
            Title = ReferencesWording.FindingTitle(finding.Code),
            Message = Message(finding),
            Subject = new CliSubject(
                SubjectKind(finding.Code),
                finding.Path,
                finding.Subject ?? finding.Path ?? "references"),
            Candidates = Candidates(finding),
        };

    private static CliSubjectKind SubjectKind(ReferencesFindingCode code) => code switch
    {
        ReferencesFindingCode.InvalidInput
            or ReferencesFindingCode.InvalidSource
            or ReferencesFindingCode.InvalidDirection
            or ReferencesFindingCode.InvalidFilter
            or ReferencesFindingCode.SourceAmbiguous
            or ReferencesFindingCode.SelectorAmbiguous
            or ReferencesFindingCode.IdentityCollision
            or ReferencesFindingCode.OperationFailed
            or ReferencesFindingCode.Interrupted => CliSubjectKind.Identifier,
        ReferencesFindingCode.WorkspaceUnavailable
            or ReferencesFindingCode.WorkspaceUnsafe => CliSubjectKind.Directory,
        _ => CliSubjectKind.File,
    };

    private static string Message(ReferencesFinding finding)
    {
        var location = finding.Path is { } path
            ? LocationText(path, finding.LocationView?.Line, finding.LocationView?.Column)
            : finding.Subject ?? global::OpenForge.Cli.OutputText.References.ReferencesText.LabelTheLink();
        return finding.Code switch
        {
            ReferencesFindingCode.InvalidInput => ReferencesWording.InvalidInput(finding.Cause),
            ReferencesFindingCode.InvalidSource => InvalidSource(finding),
            ReferencesFindingCode.InvalidDirection => ReferencesWording.InvalidDirection(),
            ReferencesFindingCode.InvalidFilter => InvalidFilter(finding),
            ReferencesFindingCode.WorkspaceUnavailable
                => CliFindingWording.WorkspaceUnavailable(finding.Path ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace()),
            ReferencesFindingCode.WorkspaceUnsafe
                => CliFindingWording.WorkspaceUnsafe(finding.Path ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace(), finding.Cause),
            ReferencesFindingCode.SourceAmbiguous
                or ReferencesFindingCode.SelectorAmbiguous
                or ReferencesFindingCode.SourceUnsafe
                or ReferencesFindingCode.SelectorUnsafe
                => ReferencesWording.CannotList(finding.Cause),
            ReferencesFindingCode.IdentityCollision
                => ReferencesWording.IdentityCollision(IdentityCollisionId(finding)),
            ReferencesFindingCode.PhysicalAlias
                => PhysicalAliasMessage(finding),
            ReferencesFindingCode.IdentityUnavailable
                => ReferencesWording.IdentityUnavailable(finding.Path),
            ReferencesFindingCode.CandidateUnsafe
                => ReferencesWording.CandidateUnsafe(finding.Path ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource()),
            ReferencesFindingCode.LayerUnresolved
                => ReferencesWording.LayerUnresolved(finding.Path ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource()),
            ReferencesFindingCode.InspectionUnavailable
                => ReferencesWording.InspectionUnavailable(finding.Path ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource()),
            ReferencesFindingCode.InvalidEncoding
                => ReferencesWording.InvalidEncoding(finding.Path ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource()),
            ReferencesFindingCode.LinkEncodingInvalid => ReferencesWording.LinkEncodingInvalid(location),
            ReferencesFindingCode.GeneratedRegionUnavailable
                => ReferencesWording.GeneratedRegionUnavailable(finding.Path ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource()),
            ReferencesFindingCode.DestinationMalformed => RowStateMessage(location, global::OpenForge.Cli.OutputText.References.ReferencesText.LabelNotAResolvableLink()),
            ReferencesFindingCode.DestinationUnsupported => RowStateMessage(location, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotFollowed()),
            ReferencesFindingCode.TargetMissing => RowStateMessage(location, global::OpenForge.Cli.OutputText.Shared.SharedText.LabelMissing()),
            ReferencesFindingCode.FragmentMissing => RowStateMessage(location, global::OpenForge.Cli.OutputText.References.ReferencesText.LabelHeadingNotFound()),
            ReferencesFindingCode.TargetUnsafe => ReferencesWording.TargetUnsafe(location),
            ReferencesFindingCode.TargetAmbiguous => ReferencesWording.TargetAmbiguous(location),
            ReferencesFindingCode.TargetUnreadable => RowStateMessage(location, global::OpenForge.Cli.OutputText.References.ReferencesText.LabelTargetCouldNotBeRead()),
            ReferencesFindingCode.OperationFailed => ReferencesWording.Failed(finding.Cause),
            ReferencesFindingCode.Interrupted => ReferencesWording.Interrupted(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The References finding code is not defined."),
        };
    }

    private static string InvalidSource(ReferencesFinding finding)
        => finding.Path is { } path
            ? ReferencesWording.SourceOutsideAgents(path)
            : ReferencesWording.UnknownSourceId(finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource());

    private static string InvalidFilter(ReferencesFinding finding)
        => finding.Subject is { } value
            ? ReferencesWording.FilterNotASource(value)
            : ReferencesWording.FilterNeedsIncoming();

    private static string? IdentityCollisionId(ReferencesFinding finding)
        => finding.Candidates.FirstOrDefault()?.Id ?? finding.Source?.Id;

    private static string PhysicalAliasMessage(ReferencesFinding finding)
        => ReferencesWording.PhysicalAlias(finding.Candidates.Select(candidate => candidate.Path).ToArray());

    private static IReadOnlyList<CliCandidate> Candidates(ReferencesFinding finding)
    {
        if (finding.Code is not (ReferencesFindingCode.IdentityCollision or ReferencesFindingCode.PhysicalAlias))
        {
            return [];
        }

        var reason = finding.Code switch
        {
            ReferencesFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelMatchesTheSameAutomaticId(),
            ReferencesFindingCode.PhysicalAlias => global::OpenForge.Cli.OutputText.References.ReferencesText.LabelResolvesToTheSamePhysicalFile(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The References finding code has no candidate wording."),
        };
        return finding.Candidates
            .Select(candidate => new CliCandidate(
                new CliSubject(CliSubjectKind.Source, Path: candidate.Path, Id: candidate.Id),
                [reason]))
            .ToArray();
    }

    private static string RowStateMessage(string location, string state)
        => $"{location}  {state}";

    private static string LocationText(string path, int? line, int? column)
        => line is { } lineValue && column is { } columnValue
            ? string.Create(CultureInfo.InvariantCulture, $"{path}:{lineValue}:{columnValue}")
            : path;

    private static IReadOnlyList<string> Diagnostics(ReferencesResult result)
        => new[]
        {
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"incoming={result.Incoming?.OccurrenceCount ?? 0}",
            $"outgoing={result.Outgoing?.OccurrenceCount ?? 0}",
            $"scanned={result.IncomingSelection?.InspectedSources.Count ?? 0}",
        }.Concat(result.Findings.Select(finding => finding.Cause)).ToArray();

    private static CliNextAction? Next(ReferencesResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            return null;
        }

        var first = result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Invalid when first?.Code == ReferencesFindingCode.InvalidSource
                => new CliNextAction("open-forge route list --depth=all", ReferencesWording.RouteListNextReason()),
            CliSemanticStatus.Invalid
                => new CliNextAction("open-forge references --help", ReferencesWording.HelpNextReason()),
            CliSemanticStatus.Failed
                => new CliNextAction("open-forge references --detail debug", ReferencesWording.FailedNextReason()),
            CliSemanticStatus.Interrupted
                => new CliNextAction("open-forge references", ReferencesWording.RetryNextReason()),
            _ => new CliNextAction("open-forge doctor", ReferencesWording.DoctorNextReason()),
        };
    }
}
