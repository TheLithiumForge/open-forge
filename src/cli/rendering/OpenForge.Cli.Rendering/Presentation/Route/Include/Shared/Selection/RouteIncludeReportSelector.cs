using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Include.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Route.Shared.Inclusion.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.Include.Shared.Selection;

internal static class RouteIncludeReportSelector
{
    internal static CliReport<RouteIncludeData> Select(RouteIncludeResult result, CliSelection selection)
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
        var sentence = kind switch
        {
            CliHeadlineKind.NothingToDo => global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.NothingChanged(path),
            CliHeadlineKind.Preview => global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.Preview(path),
            CliHeadlineKind.Done or CliHeadlineKind.Warnings => global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.Done(path),
            _ => global::OpenForge.Cli.OutputText.Route.Include.RouteIncludeText.Failure(outcome.Cause ?? "The request could not complete."),
        };
        var code = outcome.Code is { } findingCode ? "route-include." + RouteInclusionWireVocabulary.Finding(findingCode) : null;
        var recovery = RouteInclusionWireVocabulary.Recovery(outcome.Recovery);
        return new CliReport<RouteIncludeData>
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
                    Title = outcome.Cause ?? sentence, Message = outcome.Cause ?? sentence,
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
            Data = new RouteIncludeData
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
