using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Remove.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Remove.Shared.Selection;

internal static class RemoveReportSelector
{
    internal static CliReport<RemoveData> Select(RemoveResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var headlineFinding = result.Findings.FirstOrDefault();
        return new CliReport<RemoveData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = headlineFinding is null ? null : FindingCode(headlineFinding.Code),
            Workspace = result.WorkspacePath is { } workspacePath
                ? new CliWorkspaceEcho(workspacePath, result.WorkspaceExplicit)
                : null,
            Findings = result.Findings.Select(finding => Finding(finding, result.Next)).ToArray(),
            Effects = result.Effects.Select(Effect).ToArray(),
            Counts = result.Removed.Count == 0
                ? []
                : [new CliCount("removed", "removed", result.Removed.Count)],
            Data = new RemoveData
            {
                Kind = result.Kind,
                Target = result.Target,
                Mode = result.DryRun ? "dry-run" : "apply",
                Removed = result.Removed,
                Effects = result.Effects.Select(effect => new RemoveDataEffect
                {
                    Path = effect.Path,
                    Kind = effect.Kind,
                    Action = effect.Action,
                    Outcome = effect.Outcome,
                }).ToArray(),
                RecoveryPath = result.RecoveryPath,
                RecoveryDisposition = result.RecoveryDisposition,
                ShowEffects = selection.Detail >= CliDetail.Standard,
            },
            Recovery = new CliRecovery(result.RecoveryPath, ReadRecoveryDisposition(result.RecoveryDisposition)),
            Next = result.Next,
        };
    }

    private static CliHeadline Headline(RemoveResult result)
    {
        if (result.Status == CliSemanticStatus.Complete)
        {
            if (result.DryRun)
            {
                return new CliHeadline(global::OpenForge.Cli.OutputText.Remove.RemoveText.Preview(result.Target), CliHeadlineKind.Preview);
            }
            if (result.Removed.Count > 0)
            {
                return new CliHeadline(global::OpenForge.Cli.OutputText.Remove.RemoveText.Done(result.Target), CliHeadlineKind.Done);
            }
            if (result.Effects.Any(effect => effect.Outcome == "done"))
            {
                return new CliHeadline(global::OpenForge.Cli.OutputText.Remove.RemoveText.Reconciled(result.Target), CliHeadlineKind.Done);
            }
            return new CliHeadline(global::OpenForge.Cli.OutputText.Remove.RemoveText.Unchanged(result.Target), CliHeadlineKind.NothingToDo);
        }

        var message = result.Findings.FirstOrDefault()?.Message ?? "Remove could not complete.";
        var kind = result.Status switch
        {
            CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
            CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
            CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
            CliSemanticStatus.Failed => CliHeadlineKind.Failed,
            CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
            CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
            _ => CliHeadlineKind.Failed,
        };
        return new CliHeadline(message, kind);
    }

    private static CliFinding Finding(RemoveFinding finding, CliNextAction? next)
        => new()
        {
            Severity = finding.Status == CliSemanticStatus.Attention ? CliSeverity.Warning : CliSeverity.Error,
            Code = FindingCode(finding.Code),
            Title = finding.Code.ToString(),
            Message = finding.Message,
            Subject = new CliSubject(CliSubjectKind.File, finding.Target),
            Resolution = CliResolution.TargetedOperation,
            Actions = next is null ? [] : [next],
        };

    private static string FindingCode(RemoveFindingCode code)
        => "remove." + ToKebabCase(code.ToString());

    private static CliEffect Effect(RemoveEffect effect)
        => new()
        {
            Path = effect.Path,
            Kind = effect.Kind switch
            {
                "directory" => CliEffectKind.Directory,
                "link" => CliEffectKind.Link,
                "record" => CliEffectKind.Record,
                "setting" => CliEffectKind.Setting,
                _ => CliEffectKind.File,
            },
            Action = effect.Action switch
            {
                "create" => CliEffectAction.Created,
                "persist" or "update" => CliEffectAction.Replaced,
                "record-removal" or "release-ownership" => CliEffectAction.Released,
                "delete" => CliEffectAction.Deleted,
                _ => CliEffectAction.Kept,
            },
            Outcome = effect.Outcome switch
            {
                "planned" => CliEffectOutcome.Planned,
                "done" => CliEffectOutcome.Done,
                "unknown" => CliEffectOutcome.Unknown,
                "failed" => CliEffectOutcome.Failed,
                _ => CliEffectOutcome.NotStarted,
            },
        };

    private static CliRecoveryDisposition ReadRecoveryDisposition(string value)
        => value switch
        {
            "not-required" => CliRecoveryDisposition.NotRequired,
            "removed" => CliRecoveryDisposition.Removed,
            "retained" => CliRecoveryDisposition.Retained,
            "failed" => CliRecoveryDisposition.Failed,
            _ => CliRecoveryDisposition.Unknown,
        };

    private static string ToKebabCase(string value)
    {
        var result = new System.Text.StringBuilder(value.Length + 8);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsUpper(character) && index > 0)
            {
                result.Append('-');
            }
            result.Append(char.ToLowerInvariant(character));
        }
        return result.ToString();
    }
}
