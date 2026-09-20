using System.Text;
using OpenForge.Cli.Core.Presentation.Cleanup.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Cleanup.Shared.Rendering;

internal static class CleanupDataTextRenderer
{
    internal static CliTextDocument Render(CleanupData data, CliSelection selection, CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var simpleRows = selection.Detail == CliDetail.Minimal
            && data.TextRows.Count > 0
            && data.TextRows.All(row => row.Wording is "removed" or "would be removed")
            && data.TextRows.All(row => row.Detail is null);
        if (simpleRows)
        {
            var paths = new StringBuilder();
            foreach (var row in data.TextRows)
            {
                paths.Append("  ")
                    .Append(style.Subject(CliText.Escape(row.Path)))
                    .Append('\n');
            }

            spans.Add(new CliTextSpan(paths.ToString()));
        }
        else if (data.TextRows.Count > 0)
        {
            var rows = data.TextRows
                .Select(row => (IReadOnlyList<string>)[CliText.Escape(row.Path), CliText.Escape(row.Wording)])
                .ToArray();
            spans.Add(new CliTextSpan(
                CliTable.Render(rows, (column, cell) => column == 0 ? style.Subject(cell) : cell)));

            var details = new StringBuilder();
            foreach (var row in data.TextRows)
            {
                if (row.Detail is not { } detail)
                {
                    continue;
                }

                details.Append("    ")
                    .Append(CliText.Escape(detail))
                    .Append('\n');
            }

            if (details.Length > 0)
            {
                spans.Add(new CliTextSpan(details.ToString()));
            }
        }

        if (data.TextDetailLines.Count > 0 || data.ShowNoChanges)
        {
            var builder = new StringBuilder();
            foreach (var line in data.TextDetailLines)
            {
                builder.Append("  ")
                    .Append(CliText.Escape(line))
                    .Append('\n');
            }

            if (data.ShowNoChanges)
            {
                builder.Append((global::OpenForge.Cli.OutputText.Shared.SharedText.MessageNoFilesWereChanged() + "\n"));
            }

            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }
}
