using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Rendering;

internal static class CliTextRenderer
{
    internal static CliTextDocument Render<TData>(
        CliSelectedReport<TData> selected, CliTextStyle style, CliDataTextRenderer<TData> dataRenderer) where TData : class
    {
        ArgumentNullException.ThrowIfNull(selected);
        ArgumentNullException.ThrowIfNull(style);
        ArgumentNullException.ThrowIfNull(dataRenderer);
        var report = selected.Report;
        var selection = selected.Selection;
        var builder = new StringBuilder();
        if (selected.ShowHeadline)
        {
            builder.Append(style.Status(CliText.Escape(report.Headline.Sentence), report.Status)).Append('\n');
        }

        if (selected.ShowWorkspace && report.Workspace is { } workspace)
        {
            builder.Append($"Workspace: {CliText.Escape(workspace.Path)}\n");
        }

        string? category = null;
        foreach (var finding in selected.TextFindings)
        {
            if (selection.Detail >= CliDetail.Standard && finding.Category is { } heading && category != heading)
            {
                builder.Append(CliText.Escape(heading)).Append('\n');
                category = heading;
            }

            AppendFinding(builder, finding, selection, style);
        }

        foreach (var effect in selected.TextEffects)
        {
            var action = effect.Outcome switch
            {
                CliEffectOutcome.NotStarted => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelNotStarted(),
                CliEffectOutcome.Unknown => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFinalStateUnknown(),
                CliEffectOutcome.Failed => global::OpenForge.Cli.OutputText.Shared.SharedText.LabelFailed(),
                CliEffectOutcome.Planned or CliEffectOutcome.Done => CliReportVocabulary.Name(effect.Action),
                _ => throw new ArgumentOutOfRangeException(nameof(selected)),
            };
            var reason = effect.Reason is { } value ? $" ({CliText.Escape(value)})" : string.Empty;
            builder.Append($"  {style.Subject(CliText.Escape(effect.Path))}  {action}{reason}\n");
            if (selection.Detail >= CliDetail.Full)
            {
                if (effect.Before is { } before)
                {
                    builder.Append($"    {CliText.Escape(CliFindingWording.BeforeHash(before))}\n");
                }

                if (effect.After is { } after)
                {
                    builder.Append($"    {CliText.Escape(CliFindingWording.AfterHash(after))}\n");
                }
            }
        }

        var spans = new List<CliTextSpan> { new(builder.ToString()) };
        spans.AddRange(dataRenderer(report.Data, selection, style).Spans);
        builder.Clear();
        foreach (var limitation in selected.TextLimitations)
        {
            builder.Append($"  {CliText.Escape(limitation.What)}: {CliText.Escape(limitation.Why)}\n");
        }

        var counts = selected.TextCounts.Where(count => count.Value is > 0)
            .Select(count => string.Create(CultureInfo.InvariantCulture, $"{count.Value} {CliText.Escape(count.Label)}")).ToArray();
        if (counts.Length > 0)
        {
            builder.Append(style.Dim(string.Join(", ", counts) + ".")).Append('\n');
        }

        if (selected.ShowNext && report.Next is { } next)
        {
            if (selection.Detail >= CliDetail.Standard)
            {
                builder.Append($"  {style.Dim(CliText.Escape(next.Reason))}\n");
            }
            builder.Append($"Next: {CliText.Escape(next.Command)}\n");
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }

    private static void AppendFinding(StringBuilder builder, CliFinding finding, CliSelection selection, CliTextStyle style)
    {
        var subject = finding.Subject.Path ?? finding.Subject.Id
            ?? throw new InvalidOperationException("A listed finding requires a path or identifier.");
        if (finding.Subject.Location is { } location)
        {
            subject = string.Create(CultureInfo.InvariantCulture, $"{subject}:{location.Line}:{location.Column}");
        }

        var code = selection.Detail >= CliDetail.Full ? $" [{CliText.Escape(finding.Code)}]" : string.Empty;
        builder.Append($"  {style.Severity(finding.Severity.ToString(), finding.Severity)}  {style.Subject(CliText.Escape(subject))}  {CliText.Escape(finding.Title)}{code}\n");
        builder.Append($"         {CliText.Escape(finding.Message)}\n");
        if (selection.Detail >= CliDetail.Full && finding.Resolution is { } resolution)
        {
            var command = finding.Actions.FirstOrDefault(action => action.Kind == CliNextActionKind.Command)?.Command;
            if (CliFindingWording.Resolution(resolution, command) is { } phrase)
            {
                builder.Append($"         {CliText.Escape(phrase)}\n");
            }
        }

        foreach (var action in finding.Actions)
        {
            builder.Append($"         {CliText.Escape(action.Command)}\n");
            if (selection.Detail >= CliDetail.Standard)
            {
                builder.Append($"         {style.Dim(CliText.Escape(action.Reason))}\n");
            }
        }

        foreach (var candidate in finding.Candidates)
        {
            builder.Append($"         {CliText.Escape(candidate.Subject.Path ?? candidate.Subject.Id ?? string.Empty)}: {CliText.Escape(string.Join("; ", candidate.Reasons))}\n");
        }

        foreach (var evidence in finding.Evidence)
        {
            builder.Append($"         {CliText.Escape(evidence.Label)}: {CliText.Escape(evidence.Value)}\n");
        }

        if (finding.Provenance is { } provenance)
        {
            builder.Append($"         read from {CliText.Escape(provenance.Path ?? provenance.Source)}\n");
        }
    }
}
