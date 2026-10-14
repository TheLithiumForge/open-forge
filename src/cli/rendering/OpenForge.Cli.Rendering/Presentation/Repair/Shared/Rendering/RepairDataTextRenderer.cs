using System.Text;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Repair.Shared.Rendering;

internal static class RepairDataTextRenderer
{
    internal static CliTextDocument Render(RepairData data, CliSelection _, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        if (data.TextRows.Count > 0)
        {
            var rows = new List<IReadOnlyList<string>>();
            void FlushRows()
            {
                if (rows.Count == 0) return;
                spans.Add(new CliTextSpan(CliTable.Render(style, rows, (column, cell) => column == 0 ? style.Subject(cell) : cell)));
                rows.Clear();
            }
            foreach (var row in data.TextRows)
            {
                if (row.IsCommand)
                {
                    FlushRows();
                    spans.Add(new CliTextSpan("  " + CliText.Escape(row.Wording) + "\n"));
                }
                else rows.Add([row.Path, row.Wording]);
            }
            FlushRows();
        }

        var builder = new StringBuilder();
        foreach (var line in data.TextDetailLines)
            builder.Append("  ").Append(CliText.Escape(line)).Append('\n');
        if (data.ShowNoChanges)
            builder.Append((global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged() + "\n"));
        if (builder.Length > 0)
            spans.Add(new CliTextSpan(builder.ToString()));

        return new CliTextDocument(spans);
    }
}
