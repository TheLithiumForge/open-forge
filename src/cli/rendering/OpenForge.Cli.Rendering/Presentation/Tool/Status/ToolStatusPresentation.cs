using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Presentation.Tool.Status.Models;
using OpenForge.Cli.Core.Presentation.Tool.Status.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Tool.Shared;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool.Status;

internal static class ToolStatusPresentation
{
    internal static CliReportRendering<ToolStatusResult, ToolStatusData> Rendering { get; } = new()
    {
        Shape = CliCommandShape.Data,
        Selector = (result, selection) => new CliReport<ToolStatusData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = new(ToolText.StatusHeadline(), ToolReadPresentation.Headline(result.Status)),
            Workspace = result.WorkspacePath is { } path ? new CliWorkspaceEcho(path, result.WorkspaceExplicit) : null,
            Findings = ToolReadPresentation.Findings(result.Command, result.Issues),
            Next = ToolReadPresentation.Next(result.NextCommand, result.Issues),
            Data = new(result.Tools, result.Coverage),
        },
        DataJsonTypeInfo = ToolStatusJsonContext.Default.ToolStatusData,
        DataTextRenderer = Render,
    };

    private static CliTextDocument Render(ToolStatusData data, CliSelection selection, CliTextStyle style)
    {
        var lines = new List<string>();
        lines.AddRange(data.Tools.Select(row =>
            $"{CliText.Escape(row.Id)} {CliText.Escape(row.Version)}  {row.Installation.State}  {row.Configuration.State}  {row.Index.State}  {row.Guidance.State}"));
        if (data.Tools.Length == 0) lines.Add(ToolText.Empty());
        if (selection.Detail >= CliDetail.Full)
            foreach (var row in data.Tools)
            {
                lines.Add($"  {CliText.Escape(row.Id)} {ToolText.Roles()}: {string.Join(", ", row.Roles)}");
                if (row.ProcessPolicy is { } policy)
                {
                    lines.Add(ToolText.Policy());
                    lines.AddRange(policy.GetRawText().Split('\n').Select(line => CliText.Escape(line.TrimEnd('\r'))));
                }
                if (row.Limits.Length != 0)
                {
                    lines.Add(ToolText.Limits());
                    lines.AddRange(row.Limits.Select(CliText.Escape));
                }
            }
        lines.Add(ToolText.Coverage());
        lines.AddRange(data.Coverage.Select(row => $"  {row.Store}: {row.State}"));
        return new([new CliTextSpan(string.Join("\n", lines) + "\n")]);
    }
}
