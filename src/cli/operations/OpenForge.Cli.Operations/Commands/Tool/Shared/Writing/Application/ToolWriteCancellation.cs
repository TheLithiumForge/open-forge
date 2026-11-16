using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal static class ToolWriteCancellation
{
    internal static bool Stop(ToolWriteExecution state, CancellationToken token, bool cancelled = false)
    {
        if (!cancelled && !token.IsCancellationRequested) return false;
        if (!state.Findings.Any(finding => finding.Code == ToolWriteFindingCode.Interrupted))
            state.Findings.Add(new(ToolWriteFindingCode.Interrupted, CliSemanticStatus.Interrupted));
        if (state.Outcomes.LastOrDefault() is { Completed.Count: > 0 } last && last.Stage != ToolWriteStages.Records)
            state.Outcomes.Add(new(last.ToolId, ToolWriteStages.Records, ToolWriteStages.NotStarted, [], last.Completed));
        state.Stopped = true;
        return true;
    }
}
