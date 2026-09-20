using System.Text;
using OpenForge.Cli.Core.Presentation.Extension.Install.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Rendering;

internal static class ExtensionInstallDataTextRenderer
{
    internal static CliTextDocument Render(
        ExtensionInstallData data,
        CliSelection _,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var pathOnlyRows = data.TextRows.Where(row => row.Text.Length == 0).ToArray();
        if (pathOnlyRows.Length > 0)
        {
            var paths = new StringBuilder();
            foreach (var row in pathOnlyRows)
            {
                paths.Append("  ")
                    .Append(style.Subject(CliText.Escape(row.Path)))
                    .Append('\n');
            }

            spans.Add(new CliTextSpan(paths.ToString()));
        }

        var rowsWithText = data.TextRows.Where(row => row.Text.Length > 0).ToArray();
        if (rowsWithText.Length > 0)
        {
            var rows = rowsWithText
                .Select(row => (IReadOnlyList<string>)[
                    CliText.Escape(row.Path),
                    CliText.Escape(row.Text),
                ])
                .ToArray();
            var table = CliTable.Render(rows, (column, cell) =>
                column == 0 ? style.Subject(cell) : cell);
            spans.Add(new CliTextSpan(table));
            foreach (var row in rowsWithText.Where(row => row.Detail is not null))
            {
                spans.Add(new CliTextSpan($"    {CliText.Escape(row.Detail!)}\n"));
            }
        }

        var builder = new StringBuilder();
        foreach (var line in data.TextDetails)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        foreach (var line in data.TextNextLines)
        {
            builder.Append(CliText.Escape(line)).Append('\n');
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }
}
