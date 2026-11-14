using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool.Shared.Writing;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;

internal static class ToolWriteReportSelector
{
    internal static CliReport<ToolWriteData> Select(ToolWriteResult result, CliSelection selection)
        => new()
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
            Findings = [.. result.Findings.Select(finding => new CliFinding
            {
                Code = string.Concat(result.Command.Replace(' ', '-'), ".", Suffix(finding.Code)),
                Severity = finding.Status is CliSemanticStatus.Attention or CliSemanticStatus.Incomplete ? CliSeverity.Warning : CliSeverity.Error,
                Title = ToolWriteText.Finding(Suffix(finding.Code)),
                Message = finding.Cause ?? ToolWriteText.Finding(Suffix(finding.Code)),
                Subject = new(CliSubjectKind.Identifier, Path: finding.Path, Id: finding.ToolId ?? result.Command),
            })],
            Data = result.Data,
            Effects = Effects(result),
            Next = result.Next,
            Counts = [new("tools", ToolWriteText.ToolCountLabel(result.Data.Tools.Count), result.Data.Tools.Count), new("completedSteps", "Completed steps",
                result.Data.Outcomes.Count(outcome => outcome.State == ToolWriteStages.Completed))],
            Diagnostics = selection.Detail == CliDetail.Debug
                ? [.. result.Findings.Select(finding => Suffix(finding.Code))] : [],
        };

    private static CliHeadline Headline(ToolWriteResult result) => result.Status switch
    {
        CliSemanticStatus.Complete when result.Data.Grants.Count != 0 && (result.Data.Tools.Count == 0 || result.Data.Tools.All(tool => tool.NoOp))
            => new(ToolWriteText.GrantOnly(result.Data.Preview), result.Data.Preview ? CliHeadlineKind.Preview : CliHeadlineKind.Done),
        CliSemanticStatus.Complete when result.Data.Tools.Count == 0 || result.Data.Tools.All(tool => tool.NoOp) => new(ToolWriteText.NothingToDo(result.Command), CliHeadlineKind.NothingToDo),
        CliSemanticStatus.Complete => new(ToolWriteText.Completed(result.Command, result.Data.Preview, result.Data.Tools.Count),
            result.Data.Preview ? CliHeadlineKind.Preview : CliHeadlineKind.Done),
        CliSemanticStatus.Attention => new(ToolWriteText.Warnings(result.Command), CliHeadlineKind.Warnings),
        CliSemanticStatus.Incomplete => new(ToolWriteText.Incomplete(result.Command), CliHeadlineKind.Incomplete),
        CliSemanticStatus.Invalid => new(ToolWriteText.CannotStart(result.Command), CliHeadlineKind.CannotStart),
        CliSemanticStatus.Blocked => new(ToolWriteText.CannotStart(result.Command), CliHeadlineKind.Blocked),
        CliSemanticStatus.Failed => new(ToolWriteText.Failed(result.Command), CliHeadlineKind.Failed),
        CliSemanticStatus.Interrupted => new(ToolWriteText.Cancelled(result.Command), CliHeadlineKind.Cancelled),
        _ => throw new ArgumentOutOfRangeException(nameof(result)),
    };

    private static IReadOnlyList<CliEffect> Effects(ToolWriteResult result)
        => [.. result.Data.Tools.SelectMany(tool => tool.Changes
            .Where(change => change.Path is not null && change.Disposition is not ("NoChange" or "Keep" or "Conflict"))
            .Select(change => new CliEffect
            {
                Path = change.Path ?? string.Empty, Kind = CliEffectKind.File, Owner = tool.Id,
                Action = change.Disposition == "Remove" ? CliEffectAction.Deleted : CliEffectAction.Rewritten,
                Outcome = Outcome(result, tool.Id, change.Id),
            })), .. result.Data.Grants.SelectMany(grant => grant.WrittenPaths.Select(path => new CliEffect
            {
                Path = path, Kind = CliEffectKind.File, Owner = result.Command,
                Action = CliEffectAction.Rewritten, Outcome = CliEffectOutcome.Done,
            }))];

    private static CliEffectOutcome Outcome(ToolWriteResult result, string id, string change)
    {
        if (result.Data.Preview) return CliEffectOutcome.Planned;
        if (result.Data.Outcomes.Any(outcome => outcome.ToolId == id && outcome.Completed.Contains(change, StringComparer.Ordinal)))
            return CliEffectOutcome.Done;
        return CliEffectOutcome.NotStarted;
    }

    internal static string Suffix(ToolWriteFindingCode code) => code switch
    {
        ToolWriteFindingCode.InvalidInput => "invalid-input",
        ToolWriteFindingCode.RecordsUnavailable => "records-unavailable",
        ToolWriteFindingCode.UnknownTool => "unknown-tool",
        ToolWriteFindingCode.ChannelInvalid => "channel-invalid",
        ToolWriteFindingCode.UpdateRequiresRecord => "update-requires-record",
        ToolWriteFindingCode.ChannelBlocked => "channel-blocked",
        ToolWriteFindingCode.ProfileUnavailable => "profile-unavailable",
        ToolWriteFindingCode.OpenForgeVersionUnsupported => "open-forge-version-unsupported",
        ToolWriteFindingCode.NativePlanBlocked => "native-plan-blocked",
        ToolWriteFindingCode.ConfigConflict => "config-conflict",
        ToolWriteFindingCode.MissingConsent => "missing-consent",
        ToolWriteFindingCode.ManagerRequired => "manager-required",
        ToolWriteFindingCode.NativeFailed => "native-failed",
        ToolWriteFindingCode.VerificationFailed => "verification-failed",
        ToolWriteFindingCode.ConfigFailed => "config-failed",
        ToolWriteFindingCode.RecordFailed => "record-failed",
        ToolWriteFindingCode.BackupFailed => "backup-failed",
        ToolWriteFindingCode.ExpectedStateChanged => "expected-state-changed",
        ToolWriteFindingCode.PermissionRequired => "permission-required",
        ToolWriteFindingCode.Interrupted => "interrupted",
        ToolWriteFindingCode.UnexpectedFailure => "unexpected-failure",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
