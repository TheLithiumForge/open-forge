using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusToolFindingAppender
{
    internal static StatusResult Append(StatusResult result, IReadOnlyList<ToolObservation> tools, string? failure = null)
    {
        var added = tools.Where(tool => tool.State != ToolObservationState.Current).Select(tool => new StatusFinding
        {
            Code = Code(tool.State),
            Status = StatusDefinitions.ReadFindingStatus(Code(tool.State)),
            Subject = tool.Entry.Identity.Id,
            Cause = $"{tool.Entry.Identity.Name}: {tool.State}. Locked version: {tool.Locked?.Version ?? "missing"}. Detected version: {tool.Detection.Version ?? "unavailable"}.",
        }).ToList();
        if (failure is not null) added.Add(new StatusFinding
        { Code = StatusFindingCode.ToolObservationUnavailable, Status = CliSemanticStatus.Incomplete, Subject = null, Cause = failure });
        if (added.Count == 0) return result;
        StatusFinding[] findings = [.. result.Findings, .. added];
        var status = StatusResultPolicy.ReadStatus(findings);
        return result with { Findings = findings, Status = status, Next = StatusResultPolicy.ReadNext(status, findings) };
    }

    private static StatusFindingCode Code(ToolObservationState state) => state switch
    {
        ToolObservationState.Missing => StatusFindingCode.ToolDeclaredMissing,
        ToolObservationState.Changed => StatusFindingCode.ToolDrift,
        ToolObservationState.Unsupported => StatusFindingCode.ToolUnsupported,
        ToolObservationState.Unavailable => StatusFindingCode.ToolObservationUnavailable,
        _ => throw new ArgumentOutOfRangeException(nameof(state)),
    };
}
