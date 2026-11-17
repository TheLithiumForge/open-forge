using System.Text;
using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool;

/// <summary>Install and Remove share one report: the commands, Open Forge's changes and, for Remove, the tool's own uninstall.</summary>
internal static class ToolWritePresentation
{
    private const string Indent = "  ";
    private const string DetailIndent = "      ";

    internal static CliReportRendering<ToolWriteResult, ToolWriteData> Rendering { get; } = new()
    {
        Selector = Select,
        DataTextRenderer = Render,
        DataJsonTypeInfo = ToolListPresentation.Json.ToolWriteData,
        Shape = CliCommandShape.ChangeReport,
    };

    private static CliReport<ToolWriteData> Select(ToolWriteResult result, CliSelection selection) => new()
    {
        Command = result.Command,
        Status = result.Status,
        Headline = Headline(result),
        Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
        Data = result.Data,
        Findings = [.. result.Findings.Select(finding => ToolFindings.Create(result.Kind, result.Command, finding,
            id => result.Data.Tools.FirstOrDefault(tool => tool.Id == id)?.Name))],
    };

    private static CliHeadline Headline(ToolWriteResult result) => result.Status switch
    {
        CliSemanticStatus.Complete when result.Kind == ToolCommandKind.Install && result.Data.Tools.Count == 0
            => new(ToolText.NoTools(), CliHeadlineKind.NothingToDo),
        CliSemanticStatus.Complete when !HasWork(result.Data) => new(ToolText.NothingToDo(result.Command), CliHeadlineKind.NothingToDo),
        CliSemanticStatus.Complete when result.Data.Preview => new(ToolText.Previewed(result.Command), CliHeadlineKind.Preview),
        // A plan shown before consent has run nothing yet.
        CliSemanticStatus.Complete when result.Kind == ToolCommandKind.Install && result.Data.Tools.All(Planned) => new(ToolText.ReviewPlan(result.Command), CliHeadlineKind.Preview),
        CliSemanticStatus.Complete or CliSemanticStatus.Attention => new(ToolText.Finished(result.Command), CliHeadlineKind.Done),
        CliSemanticStatus.Blocked => new(ToolText.Blocked(result.Command), CliHeadlineKind.Blocked),
        CliSemanticStatus.Failed or CliSemanticStatus.Incomplete => new(ToolText.StoppedAtFailure(result.Command), CliHeadlineKind.Failed),
        CliSemanticStatus.Interrupted => new(ToolText.Cancelled(result.Command), CliHeadlineKind.Cancelled),
        _ => new(ToolText.CannotStart(result.Command), CliHeadlineKind.CannotStart),
    };

    private static bool HasWork(ToolWriteData data)
        => data.Tools.Any(tool => tool.Changes.Count != 0 || tool.Steps.Any(step => step.Outcome is not (ToolWire.Skipped or ToolWire.NotRun)));

    // Nothing has run when a plan is previewed, waits for consent or stops before its first effect.
    private static bool Planned(ToolWriteTool tool)
        => tool.Steps.All(step => step.Outcome is ToolWire.Planned or ToolWire.Skipped or ToolWire.NotRun)
            && tool.Changes.All(change => change.Outcome == ToolWire.Planned);

    private static CliTextDocument Render(ToolWriteData data, CliSelection selection, CliTextStyle style)
    {
        var builder = new StringBuilder();
        foreach (var tool in data.Tools)
        {
            var removal = tool.UninstallDescription is not null;
            var preview = data.Preview || Planned(tool);
            if (removal) Changes(builder, tool, preview, removal);
            else
            {
                if (tool.InstalledVersion is { } version) Line(builder, ToolText.AlreadyInstalled(tool.Name, version));
                var run = tool.Steps.Where(step => step.Outcome != ToolWire.Skipped).ToArray();
                if (run.Length != 0)
                {
                    Line(builder, ToolText.InstallHeading(tool.Name, preview, tool.InstalledVersion is not null));
                    foreach (var step in run) Step(builder, step, preview);
                }
                Changes(builder, tool, preview, removal);
            }
            if (removal && tool.Steps.Count != 0)
            {
                Line(builder, ToolText.UninstallHeading(tool.Name));
                Line(builder, Indent + tool.UninstallDescription);
                if (!preview && tool.Steps.All(step => step.Outcome == ToolWire.NotRun)) Line(builder, ToolText.UninstallYourself());
                foreach (var step in tool.Steps) Step(builder, step, preview || step.Outcome == ToolWire.NotRun);
            }
        }
        return new([new CliTextSpan(builder.ToString())]);
    }

    private static void Changes(StringBuilder builder, ToolWriteTool tool, bool preview, bool removal)
    {
        if (tool.Changes.Count == 0) return;
        Line(builder, !preview && tool.Changes.All(change => change.Outcome == ToolWire.NotStarted)
            ? ToolText.ChangesNotMade()
            : ToolText.ChangesHeading(preview, removal));
        var width = tool.Changes.Max(change => change.Path.Length) + 2;
        foreach (var change in tool.Changes)
        {
            var purpose = change.Kind switch
            {
                ToolWire.AddTool => ToolText.AddTool(tool.Id),
                ToolWire.RemoveTool => ToolText.RemoveTool(tool.Id),
                _ => change.Purpose ?? string.Empty,
            };
            Line(builder, $"{Indent}{change.Path.PadRight(width)}{purpose}{Outcome(change.Outcome, preview)}");
        }
    }

    private static void Step(StringBuilder builder, ToolWriteStep step, bool preview)
    {
        Line(builder, $"{Indent}{step.Command}{Outcome(step.Outcome, preview)}");
        if (step.Effect is { } effect) Line(builder, DetailIndent + effect);
        foreach (var note in step.Notes) Line(builder, DetailIndent + note);
        if (step.Outcome != ToolWire.Failed) return;
        Line(builder, DetailIndent + ToolText.Exit(step.ExitCode));
        foreach (var line in (step.Output ?? string.Empty).Split('\n')) Line(builder, DetailIndent + Indent + line);
    }

    private static string Outcome(string outcome, bool preview) => preview ? string.Empty : outcome switch
    {
        ToolWire.Done => $"  ({ToolText.Done()})",
        ToolWire.Failed => $"  ({ToolText.Failed()})",
        ToolWire.Skipped => $"  ({ToolText.Skipped()})",
        ToolWire.NotRun => $"  ({ToolText.NotRun()})",
        ToolWire.NotStarted => $"  ({ToolText.NotStarted()})",
        _ => string.Empty,
    };

    private static void Line(StringBuilder builder, string text) => builder.Append(CliText.Escape(text)).Append('\n');
}
