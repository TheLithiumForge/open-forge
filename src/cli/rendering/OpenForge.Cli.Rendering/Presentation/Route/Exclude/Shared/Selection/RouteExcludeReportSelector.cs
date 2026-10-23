using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Route.Shared.Inclusion.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Selection;

internal static class RouteExcludeReportSelector
{
    internal static CliReport<RouteExcludeData> Select(RouteExcludeResult result, CliSelection selection)
    {
        var outcome = result.Outcome;
        var path = outcome.Path ?? result.Requested;
        var kind = result.Status switch
        {
            CliSemanticStatus.Complete when outcome.NoOp => CliHeadlineKind.NothingToDo,
            CliSemanticStatus.Complete when result.DryRun => CliHeadlineKind.Preview,
            CliSemanticStatus.Complete => CliHeadlineKind.Done,
            CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
            CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
            CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
            CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
            CliSemanticStatus.Failed => CliHeadlineKind.Failed,
            CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };
        var lockMessage = outcome.LockFailure is { } lockFailure ? CliFindingWording.WorkspaceLockUnavailable(lockFailure) : null;
        var sentence = kind switch
        {
            CliHeadlineKind.NothingToDo => global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.NothingChanged(path),
            CliHeadlineKind.Preview => global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.Preview(path),
            CliHeadlineKind.Done or CliHeadlineKind.Warnings => global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.Done(path),
            _ => global::OpenForge.Cli.OutputText.Route.Exclude.RouteExcludeText.Failure(lockMessage ?? outcome.Cause ?? "The request could not complete."),
        };
        var code = outcome.Code is { } findingCode ? "route-exclude." + RouteInclusionWireVocabulary.Finding(findingCode) : null;
        var recovery = RouteInclusionWireVocabulary.Recovery(outcome.Recovery);
        return new CliReport<RouteExcludeData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = new CliHeadline(sentence, kind),
            HeadlineFindingCode = result.Status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Incomplete ? code : null,
            Workspace = result.WorkspacePath is { } workspace ? new CliWorkspaceEcho(workspace, result.WorkspaceExplicit) : null,
            Findings = code is null ? [] :
            [
                new CliFinding
                {
                    Code = code, Severity = CliReportVocabulary.Severity(result.Status),
                    LockFailure = outcome.LockFailure,
                    Title = lockMessage is null ? outcome.Cause ?? sentence : global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceLockIsUnavailable(),
                    Message = lockMessage ?? outcome.Cause ?? sentence,
                    Subject = new CliSubject(CliSubjectKind.File, Path: path),
                    Actions = result.Next is { } next ? [next] : [],
                },
            ],
            Effects = outcome.Effects.Select(effect => new CliEffect
            {
                Path = effect.Path,
                Kind = effect.Path == ".agents/loader.md" ? CliEffectKind.Section : CliEffectKind.Setting,
                Action = effect.Before is null ? CliEffectAction.Created : CliEffectAction.Replaced,
                Outcome = RouteInclusionWireVocabulary.Effect(effect.State),
                Before = selection.Detail >= CliDetail.Full ? effect.Before : null,
                After = selection.Detail >= CliDetail.Full ? effect.After : null,
            }).ToArray(),
            Counts = [new CliCount("filesChanged", "Files changed", outcome.Effects.Length)],
            Recovery = new CliRecovery(outcome.RecoveryPath, recovery),
            Next = result.Next,
            Data = new RouteExcludeData
            {
                Mode = result.DryRun ? "dry-run" : "apply",
                Target = path,
                Before = outcome.Before,
                After = outcome.After,
                RegisteredBefore = outcome.RegisteredBefore,
                RegisteredAfter = outcome.RegisteredAfter,
                Verified = outcome.Verified,
            },
        };
    }
}
