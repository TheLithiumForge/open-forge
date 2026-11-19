using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Tool;

namespace OpenForge.Cli.Core.Presentation.Tool;

internal static class ToolListPresentation
{
    internal static readonly ToolJsonContext Json = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    internal static CliReportRendering<ToolListResult, ToolListData> Rendering { get; } = new()
    {
        Selector = Select,
        DataTextRenderer = Render,
        DataJsonTypeInfo = Json.ToolListData,
        Shape = CliCommandShape.Data,
    };

    private static CliReport<ToolListData> Select(ToolListResult result, CliSelection selection) => new()
    {
        ShowMinimalTextWarnings = true,
        Command = result.Command,
        Status = result.Status,
        Headline = new(ToolText.ListHeadline(result.Data.Tools.Count), result.Status switch
        {
            CliSemanticStatus.Complete => CliHeadlineKind.Done,
            CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
            CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
            CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
            _ => CliHeadlineKind.CannotStart,
        }),
        Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
        Data = result.Data,
        Findings = [.. result.Findings.Select(finding => ToolFindings.Create(ToolCommandKind.List, result.Command, finding,
            id => result.Data.Tools.FirstOrDefault(tool => tool.Id == id)?.Name))],
    };

    private static CliTextDocument Render(ToolListData data, CliSelection selection, CliTextStyle style)
    {
        var builder = new StringBuilder();
        var idWidth = data.Tools.Select(tool => tool.Id.Length).DefaultIfEmpty().Max();
        var methodWidth = data.Tools.Select(tool => tool.Method.Length).DefaultIfEmpty().Max();
        var stateWidth = data.Tools.Select(tool => State(tool).Length).DefaultIfEmpty().Max();
        foreach (var category in data.Tools.GroupBy(tool => tool.Category))
        {
            if (builder.Length != 0) builder.Append('\n');
            builder.Append(CliText.Escape(category.Key)).Append('\n');
            foreach (var tool in category)
            {
                // Pad the plain text first so color codes never shift the columns.
                var method = Tag(tool.Method.PadRight(methodWidth), tool.Method, style);
                builder.Append(CliText.Escape($"  {tool.Id.PadRight(idWidth)}  ")).Append(method)
                    .Append(CliText.Escape($"  {State(tool).PadRight(stateWidth)}  {tool.Purpose}")).Append('\n');
                if (selection.Detail >= CliDetail.Standard)
                    builder.Append(CliText.Escape($"  {new string(' ', idWidth)}  {ToolText.Details(tool.Name, tool.Licence, tool.Upstream)}")).Append('\n');
            }
        }
        return new([new CliTextSpan(builder.ToString())]);
    }

    private static string State(ToolListEntry tool) => ToolText.State(tool.Configured, tool.Installed, tool.Version);

    // npm is a familiar package manager. A script runs the tool's own installer, so it stands out.
    private static string Tag(string text, string method, CliTextStyle style)
        => method == ToolWire.NpmMethod ? style.Information(text) : style.Warning(text);
}
