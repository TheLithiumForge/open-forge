using System.Text;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.List.Shared.Rendering;

internal static class ExtensionListDataTextRenderer
{
    internal static CliTextDocument Render(
        ExtensionListData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);
        var builder = new StringBuilder();
        _ = selection;
        if (data.ShowSource && data.TextSourceLine is { } sourceLine)
        {
            builder.Append(CliText.Escape(sourceLine)).Append('\n');
        }

        if (data.ShowInstalled)
        {
            if (data.Installed.Count == 0)
            {
                if (data.InstalledEmptyLine is { } installedEmpty)
                {
                    builder
                        .Append(CliText.Escape(data.InstalledHeading))
                        .Append("  ")
                        .Append(CliText.Escape(installedEmpty))
                        .Append('\n');
                }
                else
                {
                    builder.Append(CliText.Escape(data.InstalledHeading)).Append('\n');
                }
            }
            else
            {
                builder.Append(CliText.Escape(data.InstalledHeading)).Append('\n');
                AppendRowsWithDetails(
                    builder,
                    data.Installed,
                    row => row.TextCells,
                    row => data.ShowInstalledDetails ? row.TextDetails : [],
                    style);
            }
        }

        if (data.ShowAvailable)
        {
            if (data.ShowInstalled && builder.Length > 0)
            {
                if (builder[^1] != '\n') builder.Append('\n');
                builder.Append('\n');
            }

            if (data.Available.Count == 0)
            {
                if (data.AvailableEmptyLine is { } availableEmpty)
                {
                    builder.Append(CliText.Escape(data.AvailableHeading))
                        .Append("  ")
                        .Append(CliText.Escape(availableEmpty))
                        .Append('\n');
                }
                else
                {
                    builder.Append(CliText.Escape(data.AvailableHeading)).Append('\n');
                }
            }
            else
            {
                builder.Append(CliText.Escape(data.AvailableHeading)).Append('\n');
                AppendRowsWithDetails(
                    builder,
                    data.Available,
                    row => row.TextCells,
                    row => data.ShowAvailableDependencies ? row.TextDetails : [],
                    style);
            }
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }

    private static void AppendRowsWithDetails<T>(
        StringBuilder builder,
        IReadOnlyList<T> rows,
        Func<T, IReadOnlyList<string>> cells,
        Func<T, IReadOnlyList<string>> details,
        CliTextStyle style)
    {
        var table = CliTable.Render(
            rows.Select(cells).ToArray(),
            (column, cell) => column == 0 ? style.Subject(cell) : cell);
        var tableLines = table.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (tableLines.Length != rows.Count)
        {
            throw new InvalidOperationException("The Extension List table did not render one line per row.");
        }

        for (var index = 0; index < rows.Count; index++)
        {
            builder.Append(tableLines[index]).Append('\n');
            foreach (var detail in details(rows[index]))
            {
                builder.Append("    ")
                    .Append(CliText.Escape(detail))
                    .Append('\n');
            }
        }
    }
}
