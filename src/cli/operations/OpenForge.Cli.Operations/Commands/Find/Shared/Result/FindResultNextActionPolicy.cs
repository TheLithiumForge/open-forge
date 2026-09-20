using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal static class FindResultNextActionPolicy
{
    internal static CliNextAction? Read(FindResultNextActionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var status = input.Status;
        return status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => FindDefinitions.IncompleteNextAction,
            CliSemanticStatus.Invalid => FindDefinitions.InvalidNextAction,
            CliSemanticStatus.Blocked when input.Findings
                .Where(finding => finding.Status == CliSemanticStatus.Blocked)
                .All(finding => finding.Code == FindFindingCode.SelectorAmbiguous)
                => FindDefinitions.SelectorAmbiguousNextAction,
            CliSemanticStatus.Blocked => FindDefinitions.BlockedNextAction,
            CliSemanticStatus.Failed => FindDefinitions.FailedNextAction,
            CliSemanticStatus.Interrupted => FindDefinitions.InterruptedNextAction,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Find status is not defined."),
        };
    }
}
