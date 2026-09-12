using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusResultPolicy
{
    internal static CliSemanticStatus ReadStatus(IReadOnlyList<StatusFinding> findings)
    {
        var selected = CliSemanticStatus.Complete;
        foreach (var finding in findings)
        {
            _ = CliStatusDefinitions.Read(finding.Status);
            if (Precedence(finding.Status) > Precedence(selected))
            {
                selected = finding.Status;
            }
        }

        return selected;
    }

    internal static CliNextAction? ReadNext(
        CliSemanticStatus status,
        IReadOnlyList<StatusFinding> findings)
    {
        _ = CliStatusDefinitions.Read(status);
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => StatusDefinitions.AttentionNextAction,
            CliSemanticStatus.Incomplete => StatusDefinitions.IncompleteNextAction,
            CliSemanticStatus.Invalid => StatusDefinitions.InvalidNextAction,
            CliSemanticStatus.Blocked => StatusDefinitions.BlockedNextAction,
            CliSemanticStatus.Failed => StatusDefinitions.FailedNextAction,
            CliSemanticStatus.Interrupted => StatusDefinitions.InterruptedNextAction,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Status result is not defined."),
        };
    }

    private static int Precedence(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => 0,
            CliSemanticStatus.Attention => 1,
            CliSemanticStatus.Incomplete => 2,
            CliSemanticStatus.Blocked => 3,
            CliSemanticStatus.Invalid => 4,
            CliSemanticStatus.Failed => 5,
            CliSemanticStatus.Interrupted => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Status result is not defined."),
        };
}
