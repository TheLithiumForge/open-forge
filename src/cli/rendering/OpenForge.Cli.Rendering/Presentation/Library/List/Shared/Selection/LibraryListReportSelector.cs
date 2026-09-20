using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.List.Models;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Library.List.Shared.Selection;

internal static class LibraryListReportSelector
{
    internal static CliReport<LibraryListData> Select(LibraryListResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var shape = selection.Detail switch
        {
            CliDetail.Minimal => LibraryListDataLinksShape.Counts,
            CliDetail.Standard => LibraryListDataLinksShape.Paths,
            CliDetail.Full or CliDetail.Debug => LibraryListDataLinksShape.Targets,
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Detail, "The detail level is not defined."),
        };
        var libraries = result.Result.Libraries.Select(library => Project(library, shape)).ToArray();
        var totals = Count(result.Result.Libraries.SelectMany(library => library.Paths));
        var findings = result.Result.Findings.Select(finding => Finding(finding)).ToArray();
        var counts = Counts(result, totals);
        var data = new LibraryListData
        {
            Libraries = libraries,
            RecordCoverage = selection.Detail >= CliDetail.Full ? ProjectRecord(result.Result.Record) : null,
            Record = result.Result.Record,
            Inventory = result.Result.Inventory,
            Coverage = result.Result.Coverage,
            ShowLinks = selection.Detail >= CliDetail.Standard,
            ShowLinkTargets = selection.Detail >= CliDetail.Full,
        };
        return new CliReport<LibraryListData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } workspace
                ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit)
                : null,
            Findings = findings,
            Effects = [],
            Counts = counts,
            Data = data,
            Recovery = null,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
                    $"libraries={result.Result.Libraries.Length}",
                    $"links={result.Result.Libraries.SelectMany(library => library.Paths).Count()}",
                }.Concat(result.Result.Findings.Select(finding => finding.Cause)).ToArray()
                : [],
        };
    }

    private static LibraryListDataLibrary Project(
        LibraryListView library,
        LibraryListDataLinksShape shape)
    {
        var rows = library.Paths.Select(path => new LibraryListDataLink
        {
            Path = path.DestinationPath,
            State = LibraryListWording.LinkWireState(path.State),
            ResultState = path.State,
            ExpectedTarget = path.ExpectedRelativeLink,
            ObservedTarget = path.ObservedRelativeLink,
            SourceId = path.SourceId,
        }).ToArray();
        var counts = Count(library.Paths);
        var links = shape == LibraryListDataLinksShape.Counts
            ? LibraryListDataLinks.FromCounts(counts)
            : LibraryListDataLinks.FromPaths(shape, rows, counts);
        return new LibraryListDataLibrary
        {
            Id = library.Id,
            SourceFolder = library.SourceRoot,
            DestinationFolder = library.DestinationRoot,
            Links = links,
            SourceRootState = library.SourceRootState,
        };
    }

    private static LibraryListDataLinkCounts Count(
        IEnumerable<LibraryListRegisteredPath> paths)
        => Count(paths.Select(path => path.State));

    private static LibraryListDataLinkCounts Count(
        IEnumerable<LibraryLinkViewState> states)
    {
        var values = states.ToArray();
        return new LibraryListDataLinkCounts(
            values.Count(state => state == LibraryLinkViewState.Current),
            values.Count(state => state == LibraryLinkViewState.Missing),
            values.Count(state => state == LibraryLinkViewState.Changed),
            values.Count(state => state is LibraryLinkViewState.Unavailable or LibraryLinkViewState.Blocked));
    }

    private static LibraryListDataRecordCoverage ProjectRecord(LibraryListRecordView record)
        => new()
        {
            Path = record.Path,
            State = LibraryListWording.RecordState(record.State),
            ResultState = record.State,
            LibraryCount = record.LibraryCount,
        };

    private static CliHeadline Headline(LibraryListResult result)
    {
        var libraries = result.Result.Libraries.Length;
        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Result.Findings.Any(finding => finding.Code == LibraryListFindingCode.OwnershipObservation)
                => new(LibraryListWording.NoOwnership(), CliHeadlineKind.Done),
            CliSemanticStatus.Complete when libraries == 0
                => new(LibraryListWording.NoLibraries(), CliHeadlineKind.Done),
            CliSemanticStatus.Complete
                => new(LibraryListWording.Registered(libraries), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(LibraryListWording.Attention(libraries, AttentionCount(result)), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(LibraryListWording.Incomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(LibraryListWording.CannotList(finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTheResultDidNotContainAFinding()), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(LibraryListWording.CannotList(finding?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTheResultDidNotContainAFinding()), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(FindingMessage(finding), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(LibraryListWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Library List status is not defined."),
        };
    }

    private static int AttentionCount(LibraryListResult result)
    {
        return result.Result.Libraries.SelectMany(library => library.Paths).Count(path => path.State is
            LibraryLinkViewState.Missing or LibraryLinkViewState.Changed);
    }

    private static IReadOnlyList<CliCount> Counts(
        LibraryListResult result,
        LibraryListDataLinkCounts totals)
    {
        var record = result.Result.Record;
        var rosterIsKnown = record.State == LibraryRecordViewState.Missing
            || (record.State == LibraryRecordViewState.Complete && record.LibraryCount is not null);
        if (rosterIsKnown)
        {
            return
            [
                new CliCount("libraries", global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLibraries(), record.State == LibraryRecordViewState.Missing ? 0 : record.LibraryCount),
                new CliCount("linksCurrent", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksCurrent(), totals.Current),
                new CliCount("linksMissing", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksMissing(), totals.Missing),
                new CliCount("linksChanged", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksChanged(), totals.Changed),
                new CliCount("linksUnavailable", global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLinksUnavailable(), totals.Unavailable),
            ];
        }

        var reason = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)?.Cause
            ?? global::OpenForge.Cli.OutputText.Library.List.LibraryListText.MessageTheLibraryRecordDidNotProvideAnAuthoritativeRoster();
        reason = CliFindingWording.CauseSentence(reason);
        return
        [
            new CliCount("libraries", global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLibraries(), null, reason),
            new CliCount("linksCurrent", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksCurrent(), null, reason),
            new CliCount("linksMissing", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksMissing(), null, reason),
            new CliCount("linksChanged", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksChanged(), null, reason),
            new CliCount("linksUnavailable", global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLinksUnavailable(), null, reason),
        ];
    }

    private static string? HeadlineFindingCode(LibraryListResult result)
    {
        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        return result.Status == CliSemanticStatus.Invalid
            && finding?.Code == LibraryListFindingCode.InvalidInput
            ? LibraryListWording.MachineCode(finding.Code)
            : null;
    }

    private static CliFinding Finding(LibraryListFinding finding)
    {
        var code = LibraryListWording.MachineCode(finding.Code);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = code,
            Title = LibraryListWording.FindingTitle(finding.Code),
            Message = Message(finding),
            Subject = new CliSubject(SubjectKind(finding.Code), finding.Path, finding.LibraryId ?? finding.Path ?? global::OpenForge.Cli.OutputText.Library.List.LibraryListText.LabelLibraryList()),
        };
    }

    private static CliSubjectKind SubjectKind(LibraryListFindingCode code) => code switch
    {
        LibraryListFindingCode.InvalidInput => CliSubjectKind.Identifier,
        LibraryListFindingCode.SourceRootInvalid
            or LibraryListFindingCode.SourceRootUnavailable
            or LibraryListFindingCode.SourceRootBlocked => CliSubjectKind.Directory,
        _ => CliSubjectKind.File,
    };

    private static string Message(LibraryListFinding finding) => finding.Code switch
    {
        LibraryListFindingCode.InvalidInput => LibraryListWording.InvalidInput(finding.Cause),
        LibraryListFindingCode.OwnershipObservation => LibraryListWording.NoOwnership(),
        LibraryListFindingCode.InvalidRecord => LibraryListWording.InvalidRecord(finding.Cause),
        LibraryListFindingCode.RecordUnavailable => LibraryListWording.RecordUnavailable(),
        LibraryListFindingCode.RecordBlocked => LibraryListWording.RecordBlocked(finding.Cause),
        LibraryListFindingCode.SourceRootInvalid => LibraryListWording.SourceRootInvalid(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheLibrary(), finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryListFindingCode.SourceRootUnavailable => LibraryListWording.SourceRootUnavailable(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheLibrary(), finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryListFindingCode.SourceRootBlocked => LibraryListWording.SourceRootBlocked(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheLibrary(), finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryListFindingCode.LinkMissing => LibraryListWording.LinkMissing(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheRegisteredLink()),
        LibraryListFindingCode.LinkChanged => LibraryListWording.LinkChanged(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheRegisteredLink(), finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheLibrary()),
        LibraryListFindingCode.LinkUnavailable => LibraryListWording.LinkUnavailable(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheRegisteredLink()),
        LibraryListFindingCode.LinkBlocked => LibraryListWording.LinkBlocked(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheRegisteredLink(), finding.Cause),
        LibraryListFindingCode.OperationFailed => LibraryListWording.Failed(finding.Cause),
        LibraryListFindingCode.Interrupted => LibraryListWording.Interrupted(),
        _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Library List finding code is not defined."),
    };

    private static string FindingMessage(LibraryListFinding? finding)
        => finding is null ? LibraryListWording.Failed(global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTheResultDidNotContainAFinding()) : Message(finding);

    private static CliNextAction? Next(LibraryListResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            return result.Result.Libraries.Length == 0
                && !result.Result.Findings.Any(finding => finding.Code == LibraryListFindingCode.OwnershipObservation)
                ? new CliNextAction("open-forge library attach <id> <source-folder>", LibraryListWording.AttachNextReason())
                : null;
        }

        var first = result.Result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Attention when first?.Code == LibraryListFindingCode.LinkMissing && first.LibraryId is { } id
                => new CliNextAction($"open-forge library sync {id}", LibraryListWording.SyncNextReason()),
            CliSemanticStatus.Attention when first?.Code == LibraryListFindingCode.LinkChanged && first.LibraryId is { } id
                => new CliNextAction($"open-forge library inspect {id}", LibraryListWording.InspectNextReason()),
            CliSemanticStatus.Attention when first?.Code is LibraryListFindingCode.SourceRootInvalid or LibraryListFindingCode.SourceRootUnavailable
                && first.LibraryId is { } id
                => new CliNextAction($"open-forge library inspect {id}", LibraryListWording.InspectNextReason()),
            CliSemanticStatus.Invalid when first?.Code == LibraryListFindingCode.InvalidInput
                => new CliNextAction("open-forge library list --help", LibraryListWording.HelpNextReason()),
            CliSemanticStatus.Invalid => new CliNextAction("open-forge doctor", LibraryListWording.DoctorNextReason()),
            CliSemanticStatus.Failed => new CliNextAction("open-forge library list --detail debug", LibraryListWording.FailedNextReason()),
            CliSemanticStatus.Interrupted => new CliNextAction("open-forge library list", LibraryListWording.RetryNextReason()),
            _ => new CliNextAction("open-forge doctor", LibraryListWording.DoctorNextReason()),
        };
    }
}
