using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Stripping.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.OutputText.Strip;
using OpenForge.Cli.OutputText.Uninstall;

namespace OpenForge.Cli.Core.Presentation.Stripping.Shared.Selection;

internal static class StripReportSelector
{
    private const string DryRunMode = "dry-run";
    private const string ApplyMode = "apply";

    internal static CliReport<StripData> Select(StripResult result, CliSelection selection)
    {
        var error = result.Findings.FirstOrDefault(finding => finding.Severity == StripFindingSeverity.Error);
        var full = selection.Detail >= CliDetail.Full;
        return new CliReport<StripData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result, error),
            HeadlineFindingCode = error is not null && result.Status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete
                ? StripWireVocabulary.FindingCode(result.Command, error.Code)
                : null,
            Workspace = result.WorkspacePath is { } workspace ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit) : null,
            Findings = [.. result.Findings.Select(finding => Finding(result, finding))],
            Effects = [.. result.Plan.SelectMany(entry => Effects(entry, full))],
            Counts = Counts(result),
            Recovery = new CliRecovery(result.RecoveryPath, result.Recovery switch
            {
                StripRecoveryState.NotRequired => CliRecoveryDisposition.NotRequired,
                StripRecoveryState.Removed => CliRecoveryDisposition.Removed,
                StripRecoveryState.Retained => CliRecoveryDisposition.Retained,
                StripRecoveryState.Unknown => CliRecoveryDisposition.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result.Recovery, "The strip recovery state is not defined."),
            }),
            Next = result.Next,
            Data = new StripData
            {
                Mode = result.DryRun ? DryRunMode : ApplyMode,
                Strip = [.. result.PartNames],
                Flatten = result.Flatten,
                Plan = [.. result.Plan.Select(entry => new StripDataPlanEntry
                {
                    Action = StripWireVocabulary.ActionName(entry.Action),
                    Path = entry.Path,
                    To = entry.To,
                    Reason = StripWireVocabulary.ReasonName(entry.Reason),
                    Parts = [.. entry.Parts.Select(StripWireVocabulary.RewritePartName)],
                    Before = entry.Before,
                    After = entry.After,
                    Outcome = OutcomeName(entry.State),
                })],
                Kept = [.. result.Kept.Select(kept => new StripDataKept(kept.Path, StripWireVocabulary.KeptReasonName(kept.Reason)))],
                Collisions = [.. result.Collisions.Select(collision => new StripDataCollision(collision.Path, collision.Sources))],
                Verified = result.Verified,
            },
        };
    }

    private static CliHeadline Headline(StripResult result, StripFinding? error)
    {
        var changes = result.Plan.Length;
        var uninstall = result.Kind == StripCommandKind.Uninstall;
        if (result.Status == CliSemanticStatus.Complete && result.NoOp)
        {
            return new CliHeadline(uninstall ? UninstallText.NothingToDo() : StripText.NothingToDo(), CliHeadlineKind.NothingToDo);
        }

        if (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
        {
            var kind = result.Status == CliSemanticStatus.Attention ? CliHeadlineKind.Warnings : CliHeadlineKind.Done;
            if (result.DryRun)
            {
                return new CliHeadline(uninstall ? UninstallText.Preview(changes) : StripText.Preview(changes), CliHeadlineKind.Preview);
            }

            return new CliHeadline(uninstall ? UninstallText.Done(changes) : StripText.Done(changes), kind);
        }

        var message = error?.LockFailure is { } lockFailure
            ? CliFindingWording.WorkspaceLockUnavailable(lockFailure)
            : error?.Message ?? result.Findings.FirstOrDefault()?.Message ?? string.Empty;
        return new CliHeadline(StripText.Failure(message), result.Status switch
        {
            CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
            CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
            CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
            CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
            _ => CliHeadlineKind.Failed,
        });
    }

    private static CliFinding Finding(StripResult result, StripFinding finding)
    {
        IReadOnlyList<CliNextAction> actions = finding.Severity == StripFindingSeverity.Error && result.Next is { } next ? [next] : [];
        return new()
        {
            Code = StripWireVocabulary.FindingCode(result.Command, finding.Code),
            Severity = finding.Severity switch
            {
                StripFindingSeverity.Error => CliSeverity.Error,
                StripFindingSeverity.Warning => CliSeverity.Warning,
                StripFindingSeverity.Info => CliSeverity.Info,
                _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Severity, "The strip finding severity is not defined."),
            },
            LockFailure = finding.LockFailure,
            Title = finding.LockFailure is not null
                ? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable()
                : StripText.FindingTitle(StripWireVocabulary.FindingSuffix(finding.Code)),
            Message = finding.LockFailure is { } lockFailure ? CliFindingWording.WorkspaceLockUnavailable(lockFailure) : finding.Message,
            Subject = Subject(result, finding),
            Resolution = Resolution(finding, actions),
            Actions = actions,
        };
    }

    private static CliSubject Subject(StripResult result, StripFinding finding)
    {
        if (finding.Path is { } path)
        {
            return new CliSubject(CliSubjectKind.File, Path: path);
        }

        return result.WorkspacePath is { } workspace
            ? new CliSubject(CliSubjectKind.Workspace, Path: workspace)
            : new CliSubject(CliSubjectKind.Identifier, Id: result.Command);
    }

    private static CliResolution Resolution(StripFinding finding, IReadOnlyList<CliNextAction> actions)
    {
        if (finding.Severity == StripFindingSeverity.Info)
        {
            return CliResolution.Informational;
        }

        return actions.Count > 0 ? CliResolution.TargetedOperation : CliResolution.ManualDecision;
    }

    private static IEnumerable<CliEffect> Effects(StripPlanEntry entry, bool full)
    {
        var outcome = entry.State switch
        {
            StripEffectState.Planned => CliEffectOutcome.Planned,
            StripEffectState.NotStarted => CliEffectOutcome.NotStarted,
            StripEffectState.Done => CliEffectOutcome.Done,
            StripEffectState.Unknown => CliEffectOutcome.Unknown,
            StripEffectState.Failed => CliEffectOutcome.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(entry), entry.State, "The strip effect state is not defined."),
        };
        var reason = StripWireVocabulary.ReasonName(entry.Reason);
        switch (entry.Action)
        {
            case StripPlanAction.Move:
                yield return new CliEffect { Path = entry.Path, Kind = CliEffectKind.File, Action = CliEffectAction.Deleted, Outcome = outcome, Reason = reason, Before = full ? entry.Before : null };
                yield return new CliEffect { Path = entry.To!, Kind = CliEffectKind.File, Action = CliEffectAction.Created, Outcome = outcome, Reason = reason, After = full ? entry.After : null };
                break;
            case StripPlanAction.DeleteDirectory:
                yield return new CliEffect { Path = entry.Path, Kind = CliEffectKind.Directory, Action = CliEffectAction.Deleted, Outcome = outcome, Reason = reason };
                break;
            case StripPlanAction.Delete:
                yield return new CliEffect { Path = entry.Path, Kind = CliEffectKind.File, Action = CliEffectAction.Deleted, Outcome = outcome, Reason = reason, Before = full ? entry.Before : null };
                break;
            case StripPlanAction.Rewrite:
                yield return new CliEffect { Path = entry.Path, Kind = CliEffectKind.File, Action = CliEffectAction.Rewritten, Outcome = outcome, Reason = reason, Before = full ? entry.Before : null, After = full ? entry.After : null };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entry), entry.Action, "The strip plan action is not defined.");
        }
    }

    private static IReadOnlyList<CliCount> Counts(StripResult result)
    {
        if (result.Plan.IsEmpty && result.Kept.IsEmpty)
        {
            return [];
        }

        var deleted = result.Plan.Count(entry => entry.Action == StripPlanAction.Delete);
        var rewritten = result.Plan.Count(entry => entry.Action == StripPlanAction.Rewrite);
        var moved = result.Plan.Count(entry => entry.Action == StripPlanAction.Move);
        var folders = result.Plan.Count(entry => entry.Action == StripPlanAction.DeleteDirectory);
        var edited = result.Kept.Count(kept => kept.Reason == StripKeptReason.Edited);
        return
        [
            new CliCount("filesDeleted", StripText.LabelFilesDeleted(deleted), deleted),
            new CliCount("filesRewritten", StripText.LabelFilesRewritten(rewritten), rewritten),
            new CliCount("filesMoved", StripText.LabelFilesMoved(moved), moved),
            new CliCount("foldersDeleted", StripText.LabelFoldersDeleted(folders), folders),
            new CliCount("filesKept", StripText.LabelFilesKept(edited), edited),
        ];
    }

    private static string OutcomeName(StripEffectState state)
        => state switch
        {
            StripEffectState.Planned => "planned",
            StripEffectState.NotStarted => "not-started",
            StripEffectState.Done => "done",
            StripEffectState.Unknown => "unknown",
            StripEffectState.Failed => "failed",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The strip effect state is not defined."),
        };
}
