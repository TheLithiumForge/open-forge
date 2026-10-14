using OpenForge.Cli.Core.Presentation.Doctor.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;

internal static class DoctorTextSelector
{
    internal static CliSelectedReport<DoctorData> Select(CliSelectedReport<DoctorData> selected)
    {
        var next = selected.Report.Next;
        var singleFinding = selected.TextFindings.Count(finding => finding.Severity is CliSeverity.Error or CliSeverity.Warning) == 1;
        return selected with
        {
            TextCounts = [],
            TextLimitations = [],
            ShowNext = false,
            Report = selected.Report with { Data = selected.Report.Data with { NextLine = next?.Command } },
            TextFindings = selected.TextFindings.Select(finding => finding with
            {
                Actions = finding.Actions
                    .Where(action => !(singleFinding && next?.Kind == CliNextActionKind.Sentence && action.Command == next.Command))
                    .Select(action => finding.Message.Contains(action.Reason, StringComparison.Ordinal)
                        ? new CliNextAction(action.Command, action.Command) { Kind = action.Kind } : action)
                    .ToArray(),
            }).ToArray(),
        };
    }
}
