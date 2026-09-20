using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Selection;

internal static class CliReportTrimmer
{
    internal static bool Lists(CliSeverity severity, CliSelection selection, CliCommandShape shape)
    {
        ArgumentNullException.ThrowIfNull(selection);
        if (!Enum.IsDefined(severity) || !Enum.IsDefined(selection.Detail) || !Enum.IsDefined(shape))
        {
            throw new ArgumentOutOfRangeException(nameof(selection), "The selection contains an undefined value.");
        }

        if (selection.Filter is { } filter)
        {
            if (filter.Any(value => !Enum.IsDefined(value)))
            {
                throw new ArgumentOutOfRangeException(nameof(selection), "The severity filter contains an undefined value.");
            }

            return filter.Contains(severity);
        }

        return severity switch
        {
            CliSeverity.Error => true,
            CliSeverity.Warning => shape != CliCommandShape.Diagnosis || selection.Detail >= CliDetail.Standard,
            CliSeverity.Info => selection.Detail >= CliDetail.Full,
            _ => throw new ArgumentOutOfRangeException(nameof(severity)),
        };
    }

    internal static CliSelectedReport<TData> Trim<TData>(
        CliReport<TData> report, CliSelection selection, CliCommandShape shape) where TData : class
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(selection);
        _ = Lists(CliSeverity.Error, selection, shape);
        if (report.Findings.Any(finding => finding.Resolution == CliResolution.TargetedOperation
            && !finding.Actions.Any(action => action.Kind == CliNextActionKind.Command)))
        {
            throw new InvalidOperationException("A targeted-operation finding requires a command action.");
        }

        var counts = report.Counts.GroupBy(count => count.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First() with
            {
                Value = group.Any(count => count.Value is null) ? null : group.Max(count => count.Value),
                UnavailableReason = group.FirstOrDefault(count => count.UnavailableReason is not null)?.UnavailableReason,
            }, StringComparer.Ordinal);
        foreach (var severity in Enum.GetValues<CliSeverity>())
        {
            var name = severity switch
            {
                CliSeverity.Error => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelErrors(),
                CliSeverity.Warning => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelWarnings(),
                CliSeverity.Info => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelInfos(),
                _ => throw new ArgumentOutOfRangeException(nameof(severity)),
            };
            var total = report.Findings.LongCount(finding => finding.Severity == severity);
            if (!counts.ContainsKey(name) && total > 0)
            {
                counts.Add(name, new CliCount(name, name, total));
            }
        }

        CliFinding[] SelectFindings(bool text) => report.Findings
            .Where(finding => Lists(finding.Severity, selection, shape)
                || (text && report.ShowMinimalTextWarnings && selection.Filter is null && finding.Severity == CliSeverity.Warning))
            .OrderBy(finding => finding.Severity)
            .ThenBy(finding => finding.Subject.Path ?? finding.Subject.Id, StringComparer.Ordinal)
            .ThenBy(finding => finding.Subject.Location?.Line)
            .ThenBy(finding => finding.Subject.Location?.Column)
            .Select(finding => finding with
            {
                Candidates = selection.Detail >= CliDetail.Full ? finding.Candidates : [],
                Evidence = selection.Detail >= CliDetail.Full ? finding.Evidence : [],
                Provenance = selection.Detail >= CliDetail.Full ? finding.Provenance : null,
                Resolution = selection.Detail >= CliDetail.Standard ? finding.Resolution : null,
                Actions = selection.Detail >= CliDetail.Standard ? finding.Actions : finding.Actions.Take(1).ToArray(),
            }).ToArray();
        var findings = SelectFindings(text: false);
        var textFindings = SelectFindings(text: true);
        var selected = report with
        {
            Findings = findings,
            Effects = report.Effects.Select(effect => selection.Detail >= CliDetail.Full
                ? effect : effect with { Before = null, After = null }).ToArray(),
            Counts = counts.Values.ToArray(),
            Limitations = report.Limitations.Concat(counts.Values
                .Where(count => count.Value is null && count.UnavailableReason is not null)
                .Select(count => new CliLimitation(count.Label, count.UnavailableReason!)))
                .Distinct().ToArray(),
            Diagnostics = selection.Detail == CliDetail.Debug ? report.Diagnostics : [],
        };
        var showWorkspace = report.Workspace is { } workspace
            && (workspace.Explicit || selection.Detail >= CliDetail.Standard
                || report.Status is CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted);
        return new CliSelectedReport<TData>(selected, selection, showWorkspace)
        {
            TextFindings = selection.Detail == CliDetail.Minimal
                && report.HeadlineFindingCode is { } headlineCode
                && textFindings.Count(finding => finding.Code == headlineCode) == 1
                    ? textFindings.Where(finding => finding.Code != headlineCode).ToArray() : textFindings,
        };
    }
}
