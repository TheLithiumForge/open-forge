using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Tool.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Tool.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Tool.Shared;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool.Inspect;

internal static class ToolInspectPresentation
{
    internal static CliReportRendering<ToolInspectResult, ToolInspectData> Rendering { get; } = new()
    {
        Shape = CliCommandShape.Data,
        Selector = (result, selection) => new CliReport<ToolInspectData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = new(ToolText.InspectHeadline(), ToolReadPresentation.Headline(result.Status)),
            Workspace = result.WorkspacePath is { } path ? new CliWorkspaceEcho(path, result.WorkspaceExplicit) : null,
            Findings = ToolReadPresentation.Findings(result.Command, result.Issues),
            Next = ToolReadPresentation.Next(result.NextCommand, result.Issues),
            Data = new(result.Id, result.Version, result.Definition, result.Target, result.Installed, result.Telemetry, result.Requirements, result.Limitations, result.Instructions),
        },
        DataJsonTypeInfo = ToolInspectJsonContext.Default.ToolInspectData,
        DataTextRenderer = Render,
    };

    private static CliTextDocument Render(ToolInspectData data, CliSelection selection, CliTextStyle style)
    {
        var lines = new List<string>();
        if (data.Definition is not { } definition) lines.Add(ToolText.DefinitionUnavailable());
        else
        {
            lines.Add($"{CliText.Escape(data.Id ?? string.Empty)} {CliText.Escape(data.Version ?? string.Empty)}  {data.Target}");
            if (selection.Detail >= CliDetail.Full)
                lines.AddRange(definition.GetRawText().Split('\n').Select(line => CliText.Escape(line.TrimEnd('\r'))));
        }
        lines.Add(ToolText.TelemetryChoice(data.Telemetry));
        lines.AddRange(data.Requirements.Select(value => "  " + CliText.Escape(value)));
        if (data.Instructions.Cause is { } cause)
            lines.AddRange(CliSelectionText.Wrap(ToolText.InstructionsUnavailable(cause), style.Width));
        return new([new CliTextSpan(string.Join("\n", lines) + "\n")]);
    }
}
