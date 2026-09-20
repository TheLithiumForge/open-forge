using System.Text;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Presentation.Update.Models;

namespace OpenForge.Cli.Core.Presentation.Update.Shared.Rendering;

internal static class UpdateDataTextRenderer
{
    internal static CliTextDocument Render(UpdateData data, CliSelection _, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var rows = data.TextRows
            .Where(row => row.Path.Length > 0)
            .Select(row => (IReadOnlyList<string>)[row.Path, row.Wording])
            .ToArray();
        if (rows.Length > 0)
        {
            spans.Add(new CliTextSpan(CliTable.Render(rows, (column, cell) =>
                column == 0 ? style.Subject(cell) : cell)));
        }

        var builder = new StringBuilder();
        foreach (var row in data.TextRows.Where(row => row.Path.Length == 0))
        {
            builder.Append("  ").Append(CliText.Escape(row.Wording)).Append('\n');
        }

        foreach (var line in data.TextSummaryLines)
        {
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        }

        foreach (var line in data.TextDetailLines)
        {
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        }

        if (data.ShowNoChanges)
        {
            builder.Append((global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged() + "\n"));
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }
}
