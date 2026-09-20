using System.Text;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;

internal static class InstallDataTextRenderer
{
    internal static CliTextDocument Render(InstallData data, CliSelection _, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var pathOnlyRows = data.TextRows.Where(row => row.Wording.Length == 0).ToArray();
        if (pathOnlyRows.Length > 0)
        {
            var paths = new StringBuilder();
            foreach (var row in pathOnlyRows)
                paths.Append("  ").Append(style.Subject(CliText.Escape(row.Path))).Append('\n');
            spans.Add(new CliTextSpan(paths.ToString()));
        }

        var rowsWithWording = data.TextRows.Where(row => row.Wording.Length > 0).ToArray();
        if (rowsWithWording.Length > 0)
        {
            var rows = rowsWithWording
                .Select(row => (IReadOnlyList<string>)[row.Path, row.Wording])
                .ToArray();
            spans.Add(new CliTextSpan(CliTable.Render(rows, (column, cell) =>
                column == 0 ? style.Subject(cell) : cell)));
        }

        var builder = new StringBuilder();
        foreach (var line in data.TextSummaryLines)
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        foreach (var line in data.TextDetailLines)
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        if (data.ShowNoChanges)
            builder.Append(InstallWording.NoChanges()).Append('\n');
        if (builder.Length > 0)
            spans.Add(new CliTextSpan(builder.ToString()));

        return new CliTextDocument(spans);
    }
}
