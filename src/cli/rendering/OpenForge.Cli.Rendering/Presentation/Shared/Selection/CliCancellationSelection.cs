using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Shared.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Selection;

internal static class CliCancellationSelection
{
    internal static CliSelectedReport<TData> BeforeEffects<TData>(CliSelectedReport<TData> selected, string interruptionCode) where TData : class
    {
        if (selected.Report.Status != CliSemanticStatus.Interrupted
            || selected.Report.Effects.Any(effect => effect.Outcome is CliEffectOutcome.Done or CliEffectOutcome.Unknown or CliEffectOutcome.Failed))
            return selected;
        var interruptions = selected.Report.Findings.Count(finding => finding.Code == interruptionCode && finding.Severity == CliSeverity.Error);
        if (interruptions == 0) return selected;
        return selected with
        {
            TextFindings = selected.TextFindings.Where(finding => finding.Code != interruptionCode).ToArray(),
            TextDiagnostics = [],
            TextCounts = selected.TextCounts.Select(count => count.Name == "errors" && count.Value is { } value
                ? count with { Value = Math.Max(0, value - interruptions) } : count).ToArray(),
            TextLimitations = selected.Report.Findings.All(finding => finding.Code == interruptionCode)
                ? [] : selected.TextLimitations,
        };
    }
}
