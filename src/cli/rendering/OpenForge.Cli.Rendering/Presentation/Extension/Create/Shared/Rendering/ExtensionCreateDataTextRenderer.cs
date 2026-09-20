using System.Text;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateDataTextRenderer
{
    internal static CliTextDocument Render(
        ExtensionCreateData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var spans = new List<CliTextSpan>();
        var pathOnlyRows = data.TextRows.Where(row => row.Wording.Length == 0).ToArray();
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

        var rowsWithWording = data.TextRows.Where(row => row.Wording.Length > 0).ToArray();
        if (rowsWithWording.Length > 0)
        {
            var rows = rowsWithWording
                .Select(row => (IReadOnlyList<string>)[row.Path, row.Wording])
                .ToArray();
            spans.Add(new CliTextSpan(CliTable.Render(rows, (column, cell) =>
                column == 0
                    ? style.Subject(cell)
                    : cell)));
        }

        var builder = new StringBuilder();
        foreach (var line in data.TextManifest)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        if (data.TextEditInstruction is { } edit)
        {
            builder.Append("  ")
                .Append(CliText.Escape(edit))
                .Append('\n');
        }

        if (selection.Detail >= CliDetail.Full && data.ManifestContent is { } content)
        {
            builder.Append("  ")
                .Append(CliText.Escape(ExtensionCreateWording.ManifestContent()))
                .Append('\n')
                .Append("  ")
                .Append(CliText.Escape(content))
                .Append('\n');
        }

        if (data.ShowNoChanges)
        {
            builder.Append(CliText.Escape(ExtensionCreateWording.NoChanges()))
                .Append('\n');
        }

        if (builder.Length > 0)
        {
            spans.Add(new CliTextSpan(builder.ToString()));
        }

        return new CliTextDocument(spans);
    }
}
