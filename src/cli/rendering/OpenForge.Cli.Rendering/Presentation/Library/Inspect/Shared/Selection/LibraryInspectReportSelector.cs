using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Selection;

internal static class LibraryInspectReportSelector
{
    internal static CliReport<LibraryInspectData> Select(LibraryInspectResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);

        var comparisons = result.Result.Projection.Comparisons;
        var shape = selection.Detail switch
        {
            CliDetail.Minimal => LibraryInspectDataFilesShape.Differences,
            CliDetail.Standard => LibraryInspectDataFilesShape.Paths,
            CliDetail.Full or CliDetail.Debug => LibraryInspectDataFilesShape.Targets,
            _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Detail, "The detail level is not defined."),
        };
        var allFiles = comparisons.Select(ProjectFile).ToArray();
        var visibleFiles = selection.Detail == CliDetail.Minimal
            ? allFiles.Where(file => !string.Equals(file.Relation, "current", StringComparison.Ordinal)).ToArray()
            : allFiles;
        var findings = result.Result.Findings.Select(finding => Finding(result, finding)).ToArray();
        var current = IsCurrent(result);
        var data = new LibraryInspectData
        {
            Id = result.Result.Record.Id ?? result.Result.Findings.FirstOrDefault()?.LibraryId,
            SourceFolder = result.Result.Record.SourceRoot,
            DestinationFolder = result.Result.Record.DestinationRoot,
            Current = current,
            Files = LibraryInspectDataFiles.From(shape, visibleFiles),
            Inventory = selection.Detail >= CliDetail.Full
                ? new LibraryInspectDataInventory
                {
                    Eligible = result.Result.Source.EligiblePaths.Length,
                    Excluded = result.Result.Source.ExcludedCount,
                }
                : null,
            TextFiles = allFiles,
            ShowFiles = selection.Detail >= CliDetail.Standard,
            ShowTargets = selection.Detail >= CliDetail.Full,
        };
        return new CliReport<LibraryInspectData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = HeadlineFindingCode(result),
            Workspace = result.WorkspacePath is { } path
                ? new CliWorkspaceEcho(path, result.WorkspaceExplicit)
                : null,
            Findings = findings,
            Effects = [],
            Counts = Counts(comparisons, result.Result.Source.EligiblePaths.Length),
            Limitations = [],
            Data = data,
            Recovery = null,
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[]
                {
                    $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
                    $"library={data.Id ?? "unavailable"}",
                    $"source-files={result.Result.Source.EligiblePaths.Length}",
                    $"comparisons={comparisons.Length}",
                    $"findings={result.Result.Findings.Length}",
                }.Concat(result.Result.Findings.Select(finding => finding.Cause)).ToArray()
                : [],
        };
    }

    private static LibraryInspectDataFile ProjectFile(LibraryPathComparison comparison)
        => new()
        {
            SourcePath = comparison.SourcePath,
            DestinationPath = comparison.DestinationPath,
            Relation = LibraryInspectWording.RelationWire(comparison.Relation),
            ExpectedTarget = comparison.Registered?.ExpectedRelativeLink,
            ObservedTarget = comparison.ObservedRelativeLink,
        };

    private static bool IsCurrent(LibraryInspectResult result)
        => result.Status == CliSemanticStatus.Complete
            && result.Result.Findings.Length == 0
            && result.Result.Record.State == LibraryRecordViewState.Complete
            && result.Result.Source.State == LibraryInventoryViewState.Complete
            && result.Result.Projection.State == LibraryCoverage.Complete;

    private static CliHeadline Headline(LibraryInspectResult result)
    {
        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Result.Findings.FirstOrDefault();
        var id = result.Result.Record.Id ?? finding?.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId();
        var source = result.Result.Record.SourceRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder();
        var destination = result.Result.Record.DestinationRoot ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationFolder();
        var comparisons = result.Result.Projection.Comparisons;
        var differences = comparisons.Count(comparison => comparison.Relation is
            LibraryComparisonRelation.Added
                or LibraryComparisonRelation.Retired
                or LibraryComparisonRelation.Missing
                or LibraryComparisonRelation.Changed);
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Result.Findings.Any(finding => finding.Code == LibraryInspectFindingCode.OwnershipObservation)
                => new(LibraryInspectWording.NoOwnership(id), CliHeadlineKind.Done),
            CliSemanticStatus.Complete when comparisons.Length == 0
                => new(LibraryInspectWording.Empty(id, source), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete
                => new(LibraryInspectWording.Current(id, comparisons.Length, source, destination), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(LibraryInspectWording.NeedsSync(id, differences, source, destination), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(LibraryInspectWording.Incomplete(id, finding is null ? global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleTheComparisonCouldNotFinish() : Message(result, finding)), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid
                => new(LibraryInspectWording.CannotInspect(id, finding is null ? global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleTheRequestIsInvalid() : Message(result, finding)), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked
                => new(LibraryInspectWording.CannotInspect(id, finding is null ? global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.TitleTheInspectionIsBlocked() : Message(result, finding)), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(LibraryInspectWording.Failed(finding is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTheResultDidNotContainAFinding() : finding.Cause), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(LibraryInspectWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Library Inspect status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(LibraryInspectResult result)
    {
        var finding = result.Result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        return result.Status == CliSemanticStatus.Invalid
            && finding?.Code is LibraryInspectFindingCode.InvalidId or LibraryInspectFindingCode.UnknownId
            ? LibraryInspectWording.MachineCode(finding.Code)
            : null;
    }

    private static CliFinding Finding(LibraryInspectResult result, LibraryInspectFinding finding)
    {
        var code = LibraryInspectWording.MachineCode(finding.Code);
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = code,
            Title = LibraryInspectWording.FindingTitle(finding.Code),
            Message = Message(result, finding),
            Subject = new CliSubject(SubjectKind(finding.Code), finding.Path, finding.LibraryId ?? finding.Path ?? global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.LabelLibraryInspect()),
        };
    }

    private static CliSubjectKind SubjectKind(LibraryInspectFindingCode code) => code switch
    {
        LibraryInspectFindingCode.InvalidId or LibraryInspectFindingCode.UnknownId => CliSubjectKind.Identifier,
        LibraryInspectFindingCode.SourceRootInvalid
            or LibraryInspectFindingCode.SourceRootUnavailable
            or LibraryInspectFindingCode.SourceRootBlocked
            or LibraryInspectFindingCode.InventoryIncomplete => CliSubjectKind.Directory,
        _ => CliSubjectKind.File,
    };

    private static string Message(LibraryInspectResult result, LibraryInspectFinding finding) => finding.Code switch
    {
        LibraryInspectFindingCode.InvalidId => LibraryInspectWording.InvalidId(finding.LibraryId),
        LibraryInspectFindingCode.UnknownId => LibraryInspectWording.UnknownId(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId()),
        LibraryInspectFindingCode.RecordInvalid => LibraryInspectWording.RecordInvalid(finding.Cause),
        LibraryInspectFindingCode.RecordUnavailable => LibraryInspectWording.RecordUnavailable(),
        LibraryInspectFindingCode.RecordBlocked => LibraryInspectWording.RecordBlocked(finding.Cause),
        LibraryInspectFindingCode.OwnershipObservation => LibraryInspectWording.NoOwnership(finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSuppliedId()),
        LibraryInspectFindingCode.SourceRootInvalid => LibraryInspectWording.SourceRootInvalid(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryInspectFindingCode.SourceRootUnavailable => LibraryInspectWording.SourceRootUnavailable(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryInspectFindingCode.SourceRootBlocked => LibraryInspectWording.SourceRootBlocked(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryInspectFindingCode.InventoryIncomplete => LibraryInspectWording.InventoryIncomplete(result.Result.Record.SourceRoot ?? finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheSourceFolder()),
        LibraryInspectFindingCode.PathAdded => LibraryInspectWording.PathAdded(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath()),
        LibraryInspectFindingCode.PathRetired => LibraryInspectWording.PathRetired(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath()),
        LibraryInspectFindingCode.LinkMissing => LibraryInspectWording.LinkMissing(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath()),
        LibraryInspectFindingCode.LinkChanged => LibraryInspectWording.LinkChanged(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath(), finding.LibraryId ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheLibrary()),
        LibraryInspectFindingCode.LinkBlocked => LibraryInspectWording.LinkBlocked(finding.Path ?? global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelTheDestinationPath(), finding.Cause),
        LibraryInspectFindingCode.OperationFailed => LibraryInspectWording.Failed(finding.Cause),
        LibraryInspectFindingCode.Interrupted => LibraryInspectWording.Cancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Library Inspect finding code is not defined."),
    };

    private static IReadOnlyList<CliCount> Counts(
        IReadOnlyList<LibraryPathComparison> comparisons,
        int sourceFiles)
        =>
        [
            new CliCount("sourceFiles", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelSourceFiles(), sourceFiles),
            new CliCount("linksCurrent", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksCurrent(), comparisons.Count(comparison => comparison.Relation == LibraryComparisonRelation.Current)),
            new CliCount("filesAdded", global::OpenForge.Cli.OutputText.Library.Inspect.LibraryInspectText.LabelFilesAdded(), comparisons.Count(comparison => comparison.Relation == LibraryComparisonRelation.Added)),
            new CliCount("filesRetired", global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFilesRetired(), comparisons.Count(comparison => comparison.Relation == LibraryComparisonRelation.Retired)),
            new CliCount("linksMissing", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksMissing(), comparisons.Count(comparison => comparison.Relation == LibraryComparisonRelation.Missing)),
            new CliCount("linksChanged", global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.LabelLinksChanged(), comparisons.Count(comparison => comparison.Relation == LibraryComparisonRelation.Changed)),
        ];

    private static CliNextAction? Next(LibraryInspectResult result)
    {
        var first = result.Result.Findings.FirstOrDefault();
        if (first is null)
        {
            return null;
        }

        return first.Code switch
        {
            LibraryInspectFindingCode.InvalidId or LibraryInspectFindingCode.UnknownId
                => new CliNextAction("open-forge library list", LibraryInspectWording.ListNextReason()),
            LibraryInspectFindingCode.RecordInvalid
                => new CliNextAction("open-forge doctor", LibraryInspectWording.DoctorNextReason()),
            LibraryInspectFindingCode.PathAdded
                or LibraryInspectFindingCode.PathRetired
                or LibraryInspectFindingCode.LinkMissing when first.LibraryId is { } id
                => new CliNextAction($"open-forge library sync {id} --dry-run", LibraryInspectWording.SyncNextReason()),
            LibraryInspectFindingCode.LinkChanged
                => new CliNextAction(global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFixByHand(), LibraryInspectWording.FixByHandNextReason()) { Kind = CliNextActionKind.Sentence },
            _ => null,
        };
    }
}
