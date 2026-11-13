using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Presentation.Tool.List.Models;
using OpenForge.Cli.Core.Presentation.Tool.List.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Tool.Shared;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool.List;

internal static class ToolListPresentation
{
    internal static CliReportRendering<ToolListResult, ToolListData> Rendering { get; } = new()
    {
        Shape = CliCommandShape.Data,
        Selector = (result, selection) => new CliReport<ToolListData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = new(ToolText.ListHeadline(), ToolReadPresentation.Headline(result.Status)),
            Workspace = result.WorkspacePath is { } path ? new CliWorkspaceEcho(path, result.WorkspaceExplicit) : null,
            Findings = ToolReadPresentation.Findings(result.Command, result.Issues),
            Next = ToolReadPresentation.Next(result.NextCommand, result.Issues),
            Data = new(result.Available, result.Installed, result.Catalogue, result.Coverage, result.RoleFilter),
        },
        DataJsonTypeInfo = ToolListJsonContext.Default.ToolListData,
        DataTextRenderer = Render,
    };

    private static CliTextDocument Render(ToolListData data, CliSelection selection, CliTextStyle style)
    {
        var lines = new List<string>();
        lines.Add(ToolText.Available());
        if (data.Available.Length == 0) lines.Add("  " + ToolText.Empty());
        else
        {
            IReadOnlyList<string>[] rows = [ToolText.AvailableColumns(), .. data.Available.Select(row => new string[]
            {
                row.Id, row.Version, row.Origin, row.TargetSupport,
                $"{row.Admission}, {ToolText.Reviewed(row.Reviewed)}, {string.Join(", ", row.Capabilities)}",
            })];
            lines.Add(string.Concat(CliTable.RenderRows(rows, width: style.Width, stackOverflow: false)).TrimEnd('\n'));
        }
        lines.Add(ToolText.Installed());
        if (data.Installed.Length > 0)
        {
            IReadOnlyList<string>[] rows = [ToolText.InstalledColumns(), .. data.Installed.Select(row => new string[]
            {
                row.Id, row.Version, row.Origin ?? ToolText.UnknownOrigin(), row.Verification.State.ToString(),
            })];
            lines.Add(string.Concat(CliTable.RenderRows(rows, width: style.Width, stackOverflow: false)).TrimEnd('\n'));
        }
        if (selection.Detail >= CliDetail.Full)
            lines.AddRange(data.Installed.Select(row => $"  {CliText.Escape(row.Id)} {ToolText.Roles()}: {string.Join(", ", row.Roles)}"));
        if (data.Installed.Length == 0) lines.Add("  " + ToolText.Empty());
        lines.Add(ToolText.Coverage());
        lines.AddRange(data.Coverage.Select(row => $"  {row.Store}: {row.State}"));
        return new([new CliTextSpan(string.Join("\n", lines) + "\n")]);
    }
}
