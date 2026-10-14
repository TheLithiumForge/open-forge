using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Presentation.Index.Models;
using OpenForge.Cli.Core.Presentation.Index.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Index.Shared.Selection;

internal static class IndexReportSelector
{
    internal static CliReport<IndexData> Select(IndexResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var preview = result.Mode == IndexMode.DryRun;
        var existing = result.Regions.Where(region => !result.Entrypoints.Any(item => item.Path == region.Source.Path)).ToArray();
        var updated = existing.Count(region => region.Outcome is IndexRegionOutcome.Applied or IndexRegionOutcome.Verified
            || preview && region.Outcome == IndexRegionOutcome.NotRequested);
        var current = existing.Count(region => region.Outcome == IndexRegionOutcome.AlreadyCurrent);
        var checkedFiles = updated + current;
        var nothingWritten = result.Metadata.All(item => !IsWritten(item.Outcome)) && result.Entrypoints.All(item => !IsWritten(item.Outcome)) && result.Regions.All(region => region.Outcome is not (IndexRegionOutcome.Applied or IndexRegionOutcome.Verified or IndexRegionOutcome.Unknown));
        var regions = existing.Select(region => Project(region, selection.Detail >= CliDetail.Standard)).ToArray();
        var renames = result.Entrypoints.SelectMany(entrypoint => result.Deletions.Where(deletion =>
            Path.GetDirectoryName(deletion.Path) == Path.GetDirectoryName(entrypoint.Path)
            && deletion.Outcome == entrypoint.Outcome && (preview || IsWritten(entrypoint.Outcome)))
            .Select(deletion => KeyValuePair.Create(entrypoint.Path, deletion.Path))).ToDictionary(StringComparer.Ordinal);
        var renamed = result.Entrypoints.Count(item => renames.ContainsKey(item.Path) && IsCounted(item.Outcome, preview));
        return new CliReport<IndexData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, preview: preview, updated: updated, checkedFiles: checkedFiles, renamed: renamed),
            HeadlineFindingCode = result.Findings.Count == 1
                && result.Findings[0].Details?.InputProblem == IndexInputProblem.Folder
                    ? result.Findings[0].MachineCode : null,
            Workspace = result.WorkspacePath is { } workspace ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit) : null,
            Findings = result.Findings.Select(finding => Finding(result, finding, updated)).ToArray(),
            Effects = regions.Where(region => region.ResultAction == IndexRegionAction.Update && !result.Metadata.Any(item => item.Path == region.Path)).Select(Effect)
                .Concat(result.Metadata.Where(item => !result.Entrypoints.Any(entrypoint => entrypoint.Path == item.Path))
                    .Select(item => CompletionEffect(item.Path, item.Outcome, CliEffectAction.Rewritten)))
                .Concat(result.Entrypoints.Select(item => CompletionEffect(item.Path, item.Outcome, CliEffectAction.Created)))
                .Concat(result.Deletions.Select(item => CompletionEffect(item.Path, item.Outcome, CliEffectAction.Deleted))).ToArray(),
            Counts =
            [
                new CliCount("filesChecked", global::OpenForge.Cli.OutputText.Index.IndexText.LabelFilesChecked(), checkedFiles),
                new CliCount("filesUpdated", global::OpenForge.Cli.OutputText.Index.IndexText.LabelFilesUpdated(), updated),
                new CliCount("filesCurrent", global::OpenForge.Cli.OutputText.Index.IndexText.LabelFilesCurrent(), current),
                new CliCount("metadataCompleted", "Metadata completed", result.Metadata.Count(item => IsCounted(item.Outcome, preview))),
                new CliCount("entrypointsCreated", "Entrypoints created", result.Entrypoints.Count(item => IsCounted(item.Outcome, preview)) - renamed),
                new CliCount("filesSkipped", "Files skipped", result.Findings.Count(item => item.Code == IndexFindingCode.MetadataSkipped)),
                new CliCount("navigationFilesRenamed", global::OpenForge.Cli.OutputText.Index.IndexWording.NavigationFilesRenamed(), renamed),
            ],
            Data = new IndexData
            {
                Mode = preview ? "dry-run" : "apply",
                Metadata = result.Metadata.Select(item => new IndexDataMetadata(item.Path, item.Fields, item.Description, item.Tags?.ToArray(), Outcome(item.Outcome))).ToArray(),
                Entrypoints = result.Entrypoints.Select(item => new IndexDataEntrypoint(item.Path, Outcome(item.Outcome))).ToArray(),
                Changes = regions.Where(region => region.ResultAction == IndexRegionAction.Update)
                    .Select(region => new IndexDataChange(region.Path, region.Before, region.After)
                    {
                        Diff = selection.Detail >= CliDetail.Standard ? region.Diff : null,
                    }).ToArray(),
                Unchanged = selection.Detail >= CliDetail.Standard
                    ? regions.Where(region => region.ResultAction == IndexRegionAction.Unchanged).Select(region => new IndexDataPath(region.Path)).ToArray() : null,
                Selection = selection.Detail >= CliDetail.Full ? new IndexDataSelection(
                    CliReportVocabulary.Name(result.Selection.Origin), CliReportVocabulary.Name(result.Selection.Scope),
                    result.Selection.Sources.Select(source => new IndexDataSource(source.Id, source.Path)).ToArray()) : null,
                Regions = selection.Detail >= CliDetail.Full ? regions : null,
                TextRegions = regions,
                Recovery = result.Recovery,
                NothingWritten = nothingWritten && result.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete,
                CurrentCount = current,
                TextDeletions = result.Deletions.Where(item => !renames.ContainsValue(item.Path)).Select(item => new IndexDataEntrypoint(item.Path, Outcome(item.Outcome))).ToArray(),
                TextRenames = renames,
            },
            Recovery = new CliRecovery(result.Recovery.ResidualPath, result.Recovery.State switch
            {
                IndexRecoveryState.NotRequired or IndexRecoveryState.NotCreated => CliRecoveryDisposition.NotRequired,
                IndexRecoveryState.Removed => CliRecoveryDisposition.Removed,
                IndexRecoveryState.Retained => CliRecoveryDisposition.Retained,
                IndexRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result)),
            }),
            Next = Next(result),
            Diagnostics = selection.Detail == CliDetail.Debug
                ? new[] { $"status={CliStatusDefinitions.Read(result.Status).MachineName}", $"mode={CliReportVocabulary.Name(result.Mode)}", $"regions={result.Regions.Count}" }
                    .Concat(result.Findings.Select(finding => finding.Cause)).ToArray() : [],
        };
    }

    private static CliHeadline Headline(IndexResult result, bool preview, int updated, int checkedFiles, int renamed)
    {
        if ((result.Metadata.Count > 0 || result.Entrypoints.Count > 0) && result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention or CliSemanticStatus.Incomplete)
            return new(global::OpenForge.Cli.OutputText.Index.IndexWording.Completion(metadata: result.Metadata.Count(item => IsCounted(item.Outcome, preview)),
                entrypoints: result.Entrypoints.Count(item => IsCounted(item.Outcome, preview)) - renamed, renames: renamed, preview: preview),
                preview ? CliHeadlineKind.Preview : CliHeadlineKind.Done);
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status);
        return result.Status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention when result.Counts.Updates == 0 =>
                new(IndexWording.Current(checkedFiles), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete when preview => new(IndexWording.Preview(updated, checkedFiles), CliHeadlineKind.Preview),
            CliSemanticStatus.Attention when preview && result.Counts.Updates > 0 => new(IndexWording.Preview(updated, checkedFiles), CliHeadlineKind.Preview),
            CliSemanticStatus.Complete => new(IndexWording.Updated(updated, checkedFiles), CliHeadlineKind.Done),
            CliSemanticStatus.Attention => new(IndexWording.Updated(updated, checkedFiles), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete when result.Findings.Any(finding => finding.Code == IndexFindingCode.MetadataSkipped) =>
                new(IndexWording.Partial(updated, checkedFiles, preview), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Incomplete => new(IndexWording.Incomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid => new(InvalidHeadline(first), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Blocked => new(IndexWording.Blocked(first?.Details?.ParentPath ?? first?.Source?.Path ?? result.WorkspacePath ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheWorkspace()), CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed => new(IndexWording.Failed(updated, result.Regions.Count), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted when result.Regions.All(region => region.Outcome is not (IndexRegionOutcome.Applied or IndexRegionOutcome.Verified or IndexRegionOutcome.Unknown)) =>
                new(IndexWording.Cancelled(), CliHeadlineKind.Cancelled),
            CliSemanticStatus.Interrupted => new(IndexWording.CancelledAfter(updated, result.Regions.Count), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
    }

    private static string InvalidHeadline(IndexFinding? finding)
    {
        if (finding is null) throw new InvalidOperationException("Invalid Index results require an input finding.");
        if (finding.Details?.Operand is not { } operand)
            return CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Index.IndexText.LabelIndex(), Reason(finding.Cause));
        var reason = finding.Details.InputProblem switch
        {
            IndexInputProblem.Folder => IndexWording.FolderReason(),
            IndexInputProblem.UnknownId => IndexWording.UnknownSource(operand),
            _ => finding.Cause,
        };
        return IndexWording.CannotIndex(operand, Reason(reason));
    }

    private static IndexDataRegion Project(IndexRegion region, bool includeDiff)
        => new(region.Source.Path, CliReportVocabulary.Name(region.Action), CliReportVocabulary.Name(region.Outcome))
        {
            Before = region.BeforeEntryCount,
            After = region.ExpectedEntryCount,
            ResultAction = region.Action,
            ResultOutcome = region.Outcome,
            Diff = includeDiff && region.Change is { } change ? Diff(change) : [],
        };

    private static IReadOnlyList<string> Diff(IndexChange change)
        => Lines(change.BeforeBody, "- ").Concat(Lines(change.ExpectedBody, "+ ")).ToArray();

    private static IEnumerable<string> Lines(string source, string prefix)
    {
        for (var start = 0; start < source.Length;)
        {
            var end = start;
            while (end < source.Length && source[end] is not ('\r' or '\n')) end++;
            var next = end;
            if (next < source.Length && source[next++] == '\r' && next < source.Length && source[next] == '\n') next++;
            if (!string.IsNullOrWhiteSpace(source[start..end])) yield return prefix + source[start..next];
            start = next;
        }
    }

    private static CliEffect Effect(IndexDataRegion region) => new()
    {
        Path = region.Path,
        Kind = CliEffectKind.Section,
        Action = CliEffectAction.Rewritten,
        Outcome = region.ResultOutcome switch
        {
            IndexRegionOutcome.NotRequested => CliEffectOutcome.Planned,
            IndexRegionOutcome.Applied or IndexRegionOutcome.Verified => CliEffectOutcome.Done,
            IndexRegionOutcome.NotStarted => CliEffectOutcome.NotStarted,
            IndexRegionOutcome.NotEstablished or IndexRegionOutcome.Unknown => CliEffectOutcome.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(region)),
        },
    };

    private static CliFinding Finding(IndexResult result, IndexFinding finding, int updated)
    {
        var path = finding.Source?.Path ?? finding.Details?.Operand ?? result.WorkspacePath ?? "index";
        var reason = Reason(finding.Cause);
        var recovery = result.Recovery.ResidualPath ?? "unknown";
        var message = finding.Code switch
        {
            IndexFindingCode.InvalidInput => CliFindingWording.InvalidInput(global::OpenForge.Cli.OutputText.Index.IndexText.LabelIndex(), reason),
            IndexFindingCode.InvalidSource when finding.Details?.InputProblem == IndexInputProblem.UnknownId => IndexWording.UnknownSource(path),
            IndexFindingCode.InvalidSource when finding.Details?.InputProblem == IndexInputProblem.Folder => IndexWording.FolderSource(path),
            IndexFindingCode.InvalidSource => finding.Cause,
            IndexFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(path),
            IndexFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(path, reason),
            IndexFindingCode.SourceAmbiguous => CliFindingWording.SourceAmbiguous(path),
            IndexFindingCode.SourceUnsafe => CliFindingWording.SourceUnsafe(path),
            IndexFindingCode.TopologyAmbiguous => IndexWording.TopologyAmbiguous(path, reason),
            IndexFindingCode.TargetUnexposed => IndexWording.TargetUnexposed(path),
            IndexFindingCode.TargetUnsafe => CliFindingWording.TargetUnsafe(path, reason),
            IndexFindingCode.MetadataUnsafe when finding.Details?.MetadataProblem == IndexMetadataProblem.UnclosedFrontmatter => IndexWording.FrontmatterUnclosed(),
            IndexFindingCode.MetadataUnsafe => CliFindingWording.MetadataUnsafe(path, reason),
            IndexFindingCode.MetadataOptional => IndexWording.MetadataOptional(path),
            IndexFindingCode.GeneratedRegionUnsafe => CliFindingWording.GeneratedRegionUnsafe(path, reason),
            IndexFindingCode.WorkspaceLockUnavailable => CliFindingWording.WorkspaceLockUnavailable(),
            IndexFindingCode.TargetChanged => CliFindingWording.TargetChanged(path),
            IndexFindingCode.RecoveryConflict => CliFindingWording.RecoveryConflict(path),
            IndexFindingCode.DiscoveryIncomplete => IndexWording.DiscoveryIncomplete(path),
            IndexFindingCode.MetadataIncomplete => CliFindingWording.MetadataIncomplete(path),
            IndexFindingCode.MetadataSkipped when finding.Details?.IsNativeSkill == true => IndexWording.MetadataSkipped(path),
            IndexFindingCode.MetadataSkipped => global::OpenForge.Cli.OutputText.Index.IndexWording.MetadataSkipped(path, result.Mode == IndexMode.DryRun),
            IndexFindingCode.ProjectionIncomplete => CliFindingWording.ProjectionUnavailable(path, reason),
            IndexFindingCode.RecoveryUnavailable => CliFindingWording.RecoveryUnavailable(path),
            IndexFindingCode.RecoveryArtifactRetained => CliFindingWording.RecoveryRetained(recovery),
            IndexFindingCode.TargetChangedDuringApply => CliFindingWording.TargetChangedDuringApply(path, updated, result.Regions.Count),
            IndexFindingCode.WriteFailed => CliFindingWording.WriteFailed(path, updated, result.Regions.Count, recovery),
            IndexFindingCode.VerificationFailed => CliFindingWording.VerificationFailed(path, recovery),
            IndexFindingCode.RecoveryFailed => CliFindingWording.RecoveryFailed(),
            IndexFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Index.IndexText.TitleIndex(), reason),
            IndexFindingCode.Interrupted => result.Regions.All(region => region.Outcome is not (IndexRegionOutcome.Applied or IndexRegionOutcome.Verified or IndexRegionOutcome.Unknown))
                ? IndexWording.Cancelled() : IndexWording.CancelledAfter(updated, result.Regions.Count),
            _ => throw new ArgumentOutOfRangeException(nameof(finding)),
        };
        return new CliFinding
        {
            Severity = finding.Code == IndexFindingCode.MetadataSkipped && finding.Details?.IsNativeSkill != true ? CliSeverity.Error : CliReportVocabulary.Severity(finding.Status),
            Code = finding.MachineCode,
            Title = finding.Code == IndexFindingCode.MetadataUnsafe ? IndexWording.FrontmatterInvalidTitle() : IndexWording.FindingTitle(finding.Code),
            Message = message,
            Subject = new CliSubject(finding.Source is null ? CliSubjectKind.Identifier : CliSubjectKind.Source, finding.Source?.Path,
                finding.Source?.Id ?? path, finding.Details is { Line: { } line, Column: { } column } ? new CliSourceLocation(line, column) : null),
            Evidence = finding.Code == IndexFindingCode.MetadataSkipped ? [new CliEvidence("Cause", finding.Cause)] : [],
            Actions = finding.Code == IndexFindingCode.MetadataSkipped && finding.Details?.IsNativeSkill != true ? [new CliNextAction(EditFrontmatter(result, path), EditFrontmatter(result, path)) { Kind = CliNextActionKind.Sentence },
                new CliNextAction(finding.Cause, finding.Cause) { Kind = CliNextActionKind.Sentence }] : [],
            Candidates = finding.Candidates.Select(candidate => new CliCandidate(new CliSubject(CliSubjectKind.Source, candidate.Path, candidate.Id), [])).ToArray(),
        };
    }

    private static CliNextAction? Next(IndexResult result)
        => result.Status switch
        {
            CliSemanticStatus.Complete when result.Mode == IndexMode.DryRun && result.Counts.Updates == 0 && result.Metadata.Count == 0 && result.Entrypoints.Count == 0 => null,
            CliSemanticStatus.Complete when result.Mode == IndexMode.DryRun => new CliNextAction("open-forge index", global::OpenForge.Cli.OutputText.Index.IndexWording.ApplyPreview()) { AppliesRequest = true },
            CliSemanticStatus.Complete => new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Index.IndexWording.Verify()),
            CliSemanticStatus.Invalid when result.Findings.FirstOrDefault(finding => finding.Details?.CorrectedSourceId is not null)?.Details?.CorrectedSourceId is { } id =>
                new CliNextAction(IndexWording.CorrectedSource(id), IndexWording.ChooseSource()),
            CliSemanticStatus.Invalid when result.Findings.Any(finding => finding.Code == IndexFindingCode.InvalidSource) =>
                new CliNextAction("open-forge route list --depth=all", IndexWording.ChooseSource()),
            CliSemanticStatus.Invalid => new CliNextAction("open-forge index --help", IndexWording.CorrectInput()),
            CliSemanticStatus.Blocked when result.Findings.Any(finding => finding.Code == IndexFindingCode.WorkspaceLockUnavailable) =>
                new CliNextAction("open-forge index", IndexWording.RetryLock()),
            CliSemanticStatus.Attention when result.Findings.Count > 0
                && result.Findings.All(finding => finding.Code == IndexFindingCode.MetadataOptional) => null,
            CliSemanticStatus.Attention => new CliNextAction("open-forge cleanup", IndexWording.InspectRecovery()),
            CliSemanticStatus.Interrupted => new CliNextAction("open-forge index", IndexWording.Retry()),
            CliSemanticStatus.Incomplete when result.Findings.FirstOrDefault(finding => finding.Code == IndexFindingCode.MetadataSkipped && finding.Details?.IsNativeSkill != true)?.Source?.Path is { } path =>
                new CliNextAction(EditFrontmatter(result, path), EditFrontmatter(result, path)) { Kind = CliNextActionKind.Sentence },
            _ => new CliNextAction("open-forge doctor", IndexWording.Inspect()),
        };

    private static bool IsWritten(IndexRegionOutcome outcome) => outcome is IndexRegionOutcome.Applied or IndexRegionOutcome.Verified or IndexRegionOutcome.Unknown;
    private static string EditFrontmatter(IndexResult result, string path)
        => global::OpenForge.Cli.OutputText.Index.IndexWording.EditFrontmatter(path,
            CliRetryCommand.BuildInWorkspace(["index"], result.WorkspacePath, result.WorkspaceExplicit));
    private static bool IsCounted(IndexRegionOutcome outcome, bool preview) => outcome == IndexRegionOutcome.Verified || preview && outcome == IndexRegionOutcome.NotRequested;
    private static string Outcome(IndexRegionOutcome outcome) => outcome switch
    {
        IndexRegionOutcome.NotRequested => "planned",
        IndexRegionOutcome.Verified => "done",
        IndexRegionOutcome.NotStarted => "not-started",
        IndexRegionOutcome.Applied or IndexRegionOutcome.Unknown => "unknown",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
    };
    private static CliEffect CompletionEffect(string path, IndexRegionOutcome outcome, CliEffectAction action) => new()
    {
        Path = path,
        Kind = CliEffectKind.File,
        Action = action,
        Outcome = outcome switch
        {
            IndexRegionOutcome.NotRequested => CliEffectOutcome.Planned,
            IndexRegionOutcome.Verified => CliEffectOutcome.Done,
            IndexRegionOutcome.NotStarted => CliEffectOutcome.NotStarted,
            IndexRegionOutcome.Applied or IndexRegionOutcome.Unknown => CliEffectOutcome.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        },
    };

    private static string Reason(string sentence) => sentence.TrimEnd('.');
}
