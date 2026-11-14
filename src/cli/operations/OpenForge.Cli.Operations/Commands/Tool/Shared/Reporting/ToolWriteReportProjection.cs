using OpenForge.Cli.Core.Commands.Tool.Choose.Models;
using OpenForge.Cli.Core.Commands.Tool.Configure.Models;
using OpenForge.Cli.Core.Commands.Tool.Install.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Remove.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Update.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;

internal static class ToolWriteReportProjection
{
    internal static ToolWriteReport Read(ToolInstallResult result)
        => new(Data(result.Data) with
        {
            Tools = [.. result.Tools.Select(item => new ToolReportInstallItem(item.Id, CliStatusDefinitions.Read(item.Status).MachineName,
                Data(item.Data), Issues(result.Command, item.Issues)))],
        }, Issues(result.Command, result.Issues));

    internal static ToolWriteReport Read(ToolConfigureResult result) => new(Data(result.Data), Issues(result.Command, result.Issues));
    internal static ToolWriteReport Read(ToolChooseResult result) => new(Data(result.Data), Issues(result.Command, result.Issues));
    internal static ToolWriteReport Read(ToolUpdateResult result) => new(Data(result.Data), Issues(result.Command, result.Issues));
    internal static ToolWriteReport Read(ToolRemoveResult result) => new(Data(result.Data), Issues(result.Command, result.Issues));

    private static ToolWriteReportIssue[] Issues(string command, ToolWriteIssue[] issues)
        => [.. issues.Select(issue => new ToolWriteReportIssue($"{command.Replace(' ', '-')}.{ToolContractCodes.Suffix(issue.Code)}", issue.Status, issue.Subject, issue.Cause))];

    private static ToolWriteReportData Data(ToolWriteData data) => new()
    {
        Mode = Mode(data.Mode),
        Id = data.Id,
        Role = data.Role,
        Pin = data.Pin,
        Plan = [.. data.Plan.Select(item => new ToolReportPlannedChange(item.Kind, item.Subject, item.Action, item.Before, item.After, item.Requires, item.Verification))],
        Permissions = [.. data.Permissions.Select(item => new ToolReportDestination(item.Path, item.Allowed, item.SaveGrant, item.Cause))],
        Consents = [.. data.Consents.Select(item => new ToolReportConsent(item.Kind, item.Subject, Consent(item.Decision), item.Scope,
            item.Artifact is { } artifact ? new(artifact.Url, artifact.Bytes, artifact.Integrity) : null, item.ScopeSha256))],
        Verification = [.. data.Verification.Select(item => new ToolReportVerification(item.Check, item.Subject, Verification(item.State), item.Cause))],
        Remaining = [.. data.Remaining.Select(item => new ToolReportRemaining(item.Subject, item.Action, item.Cause))],
        Instructions = data.Instructions,
        Effects = [.. data.Effects.Select(item => new ToolReportAppliedChange(item.Kind, item.Subject, item.Action, item.Before, item.After))],
        BackupMode = data.BackupMode is { } backup ? Backup(backup) : null,
    };

    internal static string Mode(ToolWriteMode value) => value switch
    {
        ToolWriteMode.Preview => "preview",
        ToolWriteMode.Apply => "apply",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static string Consent(ToolConsentDecision value) => value switch
    {
        ToolConsentDecision.NotRequired => "not-required",
        ToolConsentDecision.Missing => "missing",
        ToolConsentDecision.Supplied => "supplied",
        ToolConsentDecision.RecordedLocal => "recorded-local",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static string Verification(ToolVerificationState value) => value switch
    {
        ToolVerificationState.NotRun => "not-run",
        ToolVerificationState.Verified => "verified",
        ToolVerificationState.Failed => "failed",
        ToolVerificationState.Unavailable => "unavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };

    internal static string Backup(ToolWriteBackupMode value) => value switch
    {
        ToolWriteBackupMode.Git => "git",
        ToolWriteBackupMode.Sibling => "sibling",
        _ => throw new ArgumentOutOfRangeException(nameof(value)),
    };
}
