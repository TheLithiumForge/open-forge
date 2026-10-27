using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.List.Models;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Rendering;

internal static class RouteListDataTextRenderer
{
    internal static CliTextDocument Render(RouteListData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        if (data.Rows.Count == 0)
        {
            return new CliTextDocument([]);
        }

        var tableRows = new List<IReadOnlyList<string>>(data.Rows.Count);
        foreach (var row in data.Rows)
        {
            var indentation = new string(' ', checked(row.RelativeDepth * 2));
            tableRows.Add([indentation + row.Id, row.Description]);
        }

        var builder = new StringBuilder(data.SeparateRows ? "\n" : string.Empty);
        var renderedRows = CliTable.RenderRows(tableRows,
            (column, cell) => column == 0 && cell.Length > 0 ? style.Subject(cell) : cell, style.Width, stackOverflow: false);
        for (var index = 0; index < data.Rows.Count; index++)
        {
            builder.Append(renderedRows[index]);
            var row = data.Rows[index];
            if (data.ShowPaths)
            {
                var indent = new string(' ', checked(row.RelativeDepth * 2 + 4));
                builder.AppendLine($"{indent}{CliText.Escape(row.Path)}");
                if (row.MaintenanceRoot is { } maintenanceRoot)
                {
                    Line(builder, indent, global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelMaintenanceRoot(), maintenanceRoot);
                }
                if (row.Tags.Count > 0)
                {
                    foreach (var line in CliSelectionText.Wrap(string.Join(" ", row.Tags.Select(tag => $"#{tag}")), style.Width - indent.Length))
                        builder.Append(indent).AppendLine(line);
                }
            }
        }
        if (data.ShowDetails)
        {
            foreach (var row in data.Rows)
            {
                AppendDetails(builder, row);
            }
        }

        if (data.ShowTrailer && data.Trailer is { } trailer)
        {
            builder.Append(CliText.Escape(trailer)).Append('\n');
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }

    private static void AppendDetails(StringBuilder builder, RouteListDataRow row)
    {
        var result = row.Result;
        var indentation = new string(' ', checked(row.RelativeDepth * 2));
        var parent = result.ParentId is null
            ? "none"
            : $"{result.ParentId}; {result.ParentPath}";
        Line(builder, indentation, global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleParent(), parent);
        Line(
            builder,
            indentation,
            global::OpenForge.Cli.OutputText.Route.List.RouteListText.HelpHeadingDepth(),
            string.Create(
                CultureInfo.InvariantCulture,
                $"absolute {result.AbsoluteDepth?.ToString(CultureInfo.InvariantCulture) ?? "none"}; relative {result.RelativeDepth}"));
        Line(
            builder,
            indentation,
            global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleKind(),
            string.Create(
                CultureInfo.InvariantCulture,
                $"{RouteListWording.Kind(result.Kind)}; direct children: {result.DirectChildCount?.ToString(CultureInfo.InvariantCulture) ?? "none"}"));
        Line(builder, indentation, global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleSelectedAs(), RouteListWording.SelectedAs(result.Selection));
        Line(builder, indentation, global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleOverwrite(), result.HasOverwrite ? "present" : "none");
    }

    private static void Line(StringBuilder builder, string indentation, string label, string value)
        => builder.Append(indentation)
            .Append("  ")
            .Append(label)
            .Append(": ")
            .Append(CliText.Escape(value))
            .Append('\n');
}
