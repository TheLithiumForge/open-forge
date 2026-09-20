using System.Text;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Rendering;

internal static class ExtensionUpdateDataTextRenderer
{
    internal static CliTextDocument Render(
        ExtensionUpdateData data,
        CliSelection selection,
        CliTextStyle style)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(style);

        var builder = new StringBuilder();
        if (data.TextRows.Count > 0)
        {
            var rows = data.TextRows
                .Select(row => (IReadOnlyList<string>)[row.Path, row.Text])
                .ToArray();
            builder.Append(CliTable.Render(rows, (column, cell) => column == 0 ? style.Subject(cell) : cell));
            if (selection.Detail >= Shell.Definitions.CliDetail.Standard)
            {
                foreach (var row in data.TextRows.Where(row => row.Detail is not null))
                {
                    builder.Append("    ")
                        .Append(CliText.Escape(row.Detail!))
                        .Append('\n');
                }
            }
        }

        foreach (var line in data.TextDetails)
        {
            builder.Append("  ")
                .Append(CliText.Escape(line))
                .Append('\n');
        }

        if (data.Mode == "dry-run")
        {
            builder.Append(ExtensionUpdateWording.NoChanges()).Append('\n');
        }

        return new CliTextDocument([new CliTextSpan(builder.ToString())]);
    }
}
