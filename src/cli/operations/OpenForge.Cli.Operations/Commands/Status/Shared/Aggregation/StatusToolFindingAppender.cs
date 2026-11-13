using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusToolFindingAppender
{
    internal static StatusResult Append(StatusResult result, IReadOnlyList<ToolRunFailure> missing)
    {
        if (missing.Count == 0) return result;
        StatusFinding[] findings = [.. result.Findings, .. missing.Select(failure => new StatusFinding
        {
            Code = StatusFindingCode.ToolDeclaredMissing,
            Status = CliSemanticStatus.Attention,
            Subject = failure.Id,
            Cause = failure.Cause,
        })];
        var status = StatusResultPolicy.ReadStatus(findings);
        return result with
        {
            Findings = findings,
            Status = status,
            Next = status == CliSemanticStatus.Attention ? new CliNextAction(ToolContractCodes.RestoreCommand, missing[0].Cause) : result.Next,
        };
    }
}
