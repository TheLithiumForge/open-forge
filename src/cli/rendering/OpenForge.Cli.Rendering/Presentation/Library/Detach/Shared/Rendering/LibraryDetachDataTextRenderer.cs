using System.Text;
using OpenForge.Cli.Core.Presentation.Library.Detach.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Library.Detach.Shared.Rendering;

internal static class LibraryDetachDataTextRenderer
{
    internal static CliTextDocument Render(
        LibraryDetachData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        var rows = data.TextRows
            .Where(row => row.Path is not null)
            .Select(row => (IReadOnlyList<string>)[CliText.Escape(row.Path!), CliText.Escape(row.Text)])
            .ToArray();
        if (rows.Length > 0)
        {
            builder.Append(CliTable.Render(rows, (column, cell) => column == 0 ? style.Subject(cell) : cell));
        }

        foreach (var row in data.TextRows.Where(row => row.Path is null))
        {
            builder.Append("  ")
                .Append(CliText.Escape(row.Text))
                .Append('\n');
        }

        foreach (var line in data.TextDetails)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        return builder.Length == 0
            ? new CliTextDocument([])
            : new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
