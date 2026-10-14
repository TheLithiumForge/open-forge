using OpenForge.Cli.Core.Shell.Presentation.Models;
using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;

namespace OpenForge.Cli.Core.Presentation.Shared.Text;

internal static class CliTable
{
    private const int Indent = 2;
    private const int ColumnGap = 2;
    private const int MinimumProseWidth = 20;

    internal static string Render(IReadOnlyList<IReadOnlyList<string>> rows,
        Func<int, string, string>? styleCell = null, int width = CliPresentation.DefaultWidth)
        => string.Concat(RenderRows(rows, styleCell, width));

    internal static string Render(CliTextStyle style, IReadOnlyList<IReadOnlyList<string>> rows,
        Func<int, string, string>? styleCell = null)
        => Render(rows, styleCell, style.Width);

    internal static IReadOnlyList<string> RenderRows(IReadOnlyList<IReadOnlyList<string>> rows,
        Func<int, string, string>? styleCell = null, int width = CliPresentation.DefaultWidth, bool wrapLastColumn = true,
        bool stackOverflow = true, IReadOnlySet<int>? unwrappedColumns = null)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0) return [];
        var columns = rows.Max(row => row.Count);
        var widths = Enumerable.Range(0, columns)
            .Select(column => rows.Max(row => column < row.Count ? CliSelectionText.Width(row[column]) : 0)).ToArray();
        var rendered = new List<string>(rows.Count);
        foreach (var row in rows)
        {
            if (row.Count == 0)
            {
                rendered.Add("\n");
                continue;
            }
            var builder = new StringBuilder();
            var prefixWidth = Indent + widths.Take(row.Count - 1).Sum() + ColumnGap * (row.Count - 1);
            var last = row.Count - 1;
            var canAlign = prefixWidth + CliSelectionText.Width(row[last]) <= width;
            if (wrapLastColumn && !stackOverflow)
                canAlign = prefixWidth + Math.Min(MinimumProseWidth, CliSelectionText.Width(row[last])) <= width;
            if (canAlign)
            {
                builder.Append(' ', Indent);
                for (var column = 0; column < last; column++)
                {
                    AppendCell(builder, column, row[column], styleCell);
                    builder.Append(' ', widths[column] - CliSelectionText.Width(row[column]) + ColumnGap);
                }
                IReadOnlyList<string> lines = wrapLastColumn ? CliSelectionText.Wrap(row[last], width - prefixWidth) : [CliText.Escape(row[last])];
                for (var index = 0; index < lines.Count; index++)
                {
                    if (index > 0) builder.Append(' ', prefixWidth);
                    builder.Append(styleCell is null ? lines[index] : styleCell(last, lines[index])).Append('\n');
                }
            }
            else
            {
                for (var column = 0; column < row.Count; column++)
                {
                    if (row[column].Length == 0) continue;
                    var leadingIndent = row[0].TakeWhile(character => character == ' ').Count();
                    var indentation = column == 0 ? Indent : Indent + ColumnGap + leadingIndent;
                    IReadOnlyList<string> lines = wrapLastColumn && (column > 0 || row.Count == 1) && unwrappedColumns?.Contains(column) != true
                        ? CliSelectionText.Wrap(row[column], width - indentation)
                        : [CliText.Escape(row[column])];
                    foreach (var line in lines)
                    {
                        builder.Append(' ', indentation);
                        builder.Append(styleCell is null ? line : styleCell(column, line)).Append('\n');
                    }
                }
            }
            rendered.Add(builder.ToString());
        }
        return rendered;
    }

    private static void AppendCell(StringBuilder builder, int column, string text, Func<int, string, string>? styleCell)
    {
        var escaped = CliText.Escape(text);
        builder.Append(styleCell is null ? escaped : styleCell(column, escaped));
    }
}
