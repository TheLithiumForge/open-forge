using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal static class FindResultStatusPolicy
{
    internal static CliSemanticStatus Read(FindResultStatusInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.TerminalEvent is { Kind: FindTerminalEventKind.Failed })
        {
            return CliSemanticStatus.Failed;
        }

        if (input.TerminalEvent is { Kind: FindTerminalEventKind.Interrupted })
        {
            return CliSemanticStatus.Interrupted;
        }

        if (input.Findings.Any(finding => finding.Status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Blocked
            || input.StageCompletion.Projection == FindProjectionCoverageState.Blocked
            || input.Findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Failed
            || input.StageCompletion.Projection == FindProjectionCoverageState.Failed
            || input.Findings.Any(finding => finding.Status == CliSemanticStatus.Failed))
        {
            return CliSemanticStatus.Failed;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Interrupted
            || input.StageCompletion.Projection == FindProjectionCoverageState.Interrupted
            || input.Findings.Any(finding => finding.Status == CliSemanticStatus.Interrupted))
        {
            return CliSemanticStatus.Interrupted;
        }

        if (input.StageCompletion.Matching == FindCoverageState.NotStarted)
        {
            return CliSemanticStatus.Invalid;
        }

        if (input.StageCompletion.Matching == FindCoverageState.Incomplete
            || input.StageCompletion.Projection == FindProjectionCoverageState.Incomplete
            || input.Findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return input.Findings.Any(finding => finding.Status == CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }
}
